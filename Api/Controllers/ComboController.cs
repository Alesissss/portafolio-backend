using Api.Common;
using Api.Dtos;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    // Endpoints de "combos": datos mínimos (id + texto) para poblar selects.
    // Solo requieren estar autenticado ([Authorize]); NO escalan a permisos de CRUD, para que
    // cualquier pantalla (ej. registrar producto) pueda cargar sus selects sin depender de los
    // permisos del módulo dueño de esos datos.
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ComboController(IComboService _comboService) : ControllerBase
    {
        // Categorías activas para selects
        [HttpGet("categorias")]
        [ProducesResponseType(typeof(ApiResponse<List<ComboDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<ComboDto>>>> GetCategoriasCombo()
        {
            var data = await _comboService.GetCategoriasComboAsync();
            return Ok(ApiResponse<List<ComboDto>>.Success(data, "Categorías para select listadas correctamente"));
        }

        // Roles activos para selects
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ApiResponse<List<ComboDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<ComboDto>>>> GetRolesCombo()
        {
            var data = await _comboService.GetRolesComboAsync();
            return Ok(ApiResponse<List<ComboDto>>.Success(data, "Roles para select listados correctamente"));
        }

        // Productos activos (con precio) para el select de la venta
        [HttpGet("productos")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductoComboDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<ProductoComboDto>>>> GetProductosCombo()
        {
            var data = await _comboService.GetProductosComboAsync();
            return Ok(ApiResponse<List<ProductoComboDto>>.Success(data, "Productos para select listados correctamente"));
        }

        // Vendedores (usuarios con rol Vendedor) para el select de la venta
        [HttpGet("vendedores")]
        [ProducesResponseType(typeof(ApiResponse<List<ComboDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<ComboDto>>>> GetVendedoresCombo()
        {
            var data = await _comboService.GetVendedoresComboAsync();
            return Ok(ApiResponse<List<ComboDto>>.Success(data, "Vendedores para select listados correctamente"));
        }
    }
}
