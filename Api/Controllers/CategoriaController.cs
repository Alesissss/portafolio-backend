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
    public class CategoriaController(ICategoriaService _categoriaService) : ControllerBase
    {
        // Listar todos
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CategoriaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<CategoriaDto>>>> ListarCategorias()
        {
            var categorias = await _categoriaService.ListarCategoriasAsync();
            return Ok(ApiResponse<List<CategoriaDto>>.Success(categorias, "Categorías listadas correctamente"));
        }

        // Listar uno
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoriaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CategoriaDto>>> ListarUnaCategoria(string id)
        {
            var result = await _categoriaService.ObtenerUnaCategoriaAsync(id);

            return result.Estado switch
            {
                CategoriaResultType.Ok =>
                    Ok(ApiResponse<CategoriaDto>.Success(result.Data!, "Categoría listada correctamente")),
                CategoriaResultType.NoExiste =>
                    NotFound(ApiResponse<object>.Fail("La categoría buscada no existe.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la búsqueda de la categoría. Por favor, contacte al administrador."))
            };
        }

        // Registrar
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RegistrarCategoria([FromBody] RegistrarRequestCategoriaDto dto)
        {
            var result = await _categoriaService.RegistrarCategoriaAsync(dto);
            return result switch
            {
                CategoriaResultType.Ok =>
                    StatusCode(201, ApiResponse<object>.Success("Categoría registrada correctamente")),
                CategoriaResultType.IdRepetido =>
                    BadRequest(ApiResponse<object>.Fail("El ID de la categoría ya está en uso. Por favor, elija otro.")),
                CategoriaResultType.NombreRepetido =>
                    BadRequest(ApiResponse<object>.Fail("El nombre de la categoría ya está en uso. Por favor, elija otro.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante el registro de la categoría. Por favor, contacte al administrador."))
            };
        }

        // Editar
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> EditarCategoria([FromBody] CategoriaDto dto)
        {
            var result = await _categoriaService.EditarCategoriaAsync(dto);
            return result switch
            {
                CategoriaResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Categoría editada correctamente")),
                CategoriaResultType.NoExiste =>
                    NotFound(ApiResponse<object>.Fail("La categoría que intenta editar no existe.")),
                CategoriaResultType.NombreRepetido =>
                    BadRequest(ApiResponse<object>.Fail("El nombre de la categoría ya está en uso por otra categoría. Por favor, elija otro.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la edición de la categoría. Por favor, contacte al administrador."))
            };
        }

        // Dar baja
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DarBajaCategoria(string id)
        {
            var result = await _categoriaService.DarBajaCategoriaAsync(id);
            return result switch
            {
                CategoriaResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Categoría dada de baja correctamente")),
                CategoriaResultType.NoExiste =>
                    NotFound(ApiResponse<object>.Fail("La categoría que intenta dar de baja no existe.")),
                CategoriaResultType.YaEsBaja =>
                    BadRequest(ApiResponse<object>.Fail("La categoría ya está dada de baja.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la operación. Por favor, contacte al administrador."))
            };
        }

        // Eliminar
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> EliminarCategoria(string id)
        {
            var result = await _categoriaService.EliminarCategoriaAsync(id);
            return result switch
            {
                CategoriaResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Categoría eliminada correctamente")),
                CategoriaResultType.NoExiste =>
                    NotFound(ApiResponse<object>.Fail("La categoría que intenta eliminar no existe.")),
                CategoriaResultType.ExisteReferencia =>
                    BadRequest(ApiResponse<object>.Fail("No se puede eliminar la categoría porque existen referencias a ella. Por favor, elimine primero las referencias.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la eliminación de la categoría. Por favor, contacte al administrador."))
            };
        }
    }
}
