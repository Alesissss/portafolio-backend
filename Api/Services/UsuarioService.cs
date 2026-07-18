using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly PortafolioDbContext _context;
        public UsuarioService(PortafolioDbContext context)
        {
            _context = context;
        }

        // Listar todos
        public async Task<List<UsuarioDto>> ListarUsuariosAsync()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .ToListAsync();
            return usuarios.Select(UsuarioToDto).ToList();
        }

        // Obtener uno
        public async Task<UsuarioResult> ObtenerUnUsuarioAsync(Guid id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null) return new UsuarioResult(UsuarioResultType.NoExiste);

            return new UsuarioResult(UsuarioResultType.Ok, UsuarioToDto(usuario));
        }

        // Registrar
        public async Task<UsuarioResultType> RegistrarUsuarioAsync(RegistrarRequestUsuarioDto dto)
        {
            // Username único
            bool existeUsername = await _context.Usuarios.AnyAsync(u => u.Username == dto.Username);
            if (existeUsername) return UsuarioResultType.UsernameRepetido;

            // El rol debe existir (y estar vigente: el query filter global excluye los eliminados)
            bool existeRol = await _context.Roles.AnyAsync(r => r.IdRol == dto.IdRol);
            if (!existeRol) return UsuarioResultType.RolNoExiste;

            var usuario = new Usuario
            {
                ApellidoPaterno = dto.ApellidoPaterno,
                ApellidoMaterno = dto.ApellidoMaterno,
                Nombres = dto.Nombres,
                Correo = dto.Correo,
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IdRol = dto.IdRol,
                Estado = dto.Estado,
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return UsuarioResultType.Ok;
        }

        // Editar (no toca la contraseña)
        public async Task<UsuarioResultType> EditarUsuarioAsync(EditarRequestUsuarioDto dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == dto.IdUsuario);

            if (usuario == null) return UsuarioResultType.NoExiste;

            // Username único entre OTROS usuarios
            bool existeUsername = await _context.Usuarios
                .AnyAsync(u => u.Username == dto.Username && u.IdUsuario != dto.IdUsuario);
            if (existeUsername) return UsuarioResultType.UsernameRepetido;

            bool existeRol = await _context.Roles.AnyAsync(r => r.IdRol == dto.IdRol);
            if (!existeRol) return UsuarioResultType.RolNoExiste;

            usuario.ApellidoPaterno = dto.ApellidoPaterno;
            usuario.ApellidoMaterno = dto.ApellidoMaterno;
            usuario.Nombres = dto.Nombres;
            usuario.Correo = dto.Correo;
            usuario.Username = dto.Username;
            usuario.IdRol = dto.IdRol;
            usuario.Estado = dto.Estado;

            await _context.SaveChangesAsync();

            return UsuarioResultType.Ok;
        }

        // Dar baja (solo desactiva)
        public async Task<UsuarioResultType> DarBajaUsuarioAsync(Guid id)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null) return UsuarioResultType.NoExiste;
            if (!usuario.Estado) return UsuarioResultType.YaEsBaja;

            usuario.Estado = false;
            await _context.SaveChangesAsync();

            return UsuarioResultType.Ok;
        }

        // Eliminar (soft-delete). Bloquea si el usuario ya realizó ventas.
        public async Task<UsuarioResultType> EliminarUsuarioAsync(Guid id)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null) return UsuarioResultType.NoExiste;

            bool existeReferencia = await _context.Ventas.AnyAsync(v => v.IdVendedor == id);
            if (existeReferencia) return UsuarioResultType.ExisteReferencia;

            usuario.EstadoRegistro = false;
            await _context.SaveChangesAsync();

            return UsuarioResultType.Ok;
        }

        private UsuarioDto UsuarioToDto(Usuario u) =>
            new UsuarioDto(
                IdUsuario: u.IdUsuario,
                IdRol: u.IdRol,
                NombreRol: u.Rol.Nombre,
                ApellidoPaterno: u.ApellidoPaterno,
                ApellidoMaterno: u.ApellidoMaterno,
                Nombres: u.Nombres,
                Correo: u.Correo,
                Username: u.Username,
                Estado: u.Estado
            );
    }
}
