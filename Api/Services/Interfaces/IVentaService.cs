using Api.Dtos;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Interfaces
{
    public interface IVentaService
    {
        public Task<List<VentaDto>> GetVentasAsync();
        public Task<VentaResult> ObtenerUnaVentaAsync(Guid id);
        public Task<VentaResultType> RegistrarVentaAsync(RegistrarRequestVentaDto dto);
        public Task<VentaResultType> EditarVentaAsync(EditarRequestVentaDto dto);
        public Task<VentaResultType> EliminarVentaAsync(Guid id);
        public Task<VentaResultType> GenerarVentaAsync(Guid id);
        public Task<VentaResultType> PagarVentaAsync(Guid id, IFormFile comprobante);
        public Task<VentaResultType> AnularVentaAsync(Guid id);
    }
}
