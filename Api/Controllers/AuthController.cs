using Api.Common;
using Api.Dtos;
using Api.Services;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService _authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            return result.Estado switch
            {
                AuthResultType.Ok =>
                    Ok(ApiResponse<LoginResponseDto>.Ok(result.Data!, "Inicio de sesión exitoso")),

                AuthResultType.UsuarioInactivo =>
                    StatusCode(403, ApiResponse<object>.Fail("El usuario está inactivo. Por favor, contacte al administrador.")),

                AuthResultType.RolNoEncontrado =>
                    StatusCode(500, ApiResponse<object>.Fail("El rol del usuario no fue encontrado. Por favor, contacte al administrador.")),

                AuthResultType.CredencialesInvalidas =>
                    Unauthorized(ApiResponse<object>.Fail("Credenciales inválidas. Por favor, verifique su nombre de usuario y contraseña.")),

                _ => StatusCode(500, ApiResponse<object>.Fail("Ocurrió un error inesperado. Por favor, contacte al administrador."))
            };
        }
    }
}
