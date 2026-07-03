namespace Api.Common
{
    public abstract class RegistroBase
    {
        public Guid? UsuarioRegistro { get; set; }
        public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;
        public bool EstadoRegistro { get; set; } = true;
    }
}
