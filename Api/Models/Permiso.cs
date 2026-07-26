using Api.Common;

namespace Api.Models
{
    public class Permiso : RegistroConEstado
    {
        public string IdPermiso { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public byte Orden { get; set; }
    }
}
