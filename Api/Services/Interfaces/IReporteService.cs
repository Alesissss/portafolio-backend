using Api.Dtos;

namespace Api.Services.Interfaces
{
    // Contrato del modulo de Reportes. Solo lectura: agrega y proyecta, nunca escribe.
    public interface IReporteService
    {
        public Task<ReporteDashboardDto> GetDashboardAsync(ReporteFiltroDto filtro);
    }
}
