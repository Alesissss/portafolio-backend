namespace Api.Dtos;

// Enum para saber los estados del flujo de la respuesta
public enum AuthResultType
{
    Ok,
    CredencialesInvalidas,
    UsuarioInactivo,
    RolNoEncontrado,
};

// El record simple que lleva consigo el estado de la respuesta y la data si es que tuviera
public record AuthResult(AuthResultType Estado, LoginResponseDto? Data = null);

// Usuario mínimo que viaja en la respuesta del login. Es INDEPENDIENTE del UsuarioDto del CRUD
// de Usuarios: cada contexto expone solo lo suyo (evita fugar campos de un endpoint a otro).
public record UsuarioAuthDto(
    Guid IdUsuario,
    Guid IdRol,
    string ApellidoPaterno,
    string ApellidoMaterno,
    string Nombres,
    string Correo,
    string Username,
    bool Estado
    );

// Login
public record LoginRequestDto(
    string Username,
    string Password
    );

public record LoginResponseDto(
    string Token,
    DateTime Expiration,
    UsuarioAuthDto Usuario
    );
