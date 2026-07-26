namespace Api.Common
{
    // Campos de auditoría por fila. NO incluye borrado lógico:
    // el soft delete vive en ISoftDelete y solo lo llevan las entidades que son
    // raíz de su agregado o independientes. Los hijos de un agregado (ej. DetalleVenta)
    // heredan solo esto y se borran físicamente; su historial ya queda en AuditoriaLog.
    public abstract class RegistroBase
    {
        public Guid? UsuarioRegistro { get; set; }
        public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;
    }
}
