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
    public class ProductoController(IProductoService _productoService) : ControllerBase
    {
        // Listar todos
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ProductoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<ProductoDto>>>> ListarProductos()
        {
            var productos = await _productoService.GetProductosAsync();
            return Ok(ApiResponse<List<ProductoDto>>.Success(productos, "Productos listados correctamente"));
        }

        // Listar uno
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductoDto>>> ListarUnProducto(int id)
        {
            var result = await _productoService.ObtenerUnProductoAsync(id);

            return result.Estado switch
            {
                ProductoResultType.Ok =>
                    Ok(ApiResponse<ProductoDto>.Success(result.Data!, "Producto listado correctamente")),
                ProductoResultType.NoExiste =>
                    NotFound(ApiResponse<object>.Fail("El producto buscado no existe.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la búsqueda del producto. Por favor, contacte al administrador."))
            };
        }

        // Registrar
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        // multipart/form-data en vez de JSON: es el único content type que puede llevar un
        // archivo junto a los campos. `foto` es opcional; si no viene, el producto no lleva imagen.
        public async Task<ActionResult<ApiResponse<object>>> RegistrarProducto([FromForm] RegistrarRequestProductoDto dto, IFormFile? foto)
        {
            var result = await _productoService.RegistrarProductoAsync(dto, foto);
            return result switch
            {
                ProductoResultType.Ok =>
                    StatusCode(201, ApiResponse<object>.Success("Producto registrado correctamente")),
                ProductoResultType.NombreRepetido =>
                    BadRequest(ApiResponse<object>.Fail("El nombre del producto ya está en uso. Por favor, elija otro.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante el registro del producto. Por favor, contacte al administrador."))
            };
        }

        // Editar
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        // Sin `foto` en el form, la imagen actual se conserva.
        public async Task<ActionResult<ApiResponse<object>>> EditarProducto([FromForm] ProductoDto dto, IFormFile? foto)
        {
            var result = await _productoService.EditarProductoAsync(dto, foto);
            return result switch
            {
                ProductoResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Producto editado correctamente")),
                ProductoResultType.NoExiste =>
                    NotFound(ApiResponse<object>.Fail("El producto que intenta editar no existe.")),
                ProductoResultType.NombreRepetido =>
                    BadRequest(ApiResponse<object>.Fail("El nombre del producto ya está en uso por otro producto. Por favor, elija otro.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la edición del producto. Por favor, contacte al administrador."))
            };
        }

        // Dar baja
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DarBajaProducto(int id)
        {
            var result = await _productoService.DarBajaProductoAsync(id);
            return result switch
            {
                ProductoResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Producto dado de baja correctamente")),
                ProductoResultType.NoExiste =>
                    NotFound(ApiResponse<object>.Fail("El producto que intenta dar de baja no existe.")),
                ProductoResultType.YaEsBaja =>
                    BadRequest(ApiResponse<object>.Fail("El producto ya está dado de baja.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la operación. Por favor, contacte al administrador."))
            };
        }

        // Eliminar
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> EliminarProducto(int id)
        {
            var result = await _productoService.EliminarProductoAsync(id);
            return result switch
            {
                ProductoResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Producto eliminado correctamente")),
                ProductoResultType.NoExiste =>
                    NotFound(ApiResponse<object>.Fail("El producto que intenta eliminar no existe.")),
                ProductoResultType.ExisteReferencia =>
                    BadRequest(ApiResponse<object>.Fail("No se puede eliminar el producto porque existen referencias a él. Por favor, elimine primero las referencias.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la eliminación del producto. Por favor, contacte al administrador."))
            };
        }
    }
}
