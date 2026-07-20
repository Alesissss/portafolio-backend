using Api.Common;

namespace Api.Models
{
    public class Rol : RegistroConEstado
    {
        public Guid IdRol { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Estado { get; set; } = true;
    }
}
