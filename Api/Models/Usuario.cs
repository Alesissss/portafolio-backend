using Api.Common;

namespace Api.Models
{
    public class Usuario : RegistroBase
    {
        public Guid IdUsuario {  get; set; }
        public Guid IdRol { get; set; }
        public string ApellidoPaterno { get; set; } = null!;
        public string ApellidoMaterno { get; set; } = null!;
        public string Nombres { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool Estado { get; set; } = true;
        public Rol Rol { get; set; } = null!;
    }
}
