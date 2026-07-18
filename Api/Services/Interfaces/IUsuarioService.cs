using Api.Dtos;

namespace Api.Services.Interfaces
{
    public interface IUsuarioService
    {
        public Task<List<UsuarioDto>> ListarUsuariosAsync();
        public Task<UsuarioResult> ObtenerUnUsuarioAsync(Guid id);
        public Task<UsuarioResultType> RegistrarUsuarioAsync(RegistrarRequestUsuarioDto dto);
        public Task<UsuarioResultType> EditarUsuarioAsync(EditarRequestUsuarioDto dto);
        public Task<UsuarioResultType> DarBajaUsuarioAsync(Guid id);
        public Task<UsuarioResultType> EliminarUsuarioAsync(Guid id);
    }
}
