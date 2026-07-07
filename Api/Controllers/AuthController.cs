using Api.Common;
using Api.Dtos;
using Api.Services;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService _authService) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            return result.Estado switch
            {
                AuthResultType.Ok =>
                    Ok(ApiResponse<LoginResponseDto>.Success(result.Data!, "Inicio de sesión exitoso")),

                AuthResultType.UsuarioInactivo =>
                    StatusCode(403, ApiResponse<object>.Fail("El usuario está inactivo. Por favor, contacte al administrador.")),

                AuthResultType.RolNoEncontrado =>
                    StatusCode(500, ApiResponse<object>.Fail("El rol del usuario no fue encontrado. Por favor, contacte al administrador.")),

                AuthResultType.CredencialesInvalidas =>
                    Unauthorized(ApiResponse<object>.Fail("Credenciales inválidas. Por favor, verifique su nombre de usuario y contraseña.")),

                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado. Por favor, contacte al administrador."))
            };
        }

        [HttpPost("registrar")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> Registrar([FromBody] RegistrarRequestDto dto)
        {
            var result = await _authService.RegistrarAsync(dto);
            return result.Estado switch
            {
                RegisterResultType.Ok =>
                    StatusCode(201, ApiResponse<object>.Success("Registro exitoso")),
                RegisterResultType.UsuarioExistente =>
                    BadRequest(ApiResponse<object>.Fail("El nombre de usuario ya está en uso. Por favor, elija otro.")),
                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado durante el registro. Por favor, contacte al administrador."))
            };
        }
    }
}
