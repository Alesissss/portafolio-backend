using Api.Dtos;

namespace Api.Services.Interfaces
{
    public interface IProductoService
    {
        public Task<List<ProductoDto>> GetProductosAsync();
        public Task<ProductoResult> ObtenerUnProductoAsync(int id);
        // La foto es opcional (null = sin foto al registrar / conservar la actual al editar).
        public Task<ProductoResultType> RegistrarProductoAsync(RegistrarRequestProductoDto dto, IFormFile? foto);
        public Task<ProductoResultType> EditarProductoAsync(ProductoDto dto, IFormFile? foto);
        public Task<ProductoResultType> DarBajaProductoAsync(int id);
        public Task<ProductoResultType> EliminarProductoAsync(int id);
    }
}
