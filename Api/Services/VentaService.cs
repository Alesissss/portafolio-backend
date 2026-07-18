using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class VentaService : IVentaService
    {
        private readonly PortafolioDbContext _context;
        public VentaService(PortafolioDbContext context)
        {
            _context = context;
        }

        // Listar todas las ventas con su vendedor, su estado y sus detalles (+ nombre de producto).
        public async Task<List<VentaDto>> GetVentasAsync()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Vendedor)
                .Include(v => v.EstadoVenta)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .AsNoTracking()
                .OrderByDescending(v => v.FechaEmision)
                .ToListAsync();

            return ventas.Select(VentaToDto).ToList();
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
