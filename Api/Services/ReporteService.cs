using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class ReporteService : IReporteService
    {
        private readonly PortafolioDbContext _context;
        public ReporteService(PortafolioDbContext context)
        {
            _context = context;
        }

        // Una venta solo "vendio" cuando salio de borrador y no fue anulada: BO todavia no
        // descuenta stock y AN se revirtio. Ninguno de los dos suma dinero.
        private static readonly string[] EstadosVendidos = ["GEN", "PAG"];

        // Peru no tiene horario de verano, asi que un desfase fijo alcanza. Sin esto una venta
        // de las 20:00 del dia 25 (01:00 UTC del 26) caeria en el dia equivocado al agrupar,
        // porque Npgsql guarda el timestamptz en UTC.
        private const int DesfaseHorasLima = -5;

        // Por encima de este rango la serie diaria se vuelve ilegible: se agrupa por mes.
        private const int DiasMaximoParaSerieDiaria = 62;

        public async Task<ReporteDashboardDto> GetDashboardAsync(ReporteFiltroDto filtro)
        {
            // Base comun a todo: rango de fechas + vendedor. El soft-delete (EstadoRegistro)
            // ya lo aplica el query filter global del DbContext.
            var ventas = _context.Ventas
                .AsNoTracking()
                .Where(v => v.FechaEmision >= filtro.Desde && v.FechaEmision < filtro.Hasta);

            if (filtro.IdVendedor is Guid idVendedor)
                ventas = ventas.Where(v => v.IdVendedor == idVendedor);

            // La torta de estados NO aplica el filtro de estado: si lo hiciera, al hacer clic
            // en una porcion quedaria una sola y el grafico dejaria de servir para volver atras.
            var porEstado = await ventas
                .GroupBy(v => new { v.IdEstadoVenta, v.EstadoVenta.Nombre })
                .Select(g => new VentasPorEstadoDto(
                    g.Key.IdEstadoVenta,
                    g.Key.Nombre,
                    g.Count(),
                    g.Sum(v => v.Total)))
                .ToListAsync();

            // De aqui en adelante si se aplica el filtro de estado.
            if (!string.IsNullOrWhiteSpace(filtro.IdEstadoVenta))
                ventas = ventas.Where(v => v.IdEstadoVenta == filtro.IdEstadoVenta);

            // --- Resumen (KPIs) ---
            // Solo GEN/PAG cuentan como dinero vendido. Si el usuario filtro por BO o AN, el
            // total vendido dara 0 a proposito: un borrador no es una venta.
            var vendidas = ventas.Where(v => EstadosVendidos.Contains(v.IdEstadoVenta));

            var totalVendido = await vendidas.SumAsync(v => (decimal?)v.Total) ?? 0m;
            var numeroVentas = await vendidas.CountAsync();
            var porCobrar = await ventas
                .Where(v => v.IdEstadoVenta == "GEN")
                .SumAsync(v => (decimal?)v.Total) ?? 0m;

            var resumen = new ResumenReporteDto(
                totalVendido,
                numeroVentas,
                numeroVentas == 0 ? 0m : Math.Round(totalVendido / numeroVentas, 2),
                porCobrar);

            // --- Serie de tiempo ---
            var granularidad = (filtro.Hasta - filtro.Desde).TotalDays > DiasMaximoParaSerieDiaria
                ? "mes"
                : "dia";
            var serie = await ConstruirSerieAsync(vendidas, granularidad);

            // --- Top de productos ---
            // Se agrega al nivel del DETALLE (precio x cantidad), no del total de la venta:
            // el total incluye IGV y mezcla todos los productos de la venta.
            // El ORDER BY va sobre la EXPRESION de agregacion, no sobre 'Total' del DTO ya
            // proyectado: EF no sabe volver de una propiedad del record al SUM que la creo y
            // se rinde con "could not be translated". Ordenar y recortar antes de proyectar
            // ademas deja que Postgres haga el ORDER BY ... LIMIT 10 con el agregado.
            var topProductos = await _context.DetallesVenta
                .AsNoTracking()
                .Where(d => vendidas.Select(v => v.IdVenta).Contains(d.IdVenta))
                .GroupBy(d => new { d.IdProducto, d.Producto.Nombre })
                .OrderByDescending(g => g.Sum(d => d.PrecioVenta * d.Cantidad))
                .Take(10)
                .Select(g => new TopProductoDto(
                    g.Key.IdProducto,
                    g.Key.Nombre,
                    g.Sum(d => d.PrecioVenta * d.Cantidad),
                    g.Sum(d => d.Cantidad)))
                .ToListAsync();

            return new ReporteDashboardDto(resumen, granularidad, serie, topProductos, porEstado);
        }

        // Agrupa en la BD por los componentes de la fecha (se traducen a date_part) y recien
        // arma el DateOnly en memoria. Agrupar por 'v.FechaEmision.Date' no se traduce igual
        // en todos los proveedores, por eso se usan Year/Month/Day.
        private async Task<List<PuntoSerieDto>> ConstruirSerieAsync(IQueryable<Venta> ventas, string granularidad)
        {
            if (granularidad == "mes")
            {
                var porMes = await ventas
                    .GroupBy(v => new
                    {
                        v.FechaEmision.AddHours(DesfaseHorasLima).Year,
                        v.FechaEmision.AddHours(DesfaseHorasLima).Month
                    })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Total = g.Sum(v => v.Total),
                        Ventas = g.Count()
                    })
                    .ToListAsync();

                return porMes
                    .Select(x => new PuntoSerieDto(new DateOnly(x.Year, x.Month, 1), x.Total, x.Ventas))
                    .OrderBy(p => p.Fecha)
                    .ToList();
            }

            var porDia = await ventas
                .GroupBy(v => new
                {
                    v.FechaEmision.AddHours(DesfaseHorasLima).Year,
                    v.FechaEmision.AddHours(DesfaseHorasLima).Month,
                    v.FechaEmision.AddHours(DesfaseHorasLima).Day
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    Total = g.Sum(v => v.Total),
                    Ventas = g.Count()
                })
                .ToListAsync();

            return porDia
                .Select(x => new PuntoSerieDto(new DateOnly(x.Year, x.Month, x.Day), x.Total, x.Ventas))
                .OrderBy(p => p.Fecha)
                .ToList();
        }
    }
}
