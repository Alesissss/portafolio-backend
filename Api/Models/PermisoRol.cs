using Api.Common;

namespace Api.Models
{
    public class PermisoRol : RegistroBase
    {
        public string IdPermiso { get; set; } = null!;
        public Guid IdRol { get; set; }
        public Permiso Permiso { get; set; } = null!;
        public Rol Rol { get; set; } = null!;
    }
}
