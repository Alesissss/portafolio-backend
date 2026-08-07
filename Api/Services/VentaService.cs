using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class VentaService : IVentaService
    {
        private readonly PortafolioDbContext _context;
        private readonly IFileService _fileService;

        public VentaService(PortafolioDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // Listar todas las ventas con su vendedor, su estado y sus detalles (+ nombre de producto).
        public async Task<PaginacionResponseDto<VentaDto>> GetVentasAsync(int pagina, int registrosPorPagina, string? search)
        {
            if (pagina < 1) pagina = 1;
            if (registrosPorPagina < 1) registrosPorPagina = 10;

            // Creamos la consulta
            var query = _context.Ventas
                .Include(v => v.Vendedor)
                .Include(v => v.EstadoVenta)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .AsNoTracking()
                .AsQueryable();

            // Buscar por el campo enviado  
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower().Trim();
                query = query.Where(v =>
                    v.IdVenta.ToString().Contains(searchLower) ||
                    v.Vendedor.Nombres.ToLower().Contains(searchLower) ||
                    v.Vendedor.ApellidoPaterno.ToLower().Contains(searchLower) ||
                    v.Vendedor.ApellidoMaterno.ToLower().Contains(searchLower) ||
                    v.EstadoVenta.Descripcion.ToLower().Contains(searchLower) ||
                    v.Total.ToString().Contains(searchLower)
                );
            }

            var totalRegistros = await query.CountAsync();

            var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            var ventas = await query
                .OrderByDescending(v => v.FechaEmision)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            var elementosDto = ventas.Select(VentaToDto).ToList();

            return new PaginacionResponseDto<VentaDto>
            (
                TotalRegistros: totalRegistros,
                PaginaActual: pagina,
                RegistrosPorPagina: registrosPorPagina,
                TotalPaginas: totalPaginas,
                Elementos: elementosDto
            );
        }

        // Ver datos de una venta y sus detalles
        public async Task<VentaResult> ObtenerUnaVentaAsync(Guid id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Vendedor)
                .Include(v => v.EstadoVenta)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null) return new VentaResult(VentaResultType.NoEncontrada);

            return new VentaResult(VentaResultType.Ok, VentaToDto(venta));
        }
        public async Task<VentaResultType> RegistrarVentaAsync(RegistrarRequestVentaDto dto)
        {
            var ids = dto.Detalles.Select(d => d.IdProducto).Distinct().ToList();
            var productos = await _context.Productos
                .Where(p => ids.Contains(p.IdProducto))
                .ToDictionaryAsync(p => p.IdProducto);

            var venta = new Venta
            {
                FechaEmision = DateTimeOffset.UtcNow,
                IdVendedor = dto.IdVendedor,
                IdEstadoVenta = "BO",
            };

            decimal subtotal = 0;
            foreach (var detalle in dto.Detalles)
            {
                if (!productos.TryGetValue(detalle.IdProducto, out var producto))
                    return VentaResultType.ProductoNoExiste;

                venta.Detalles.Add(new DetalleVenta
                {
                    IdProducto = detalle.IdProducto,
                    PrecioVenta = producto.Precio,
                    Cantidad = detalle.Cantidad,
                    Observacion = detalle.Observacion,
                });
                subtotal += producto.Precio * detalle.Cantidad;
            }

            venta.Subtotal = subtotal;
            venta.Igv = subtotal * 0.18m;
            venta.Total = venta.Subtotal + venta.Igv;

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();
            return VentaResultType.Ok;
        }

        // Editar una venta en Borrador reconciliando sus detalles contra la BD (alta / modificación / baja),
        // en vez de borrar todo y reinsertar. Solo toca lo que realmente cambió.
        public async Task<VentaResultType> EditarVentaAsync(EditarRequestVentaDto dto)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)   // TRACKED (sin AsNoTracking) para poder modificarlos
                .FirstOrDefaultAsync(v => v.IdVenta == dto.IdVenta);

            if (venta is null) return VentaResultType.NoEncontrada;
            if (venta.IdEstadoVenta != "BO") return VentaResultType.NoEditable;   // solo borradores

            // Actualizar vendedor
            venta.IdVendedor = dto.IdVendedor;

            var ids = dto.Detalles.Select(d => d.IdProducto).Distinct().ToList();
            var productos = await _context.Productos
                .Where(p => ids.Contains(p.IdProducto))
                .ToDictionaryAsync(p => p.IdProducto);

            var existentes = venta.Detalles.ToDictionary(d => d.IdProducto);   // lo que YA hay, por producto
            var idsEntrantes = ids.ToHashSet();

            // 1) altas y modificaciones: recorro lo que manda el cliente
            foreach (var detalle in dto.Detalles)
            {
                if (!productos.TryGetValue(detalle.IdProducto, out var producto))
                    return VentaResultType.ProductoNoExiste;

                if (existentes.TryGetValue(detalle.IdProducto, out var existente))
                {
                    existente.Cantidad = detalle.Cantidad;         // ya estaba -> actualizo
                    existente.Observacion = detalle.Observacion;
                    existente.PrecioVenta = producto.Precio;       // re-leo por si el precio cambió
                }
                else
                {
                    venta.Detalles.Add(new DetalleVenta            // es nuevo -> alta
                    {
                        IdProducto = detalle.IdProducto,
                        PrecioVenta = producto.Precio,
                        Cantidad = detalle.Cantidad,
                        Observacion = detalle.Observacion,
                    });
                }
            }

            // 2) bajas: lo que estaba y ya no llegó (borrado físico: es un borrador, sin referencias)
            var eliminados = venta.Detalles
                .Where(d => !idsEntrantes.Contains(d.IdProducto))
                .ToList();
            _context.DetallesVenta.RemoveRange(eliminados);

            // 3) totales: tras reconciliar, los vigentes son exactamente los que llegaron
            var subtotal = dto.Detalles.Sum(d => productos[d.IdProducto].Precio * d.Cantidad);
            venta.Subtotal = subtotal;
            venta.Igv = subtotal * 0.18m;
            venta.Total = venta.Subtotal + venta.Igv;

            await _context.SaveChangesAsync();
            return VentaResultType.Ok;
        }

        // Eliminar una venta: solo borradores, soft delete de la cabecera (Venta es raíz de agregado).
        public async Task<VentaResultType> EliminarVentaAsync(Guid id)
        {
            var venta = await _context.Ventas.FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta is null) return VentaResultType.NoEncontrada;
            if (venta.IdEstadoVenta != "BO") return VentaResultType.EstadoInvalido;

            venta.EstadoRegistro = false;   // soft delete
            await _context.SaveChangesAsync();
            return VentaResultType.Ok;
        }

        // Generar (BO -> GEN): descuenta stock. El pago queda pendiente. No toca comprobante.
        public async Task<VentaResultType> GenerarVentaAsync(Guid id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta is null) return VentaResultType.NoEncontrada;
            if (venta.IdEstadoVenta != "BO") return VentaResultType.EstadoInvalido;

            var ids = venta.Detalles.Select(d => d.IdProducto).ToList();
            var productos = await _context.Productos
                .Where(p => ids.Contains(p.IdProducto))
                .ToDictionaryAsync(p => p.IdProducto);

            // 1) valido TODO el stock antes de tocar nada (todo o nada)
            foreach (var detalle in venta.Detalles)
            {
                if (!productos.TryGetValue(detalle.IdProducto, out var producto))
                    return VentaResultType.ProductoNoExiste;
                if (producto.Stock < detalle.Cantidad)
                    return VentaResultType.StockInsuficiente;
            }

            // 2) recién ahora descuento
            foreach (var detalle in venta.Detalles)
                productos[detalle.IdProducto].Stock -= detalle.Cantidad;

            venta.IdEstadoVenta = "GEN";
            await _context.SaveChangesAsync();
            return VentaResultType.Ok;
        }

        // Pagar (GEN -> PAG): registra el comprobante y cierra la venta. Estado irreversible.
        public async Task<VentaResultType> PagarVentaAsync(Guid id, IFormFile comprobante)
        {
            if (comprobante is null || comprobante.Length == 0)
                return VentaResultType.ArchivoRequerido;

            var venta = await _context.Ventas.FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta is null) return VentaResultType.NoEncontrada;
            if (venta.IdEstadoVenta != "GEN") return VentaResultType.EstadoInvalido;

            // Valido el estado ANTES de escribir en disco: así no quedan comprobantes huérfanos
            // de ventas que no correspondían.
            venta.ArchivoPago = await _fileService.GuardarPrivadoAsync(comprobante, "comprobantes");
            venta.IdEstadoVenta = "PAG";

            await _context.SaveChangesAsync();
            return VentaResultType.Ok;
        }

        // Anular (GEN -> AN): devuelve el stock que se descontó al generar. Estado irreversible.
        // La restricción a administradores se aplica en el controller (autorización), no aquí.
        public async Task<VentaResultType> AnularVentaAsync(Guid id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta is null) return VentaResultType.NoEncontrada;
            if (venta.IdEstadoVenta != "GEN") return VentaResultType.EstadoInvalido;

            var ids = venta.Detalles.Select(d => d.IdProducto).ToList();
            var productos = await _context.Productos
                .Where(p => ids.Contains(p.IdProducto))
                .ToDictionaryAsync(p => p.IdProducto);

            foreach (var detalle in venta.Detalles)
                if (productos.TryGetValue(detalle.IdProducto, out var producto))
                    producto.Stock += detalle.Cantidad;   // regresa lo descontado

            venta.IdEstadoVenta = "AN";
            await _context.SaveChangesAsync();
            return VentaResultType.Ok;
        }

        // Devuelve el comprobante de pago para que el controller lo sirva.
        // Es PRIVADO a propósito: vive fuera de wwwroot, así que la única puerta de entrada
        // es el endpoint con [Authorize]; no hay URL adivinable que lo exponga.
        public async Task<ComprobanteResult> ObtenerComprobanteAsync(Guid id)
        {
            var venta = await _context.Ventas
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta is null) return new ComprobanteResult(VentaResultType.NoEncontrada);
            if (string.IsNullOrWhiteSpace(venta.ArchivoPago))
                return new ComprobanteResult(VentaResultType.ArchivoRequerido);

            var archivo = _fileService.AbrirPrivado(venta.ArchivoPago);
            // La fila apunta a un archivo que ya no está en disco.
            if (archivo is null) return new ComprobanteResult(VentaResultType.NoEncontrada);

            // Nombre amigable para la descarga; el del disco es un Guid sin significado.
            var extension = Path.GetExtension(venta.ArchivoPago);
            var nombreDescarga = $"comprobante-{venta.IdVenta}{extension}";

            return new ComprobanteResult(
                VentaResultType.Ok,
                archivo.Value.Contenido,
                archivo.Value.TipoContenido,
                nombreDescarga);
        }

        private static VentaDto VentaToDto(Venta v) =>
            new VentaDto(
                IdVenta: v.IdVenta,
                FechaEmision: v.FechaEmision,
                Subtotal: v.Subtotal,
                Igv: v.Igv,
                Total: v.Total,
                IdVendedor: v.IdVendedor,
                NombreVendedor: $"{v.Vendedor.Nombres} {v.Vendedor.ApellidoPaterno}",
                IdEstadoVenta: v.IdEstadoVenta,
                NombreEstadoVenta: v.EstadoVenta.Nombre,
                Detalles: v.Detalles.Select(DetalleToDto).ToList()
            );

        private static DetalleVentaDto DetalleToDto(DetalleVenta d) =>
            new DetalleVentaDto(
                IdProducto: d.IdProducto,
                NombreProducto: d.Producto.Nombre,
                PrecioVenta: d.PrecioVenta,
                Cantidad: d.Cantidad,
                Observacion: d.Observacion
            );
    }
}
