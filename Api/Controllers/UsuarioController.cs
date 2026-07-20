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
    public class UsuarioController(IUsuarioService _usuarioService) : ControllerBase
    {
        // Listar todos
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UsuarioDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<UsuarioDto>>>> ListarUsuarios()
        {
            var usuarios = await _usuarioService.ListarUsuariosAsync();
            return Ok(ApiResponse<List<UsuarioDto>>.Success(usuarios, "Usuarios listados correctamente"));
        }

        // Listar uno
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UsuarioDto>>> ListarUnUsuario(Guid id)
        {
            var result = await _usuarioService.ObtenerUnUsuarioAsync(id);

            return result.Estado switch
            {
                UsuarioResultType.Ok =>
                    Ok(ApiResponse<UsuarioDto>.Success(result.Data!, "Usuario listado correctamente")),
                UsuarioResultType.NoExiste =>
                    NotFound(ApiResponse<object>.Fail("El usuario buscado no existe.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la búsqueda del usuario. Por favor, contacte al administrador."))
            };
        }

        // Registrar
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RegistrarUsuario([FromBody] RegistrarRequestUsuarioDto dto)
        {
            var result = await _usuarioService.RegistrarUsuarioAsync(dto);
            return result switch
            {
                UsuarioResultType.Ok =>
                    StatusCode(201, ApiResponse<object>.Success("Usuario registrado correctamente")),
                UsuarioResultType.UsernameRepetido =>
                    BadRequest(ApiResponse<object>.Fail("El nombre de usuario ya está en uso. Por favor, elija otro.")),
                UsuarioResultType.RolNoExiste =>
                    BadRequest(ApiResponse<object>.Fail("El rol seleccionado no existe.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante el registro del usuario. Por favor, contacte al administrador."))
            };
        }

        // Editar
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> EditarUsuario([FromBody] EditarRequestUsuarioDto dto)
        {
            var result = await _usuarioService.EditarUsuarioAsync(dto);
            return result switch
            {
                UsuarioResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Usuario editado correctamente")),
                UsuarioResultType.NoExiste =>
                    NotFound(ApiResponse<object>.Fail("El usuario que intenta editar no existe.")),
                UsuarioResultType.UsernameRepetido =>
                    BadRequest(ApiResponse<object>.Fail("El nombre de usuario ya está en uso por otro usuario. Por favor, elija otro.")),
                UsuarioResultType.RolNoExiste =>
                    BadRequest(ApiResponse<object>.Fail("El rol seleccionado no existe.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la edición del usuario. Por favor, contacte al administrador."))
            };
        }

        // Dar baja
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DarBajaUsuario(Guid id)
        {
            var result = await _usuarioService.DarBajaUsuarioAsync(id);
            return result switch
            {
                UsuarioResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Usuario dado de baja correctamente")),
                UsuarioResultType.NoExiste =>
                    NotFound(ApiResponse<object>.Fail("El usuario que intenta dar de baja no existe.")),
                UsuarioResultType.YaEsBaja =>
                    BadRequest(ApiResponse<object>.Fail("El usuario ya está dado de baja.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la operación. Por favor, contacte al administrador."))
            };
        }

        // Eliminar
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> EliminarUsuario(Guid id)
        {
            var result = await _usuarioService.EliminarUsuarioAsync(id);
            return result switch
            {
                UsuarioResultType.Ok =>
                    Ok(ApiResponse<object>.Success("Usuario eliminado correctamente")),
                UsuarioResultType.NoExiste =>
                    NotFound(ApiResponse<object>.Fail("El usuario que intenta eliminar no existe.")),
                UsuarioResultType.ExisteReferencia =>
                    BadRequest(ApiResponse<object>.Fail("No se puede eliminar el usuario porque tiene ventas asociadas. Por favor, désele de baja en su lugar.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante la eliminación del usuario. Por favor, contacte al administrador."))
            };
        }
    }
}
