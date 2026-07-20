namespace Api.Common
{
    // Base para raíces de agregado y entidades independientes: auditoría + borrado lógico.
    public abstract class RegistroConEstado : RegistroBase, ISoftDelete
    {
        public bool EstadoRegistro { get; set; } = true;
    }
}
