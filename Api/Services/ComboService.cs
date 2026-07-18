using Api.Data;
using Api.Dtos;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class ComboService : IComboService
    {
        private readonly PortafolioDbContext _context;
        public ComboService(PortafolioDbContext context)
        {
            _context = context;
        }

        public async Task<List<ComboDto>> GetCategoriasComboAsync()
        {
            // Solo activas. El soft-delete (EstadoRegistro) ya lo filtra el query filter global.
            // Proyectamos directo a ComboDto en la consulta: la BD solo trae 2 columnas.
            return await _context.Categorias
                .AsNoTracking()
                .Where(c => c.Estado)
                .OrderBy(c => c.Nombre)
                .Select(c => new ComboDto(c.IdCategoria, c.Nombre))
                .ToListAsync();
        }

        public async Task<List<ComboDto>> GetRolesComboAsync()
        {
            // El value es el Guid del rol como string (el select del front trabaja con texto).
            return await _context.Roles
                .AsNoTracking()
                .Where(r => r.Estado)
                .OrderBy(r => r.Nombre)
                .Select(r => new ComboDto(r.IdRol.ToString(), r.Nombre))
                .ToListAsync();
        }
    }
}
