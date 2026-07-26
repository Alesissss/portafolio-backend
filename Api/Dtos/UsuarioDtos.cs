namespace Api.Dtos;

public enum UsuarioResultType
{
    Ok,
    NoExiste,
    UsernameRepetido,
    RolNoExiste,
    YaEsBaja,
    ExisteReferencia,
}

public record UsuarioResult(
    UsuarioResultType Estado,
    UsuarioDto? Data = null
);

// Dto para devolver/listar un usuario. Incluye NombreRol (solo lectura) para mostrar el rol
// sin exponerlo en la respuesta del login (ese usa UsuarioAuthDto).
public record UsuarioDto(
    Guid IdUsuario,
    Guid IdRol,
    string NombreRol,
    string ApellidoPaterno,
    string ApellidoMaterno,
    string Nombres,
    string Correo,
    string Username,
    bool Estado
);

// Crear usuario (el alta la hace un administrador; no hay autoregistro).
public record RegistrarRequestUsuarioDto(
    string ApellidoPaterno,
    string ApellidoMaterno,
    string Nombres,
    string Correo,
    string Username,
    string Password,
    string ConfirmPassword,
    Guid IdRol,
    bool Estado
);

// Editar usuario: sin password (el cambio de clave sería un flujo aparte) ni NombreRol.
public record EditarRequestUsuarioDto(
    Guid IdUsuario,
    string ApellidoPaterno,
    string ApellidoMaterno,
    string Nombres,
    string Correo,
    string Username,
    Guid IdRol,
    bool Estado
);
