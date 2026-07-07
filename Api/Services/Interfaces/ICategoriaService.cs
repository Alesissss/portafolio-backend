using Api.Dtos;

namespace Api.Services.Interfaces
{
    public interface ICategoriaService
    {
        public Task<List<CategoriaDto>> ListarCategoriasAsync();
        public Task<CategoriaResult> ObtenerUnaCategoriaAsync(string id);
        public Task<CategoriaResultType> RegistrarCategoriaAsync(RegistrarRequestCategoriaDto dto);
        public Task<CategoriaResultType> EditarCategoriaAsync(CategoriaDto dto);
        public Task<CategoriaResultType> DarBajaCategoriaAsync(string id);
        public Task<CategoriaResultType> EliminarCategoriaAsync(string id);
    }
}
