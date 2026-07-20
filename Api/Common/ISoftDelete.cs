namespace Api.Common
{
    // Marca las entidades que participan del borrado lógico (soft delete).
    // El query filter global de PortafolioDbContext se aplica SOLO a las que implementan esta interfaz.
    public interface ISoftDelete
    {
        bool EstadoRegistro { get; set; }
    }
}
