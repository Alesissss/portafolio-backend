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

        // Listar una
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<VentaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VentaDto>>> ListarUnaVenta(Guid id)
        {
            var result = await _ventaService.ObtenerUnaVentaAsync(id);
            return result.Estado switch
            {
                VentaResultType.Ok =>
                    Ok(ApiResponse<VentaDto>.Success(result.Data!, "Venta obtenida correctamente")),
                VentaResultType.NoEncontrada =>
                    NotFound(ApiResponse<object>.Fail("La venta buscada no existe.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la búsqueda de la venta. Por favor, contacte al administrador."))
            };
        }

        // Registrar (queda en estado Borrador)
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RegistrarVenta([FromBody] RegistrarRequestVentaDto dto)
        {
            var result = await _ventaService.RegistrarVentaAsync(dto);
            return result switch
            {
                VentaResultType.Ok =>
                    StatusCode(201, ApiResponse<object>.Success("Venta registrada correctamente")),
                VentaResultType.ProductoNoExiste =>
                    BadRequest(ApiResponse<object>.Fail("Uno de los productos de la venta no existe.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante el registro de la venta. Por favor, contacte al administrador."))
            };
        }

        // Editar (solo borradores)
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> EditarVenta([FromBody] EditarRequestVentaDto dto)
        {
            var result = await _ventaService.EditarVentaAsync(dto);
            return result switch
            {
                VentaResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Venta editada correctamente")),
                VentaResultType.NoEncontrada =>
                    NotFound(ApiResponse<object>.Fail("La venta que intenta editar no existe.")),
                VentaResultType.NoEditable =>
                    BadRequest(ApiResponse<object>.Fail("Solo se pueden editar ventas en estado Borrador.")),
                VentaResultType.ProductoNoExiste =>
                    BadRequest(ApiResponse<object>.Fail("Uno de los productos de la venta no existe.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la edición de la venta. Por favor, contacte al administrador."))
            };
        }

        // Eliminar (soft delete, solo borradores)
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> EliminarVenta(Guid id)
        {
            var result = await _ventaService.EliminarVentaAsync(id);
            return result switch
            {
                VentaResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Venta eliminada correctamente")),
                VentaResultType.NoEncontrada =>
                    NotFound(ApiResponse<object>.Fail("La venta que intenta eliminar no existe.")),
                VentaResultType.EstadoInvalido =>
                    BadRequest(ApiResponse<object>.Fail("Solo se pueden eliminar ventas en estado Borrador.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la eliminación de la venta. Por favor, contacte al administrador."))
            };
        }

        // Generar: Borrador -> Generada (descuenta stock)
        [HttpPatch("{id:guid}/generar")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> GenerarVenta(Guid id)
        {
            var result = await _ventaService.GenerarVentaAsync(id);
            return result switch
            {
                VentaResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Venta generada correctamente")),
                VentaResultType.NoEncontrada =>
                    NotFound(ApiResponse<object>.Fail("La venta que intenta generar no existe.")),
                VentaResultType.EstadoInvalido =>
                    BadRequest(ApiResponse<object>.Fail("Solo se pueden generar ventas en estado Borrador.")),
                VentaResultType.StockInsuficiente =>
                    BadRequest(ApiResponse<object>.Fail("No hay stock suficiente para uno o más productos de la venta.")),
                VentaResultType.ProductoNoExiste =>
                    BadRequest(ApiResponse<object>.Fail("Uno de los productos de la venta no existe.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado al generar la venta. Por favor, contacte al administrador."))
            };
        }

        // Pagar: Generada -> Pagada (requiere el comprobante; estado irreversible)
        [HttpPatch("{id:guid}/pagar")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> PagarVenta(Guid id, IFormFile comprobante)
        {
            var result = await _ventaService.PagarVentaAsync(id, comprobante);
            return result switch
            {
                VentaResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Venta pagada correctamente")),
                VentaResultType.NoEncontrada =>
                    NotFound(ApiResponse<object>.Fail("La venta que intenta pagar no existe.")),
                VentaResultType.EstadoInvalido =>
                    BadRequest(ApiResponse<object>.Fail("Solo se pueden pagar ventas en estado Generada.")),
                VentaResultType.ArchivoRequerido =>
                    BadRequest(ApiResponse<object>.Fail("Debe adjuntar el comprobante de pago.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado al pagar la venta. Por favor, contacte al administrador."))
            };
        }

        // Anular: Generada -> Anulada (devuelve stock; estado irreversible)
        // TODO RBAC: restringir a administradores vía policy cuando exista (ej. [Authorize(Policy = "Ventas.Anular")]).
        [HttpPatch("{id:guid}/anular")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> AnularVenta(Guid id)
        {
            var result = await _ventaService.AnularVentaAsync(id);
            return result switch
            {
                VentaResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Venta anulada correctamente")),
                VentaResultType.NoEncontrada =>
                    NotFound(ApiResponse<object>.Fail("La venta que intenta anular no existe.")),
                VentaResultType.EstadoInvalido =>
                    BadRequest(ApiResponse<object>.Fail("Solo se pueden anular ventas en estado Generada.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado al anular la venta. Por favor, contacte al administrador."))
            };
        }
    }
}
