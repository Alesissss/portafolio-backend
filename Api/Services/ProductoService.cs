using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class ProductoService : IProductoService
    {
        private readonly PortafolioDbContext _context;
        public ProductoService(PortafolioDbContext context)
        {
            _context = context;
        }

        // Listar todos
        public async Task<List<ProductoDto>> GetProductosAsync()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .AsNoTracking()
                .ToListAsync();
            return productos.Select(ProductoToDto).ToList();
        }
        // Obtener uno
        public async Task<ProductoResult> ObtenerUnProductoAsync(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null) return new ProductoResult(ProductoResultType.NoExiste);

            return new ProductoResult(ProductoResultType.Ok, ProductoToDto(producto));
        }
        // Registrar
        public async Task<ProductoResultType> RegistrarProductoAsync(RegistrarRequestProductoDto dto)
        {
            bool existeNombre = await _context.Productos.AnyAsync(p => p.Nombre == dto.Nombre);

            if (existeNombre) return ProductoResultType.NombreRepetido;

            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Stock = dto.Stock,
                Precio = dto.Precio,
                Estado = dto.Estado,
                IdCategoria = dto.IdCategoria,
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return ProductoResultType.Ok;
        }
        // Editar
        public async Task<ProductoResultType> EditarProductoAsync(ProductoDto dto)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == dto.IdProducto);

            // No existe producto
            if (producto == null) return ProductoResultType.NoExiste;
            // Nombre repetido
            bool existeNombre = await _context.Productos.AnyAsync(p => p.Nombre == dto.Nombre && p.IdProducto != dto.IdProducto);
            if (existeNombre) return ProductoResultType.NombreRepetido;

            // Editar
            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.Stock = dto.Stock;
            producto.Precio = dto.Precio;
            producto.Estado = dto.Estado;
            producto.IdCategoria = dto.IdCategoria;

            // Guardar cambios
            await _context.SaveChangesAsync();

            return ProductoResultType.Ok;
        }
        // Dar baja
        public async Task<ProductoResultType> DarBajaProductoAsync(int id)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);

            // No existe producto
            if (producto == null) return ProductoResultType.NoExiste;

            // Ya está de baja
            if (!producto.Estado) return ProductoResultType.YaEsBaja;

            producto.Estado = false;

            await _context.SaveChangesAsync();

            return ProductoResultType.Ok;
        }
        // Eliminar
        public async Task<ProductoResultType> EliminarProductoAsync(int id)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);

            // No existe producto
            if (producto == null) return ProductoResultType.NoExiste;

            var existeReferencia = await _context.DetallesVenta.AnyAsync(dv => dv.IdProducto == id);
            if (existeReferencia) return ProductoResultType.ExisteReferencia;

            producto.EstadoRegistro = false;

            await _context.SaveChangesAsync();

            return ProductoResultType.Ok;
        }

        private static ProductoDto ProductoToDto(Producto p) =>
            new ProductoDto(
                IdProducto: p.IdProducto,
                Nombre: p.Nombre,
                Descripcion: p.Descripcion,
                Stock: p.Stock,
                Precio: p.Precio,
                Estado: p.Estado,
                IdCategoria: p.IdCategoria,
                NombreCategoria: p.Categoria.Nombre
            );
    }
}
