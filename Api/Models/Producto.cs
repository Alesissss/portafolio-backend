using Api.Common;

namespace Api.Models
{
    public class Producto : RegistroConEstado
    {
        public int IdProducto { get; set; }
        public string IdCategoria { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Stock { get; set; }
        public decimal Precio { get; set; }
        public bool Estado { get; set; } = true;
        public string? ArchivoFoto { get; set; }
        public Categoria Categoria { get; set; } = null!;
    }
}
