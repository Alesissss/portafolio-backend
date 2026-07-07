namespace Api.Models
{
    public class AuditoriaLog
    {
        public int IdLog { get; set; }
        public string NombreTabla { get; set; } = null!;
        public string Operacion { get; set; } = null!;
        public string? IdRegistro { get; set; }
        public string? RegistroAnterior { get; set; }
        public string? RegistroNuevo { get; set; }
        public Guid? UsuarioAccion { get; set; }
        public string UsuarioBd { get; set; } = null!;
        public DateTimeOffset FechaAccion { get; set; } = DateTimeOffset.UtcNow;
    }
}
