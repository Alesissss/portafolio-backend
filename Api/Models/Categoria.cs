using Api.Common;

namespace Api.Models
{
    public class Categoria : RegistroConEstado
    {
        public string IdCategoria { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; } = true;
    }
}
