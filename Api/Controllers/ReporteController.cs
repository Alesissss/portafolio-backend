using Api.Common;
using Api.Dtos;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    // Reportes: solo lectura y agregado. Hoy basta con [Authorize]; cuando exista el RBAC
    // este es el candidato natural a una policy propia (ver quien vende cuanto no es algo
    // que deba ver cualquier vendedor).
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReporteController(IReporteService _reporteService) : ControllerBase
    {
        // Rango por defecto cuando el front no manda fechas.
        private const int DiasPorDefecto = 30;

        // Todo el dashboard en una sola llamada, para que los graficos no puedan mostrar
        // tajadas distintas de los datos.
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<ReporteDashboardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ReporteDashboardDto>>> GetDashboard(
            [FromQuery] DateTimeOffset? desde,
            [FromQuery] DateTimeOffset? hasta,
            [FromQuery] string? estado,
            [FromQuery] Guid? vendedor)
        {
            var hastaFinal = hasta ?? DateTimeOffset.UtcNow;
            var desdeFinal = desde ?? hastaFinal.AddDays(-DiasPorDefecto);

            if (desdeFinal >= hastaFinal)
                return BadRequest(ApiResponse<object>.Fail("La fecha inicial debe ser anterior a la final."));

            var filtro = new ReporteFiltroDto(desdeFinal, hastaFinal, estado, vendedor);
            var data = await _reporteService.GetDashboardAsync(filtro);

            return Ok(ApiResponse<ReporteDashboardDto>.Success(data, "Reporte generado correctamente"));
        }
    }
}
