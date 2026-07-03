namespace Api.Dtos;

// Enum para saber los estados del flujo de la respuesta
public enum AuthResultType
{
    Ok,
    CredencialesInvalidas,
    UsuarioInactivo,
    RolNoEncontrado
};

// El record simple que lleva consigo el estado de la respuesta y la data si es que tuviera
public record AuthResult(AuthResultType Estado, LoginResponseDto? Data = null);

// DTOs para el flujo de Request -> Response
public record UsuarioDto(
    Guid IdUsuario,
    Guid IdRol,
    string ApellidoPaterno,
    string ApellidoMaterno,
    string Nombres,
    string Correo,
    string Username,
    bool Estado
    );

public record LoginRequestDto(
    string Username, 
    string Password
    );

public record LoginResponseDto(
    string Token,
    DateTime Expiration,
    UsuarioDto Usuario
    );