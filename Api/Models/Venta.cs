using Api.Common;

namespace Api.Models
{
    public class Venta : RegistroBase
    {
        public Guid IdVenta { get; set; }
        public DateTimeOffset FechaEmision {  get; set; } = DateTimeOffset.UtcNow;
        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
        public Guid IdVendedor { get; set; }
        public string IdEstadoVenta { get; set; } = null!;
        public EstadoVenta EstadoVenta { get; set; } = null!;
        public Usuario Vendedor { get; set; } = null!;
        public List<DetalleVenta> Detalles { get; set; } = new();
    }
}
