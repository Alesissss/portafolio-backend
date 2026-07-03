using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Api.Data
{
    public class PortafolioDbContext(DbContextOptions<PortafolioDbContext> options, IHttpContextAccessor http) : DbContext(options)
    {
        public DbSet<Permiso> Permiso { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<PermisoRol> PermisoRol { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<Producto> Producto { get; set; }
        public DbSet<EstadoVenta> EstadoVenta { get; set; }
        public DbSet<Venta> Venta { get; set; }
        public DbSet<DetalleVenta> DetalleVenta { get; set; }
        public DbSet<AuditoriaLog> AuditoriaLog { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Permiso>(b =>
            {
                b.HasKey(p => p.IdPermiso);
                b.Property(p => p.IdPermiso).HasMaxLength(15);
                b.Property(p => p.Nombre).IsRequired().HasMaxLength(30);
                b.Property(p => p.Descripcion).HasMaxLength(255);
            });

            modelBuilder.Entity<Rol>(b =>
            {
                b.HasKey(r => r.IdRol);
                b.Property(r => r.Nombre).IsRequired().HasMaxLength(30);
                b.HasIndex(r => r.Nombre).IsUnique();
            });

            modelBuilder.Entity<PermisoRol>(b =>
            {
                b.HasKey(pr => new { pr.IdPermiso, pr.IdRol });

                b.HasOne<Permiso>()
                 .WithMany()
                 .HasForeignKey(pr => pr.IdPermiso);

                b.HasOne<Rol>()
                 .WithMany()
                 .HasForeignKey(pr => pr.IdRol);
            });

            modelBuilder.Entity<Usuario>(b =>
            {
                b.HasKey(u => u.IdUsuario);
                b.Property(u => u.ApellidoPaterno).IsRequired().HasMaxLength(30);
                b.Property(u => u.ApellidoMaterno).IsRequired().HasMaxLength(30);
                b.Property(u => u.Nombres).IsRequired().HasMaxLength(30);
                b.Property(u => u.Correo).IsRequired().HasMaxLength(255);
                b.Property(u => u.Username).IsRequired().HasMaxLength(255);
                b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
                b.HasIndex(u => u.Username).IsUnique();

                b.HasOne(u => u.Rol)
                 .WithMany()
                 .HasForeignKey(u => u.IdRol)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Categoria>(b =>
            {
                b.HasKey(c => c.IdCategoria);
                b.Property(c => c.IdCategoria).HasMaxLength(3);
                b.Property(c => c.Nombre).IsRequired().HasMaxLength(30);
                b.Property(c => c.Descripcion).HasMaxLength(255);
            });

            modelBuilder.Entity<Producto>(b =>
            {
                b.HasKey(p => p.IdProducto);
                b.Property(p => p.Descripcion).IsRequired().HasMaxLength(50);
                b.Property(p => p.Stock).HasPrecision(19, 2);
                b.Property(p => p.Precio).HasPrecision(19, 2);

                b.HasOne(p => p.Categoria)
                 .WithMany()
                 .HasForeignKey(p => p.IdCategoria)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EstadoVenta>(b =>
            {
                b.HasKey(ev => ev.IdEstadoVenta);
                b.Property(ev => ev.IdEstadoVenta).HasMaxLength(3);
                b.Property(ev => ev.Descripcion).IsRequired().HasMaxLength(30);
            });

            modelBuilder.Entity<Venta>(b =>
            {
                b.HasKey(v => v.IdVenta);
                b.Property(v => v.IdEstadoVenta).HasMaxLength(3);
                b.Property(v => v.Subtotal).HasPrecision(19, 2);
                b.Property(v => v.Igv).HasPrecision(19, 2);
                b.Property(v => v.Total).HasPrecision(19, 2);

                b.HasOne(v => v.EstadoVenta)
                 .WithMany()
                 .HasForeignKey(v => v.IdEstadoVenta);

                b.HasOne(v => v.Vendedor)
                 .WithMany()
                 .HasForeignKey(v => v.IdVendedor)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(v => v.Cliente)
                 .WithMany()
                 .HasForeignKey(v => v.IdCliente)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DetalleVenta>(b =>
            {
                b.HasKey(dv => new { dv.IdVenta, dv.IdProducto });
                b.Property(dv => dv.PrecioVenta).HasPrecision(9, 2);
                b.Property(dv => dv.Cantidad).HasPrecision(9, 2);
                b.Property(dv => dv.Observacion).HasMaxLength(255);

                b.HasOne(dv => dv.Venta)
                 .WithMany()
                 .HasForeignKey(dv => dv.IdVenta);

                b.HasOne(dv => dv.Producto)
                 .WithMany()
                 .HasForeignKey(dv => dv.IdProducto);
            });

            modelBuilder.Entity<AuditoriaLog>(b =>
            {
                b.HasKey(al => al.IdLog);
                b.Property(al => al.NombreTabla).IsRequired().HasMaxLength(100);
                b.Property(al => al.Operacion).IsRequired().HasMaxLength(10);
                b.Property(al => al.UsuarioBd).IsRequired().HasMaxLength(100);

                b.Property(al => al.RegistroAnterior).HasColumnType("jsonb");
                b.Property(al => al.RegistroNuevo).HasColumnType("jsonb");
            });
        }

        //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    var cambios = ChangeTracker.Entries()
        //        .Where(e => e.Entity is not AuditoriaLog
        //                && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
        //        .ToList();
        //}
    }
}