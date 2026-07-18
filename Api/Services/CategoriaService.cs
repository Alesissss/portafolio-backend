using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly PortafolioDbContext _context;
        public CategoriaService(PortafolioDbContext context)
        {
            _context = context;
        }
        public async Task<List<CategoriaDto>> ListarCategoriasAsync()
        {
            var categorias = await _context.Categorias.AsNoTracking().ToListAsync();
            return categorias.Select(CategoriaToDto).ToList();
        }

        public async Task<CategoriaResult> ObtenerUnaCategoriaAsync(string id)
        {
            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.IdCategoria == id);

            if (categoria == null) return new CategoriaResult(CategoriaResultType.NoExiste);

            return new CategoriaResult(CategoriaResultType.Ok, CategoriaToDto(categoria));
        }

        public async Task<CategoriaResultType> RegistrarCategoriaAsync(RegistrarRequestCategoriaDto dto)
        {
            // Validar si la categoría con ese ID ya existe
            bool existeID = await _context.Categorias.AnyAsync(c => c.IdCategoria == dto.IdCategoria);
            if (existeID) return CategoriaResultType.IdRepetido;

            // Validar si la categoría con ese nombre ya existe
            bool existeCategoria = await _context.Categorias.AnyAsync(c => c.Nombre == dto.Nombre);
            if (existeCategoria) return CategoriaResultType.NombreRepetido;

            var categoria = new Categoria
            {
                IdCategoria = dto.IdCategoria,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Estado = dto.Estado,
            };

            // Guardar cambios
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CategoriaResultType.Ok;
        }

        public async Task<CategoriaResultType> EditarCategoriaAsync(CategoriaDto dto)
        {
            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.IdCategoria == dto.IdCategoria);

            // Validar que la categoría exista
            if (categoria == null) return CategoriaResultType.NoExiste;

            // Validar que al editar no se elija un nombre repetido por otra categoría
            bool existeNombre = await _context.Categorias.AnyAsync(c => c.Nombre == dto.Nombre && c.IdCategoria != dto.IdCategoria);
            if (existeNombre) return CategoriaResultType.NombreRepetido;

            // Editar registro
            categoria.Nombre = dto.Nombre;
            categoria.Descripcion = dto.Descripcion;
            categoria.Estado = dto.Estado;

            // Guardar cambios
            await _context.SaveChangesAsync();

            return CategoriaResultType.Ok;
        }

        public async Task<CategoriaResultType> DarBajaCategoriaAsync(string id)
        {
            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.IdCategoria == id);

            // Validar que la categoría exista
            if (categoria == null) return CategoriaResultType.NoExiste;

            // Validar que no esté dado de baja
            if (!categoria.Estado) return CategoriaResultType.YaEsBaja;

            // Dar de baja
            categoria.Estado = false;

            // Guardar cambios
            await _context.SaveChangesAsync();

            return CategoriaResultType.Ok;
        }

        public async Task<CategoriaResultType> EliminarCategoriaAsync(string id)
        {
            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.IdCategoria == id);

            // Validar que la categoría exista
            if (categoria == null) return CategoriaResultType.NoExiste;

            // Validar que no tenga productos asociados
            var existeProductoAsociado = await _context.Productos.AnyAsync(p => p.IdCategoria == id);
            if (existeProductoAsociado) return CategoriaResultType.ExisteReferencia;

            // Eliminar (soft-delete interno)
            categoria.EstadoRegistro = false;
            // _context.Categorias.Remove(categoria); Eliminación real (física) de la bd

            // Guardar cambios
            await _context.SaveChangesAsync();

            return CategoriaResultType.Ok;
        }

        private CategoriaDto CategoriaToDto(Categoria cat) =>
            new CategoriaDto( 
                IdCategoria: cat.IdCategoria,
                Nombre: cat.Nombre,
                Descripcion: cat.Descripcion,
                Estado: cat.Estado
            );
    }
}
