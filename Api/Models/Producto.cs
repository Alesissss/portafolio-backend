using Api.Common;

namespace Api.Models
{
    public class Producto : RegistroBase
    {
        public int IdProducto { get; set; }
        public string IdCategoria { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Stock { get; set; }
        public decimal Precio { get; set; }
        public bool Estado { get; set; } = true;
        public Categoria Categoria { get; set; } = null!;
    }
}
