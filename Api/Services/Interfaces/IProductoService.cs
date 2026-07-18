using Api.Dtos;

namespace Api.Services.Interfaces
{
    public interface IProductoService
    {
        public Task<List<ProductoDto>> GetProductosAsync();
        public Task<ProductoResult> ObtenerUnProductoAsync(int id);
        public Task<ProductoResultType> RegistrarProductoAsync(RegistrarRequestProductoDto dto);
        public Task<ProductoResultType> EditarProductoAsync(ProductoDto dto);
        public Task<ProductoResultType> DarBajaProductoAsync(int id);
        public Task<ProductoResultType> EliminarProductoAsync(int id);
    }
}
