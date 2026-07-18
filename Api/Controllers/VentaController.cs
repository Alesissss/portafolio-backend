using Api.Common;
using Api.Dtos;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController(IVentaService _ventaService) : ControllerBase
    {
        // Listar todas
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<VentaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<VentaDto>>>> ListarVentas()
        {
            var ventas = await _ventaService.GetVentasAsync();
            return Ok(ApiResponse<List<VentaDto>>.Success(ventas, "Ventas listadas correctamente"));
        }
    }
}
