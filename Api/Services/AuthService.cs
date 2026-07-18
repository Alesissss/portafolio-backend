using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly PortafolioDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        public AuthService(PortafolioDbContext context, IJwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        // Login
        public async Task<AuthResult> LoginAsync(LoginRequestDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            // Usuario no encontrado (no se deberían dar detalles de si el usuario existe o no)
            if (usuario == null) return new AuthResult(AuthResultType.CredencialesInvalidas);

            // Rol no encontrado o inactivo
            if (usuario.Rol == null || !usuario.Rol.Estado) return new AuthResult(AuthResultType.RolNoEncontrado);

            // Usuario inactivo
            if (!usuario.Estado) return new AuthResult(AuthResultType.UsuarioInactivo);

            // Verificar la contraseña y solo arrojar error de credenciales inválidas si no coincide
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
            if (!isPasswordValid) return new AuthResult(AuthResultType.CredencialesInvalidas);

            var (token, expiraEn) = _jwtTokenService.GenerarToken(usuario);

            var loginResponse = new LoginResponseDto(
                Token: token,
                Expiration: expiraEn,
                Usuario: new UsuarioDto(
                    IdUsuario: usuario.IdUsuario,
                    IdRol: usuario.IdRol,
                    ApellidoPaterno: usuario.ApellidoPaterno,
                    ApellidoMaterno: usuario.ApellidoMaterno,
                    Nombres: usuario.Nombres,
                    Correo: usuario.Correo,
                    Username: usuario.Username,
                    Estado: usuario.Estado
                )
            );

            return new AuthResult(AuthResultType.Ok, loginResponse);
        }

        // Registrar
        public async Task<RegisterResult> RegistrarAsync(RegistrarRequestDto dto)
        {
            // Verificar si el usuario ya existe (Username es UNIQUE)
            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (usuarioExistente != null) return new RegisterResult(RegisterResultType.UsuarioExistente);
            
            // Crear nuevo usuario
            var nuevoUsuario = new Usuario
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Correo = dto.Correo,
                Nombres = dto.Nombres,
                ApellidoPaterno = dto.ApellidoPaterno,
                ApellidoMaterno = dto.ApellidoMaterno,
                IdRol = dto.IdRol,
                Estado = true // Estado siempre es activo por defecto al registrar
            };
            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();
            return new RegisterResult(RegisterResultType.Ok);
        }
    }
}
