using Api.Dtos;

namespace Api.Services.Interfaces
{
    public interface IVentaService
    {
        public Task<List<VentaDto>> GetVentasAsync();
    }
}
