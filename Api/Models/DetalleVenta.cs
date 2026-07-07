namespace Api.Models
{
    public class DetalleVenta
    {
        public Guid IdVenta { get; set; }
        public int IdProducto { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Cantidad { get; set; }
        public string? Observacion { get; set; }
        public Venta Venta { get; set; } = null!;
        public Producto Producto { get; set; } = null!;
    }
}
