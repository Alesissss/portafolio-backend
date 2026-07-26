using Api.Common;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Text.Json;

namespace Api.Data
{
    public class PortafolioDbContext(DbContextOptions<PortafolioDbContext> options, IHttpContextAccessor httpContextAccessor) : DbContext(options)
    {
        public DbSet<Permiso> Permisos => Set<Permiso>();
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<PermisoRol> PermisosRol => Set<PermisoRol>();  
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<EstadoVenta> EstadosVenta => Set<EstadoVenta>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
        public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Permiso>(b =>
            {
                b.ToTable("permisos");
                b.HasKey(p => p.IdPermiso);
                b.Property(p => p.IdPermiso).HasMaxLength(30);
                b.Property(p => p.Nombre).IsRequired().HasMaxLength(30);
                b.Property(p => p.Descripcion).HasMaxLength(255);
            });

            modelBuilder.Entity<Rol>(b =>
            {
                b.ToTable("rol");
                b.HasKey(r => r.IdRol);
                b.Property(r => r.Nombre).IsRequired().HasMaxLength(30);
                b.HasIndex(r => r.Nombre).IsUnique();
            });

            modelBuilder.Entity<PermisoRol>(b =>
            {
                b.ToTable("permiso_rol");
                b.HasKey(pr => new { pr.IdPermiso, pr.IdRol });
                b.Property(pr => pr.IdPermiso).HasMaxLength(30);

                b.HasOne<Permiso>()
                 .WithMany()
                 .HasForeignKey(pr => pr.IdPermiso);

                b.HasOne<Rol>()
                 .WithMany()
                 .HasForeignKey(pr => pr.IdRol);
            });

            modelBuilder.Entity<Usuario>(b =>
            {
                b.ToTable("usuario");
                b.HasKey(u => u.IdUsuario);
                b.Property(u => u.ApellidoPaterno).IsRequired().HasMaxLength(30);
                b.Property(u => u.ApellidoMaterno).IsRequired().HasMaxLength(30);
                b.Property(u => u.Nombres).IsRequired().HasMaxLength(30);
                b.Property(u => u.Correo).IsRequired().HasMaxLength(255);
                b.Property(u => u.Username).IsRequired().HasMaxLength(255);
                b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
                b.HasIndex(u => u.Username).IsUnique().HasFilter("estado_registro = true"); ;

                b.HasOne(u => u.Rol)
                 .WithMany()
                 .HasForeignKey(u => u.IdRol)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Categoria>(b =>
            {
                b.ToTable("categoria");
                b.HasKey(c => c.IdCategoria);
                b.Property(c => c.IdCategoria).HasMaxLength(3);
                b.Property(c => c.Nombre).IsRequired().HasMaxLength(30);
                b.Property(c => c.Descripcion).HasMaxLength(255);
            });

            modelBuilder.Entity<Producto>(b =>
            {
                b.ToTable("producto");
                b.HasKey(p => p.IdProducto);
                b.Property(p => p.Nombre).IsRequired().HasMaxLength(50);
                b.Property(p => p.Descripcion).IsRequired().HasMaxLength(50);
                b.Property(p => p.Stock).HasPrecision(19, 2);
                b.Property(p => p.Precio).HasPrecision(19, 2);
                b.Property(p => p.ArchivoFoto).HasMaxLength(255);

                b.HasOne(p => p.Categoria)
                 .WithMany()
                 .HasForeignKey(p => p.IdCategoria)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EstadoVenta>(b =>
            {
                b.ToTable("estado_venta");
                b.HasKey(ev => ev.IdEstadoVenta);
                b.Property(ev => ev.IdEstadoVenta).HasMaxLength(3);
                b.Property(ev => ev.Descripcion).IsRequired().HasMaxLength(30);
            });

            modelBuilder.Entity<Venta>(b =>
            {
                b.ToTable("venta");
                b.HasKey(v => v.IdVenta);
                b.Property(v => v.IdEstadoVenta).HasMaxLength(3);
                b.Property(v => v.Subtotal).HasPrecision(19, 2);
                b.Property(v => v.Igv).HasPrecision(19, 2);
                b.Property(v => v.Total).HasPrecision(19, 2);
                b.Property(p => p.ArchivoPago).HasMaxLength(255);

                b.HasOne(v => v.EstadoVenta)
                 .WithMany()
                 .HasForeignKey(v => v.IdEstadoVenta);

                b.HasOne(v => v.Vendedor)
                 .WithMany()
                 .HasForeignKey(v => v.IdVendedor)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DetalleVenta>(b =>
            {
                b.ToTable("detalle_venta");
                b.HasKey(dv => new { dv.IdVenta, dv.IdProducto });
                b.Property(dv => dv.PrecioVenta).HasPrecision(9, 2);
                b.Property(dv => dv.Cantidad).HasPrecision(9, 2);
                b.Property(dv => dv.Observacion).HasMaxLength(255);

                b.HasOne(dv => dv.Venta)
                 .WithMany(v => v.Detalles)
                 .HasForeignKey(dv => dv.IdVenta);

                b.HasOne(dv => dv.Producto)
                 .WithMany()
                 .HasForeignKey(dv => dv.IdProducto);
            });

            modelBuilder.Entity<AuditoriaLog>(b =>
            {
                b.ToTable("auditoria_log");

                b.Property(al => al.UsuarioBd).HasDefaultValueSql("CURRENT_USER").ValueGeneratedOnAdd();
                b.HasKey(al => al.IdLog);
                b.Property(al => al.NombreTabla).IsRequired().HasMaxLength(100);
                b.Property(al => al.Operacion).IsRequired().HasMaxLength(10);
                b.Property(al => al.UsuarioBd).IsRequired().HasMaxLength(100);

                b.Property(al => al.RegistroAnterior).HasColumnType("jsonb");
                b.Property(al => al.RegistroNuevo).HasColumnType("jsonb");
            });

            // Aplicar el query filter global para soft delete solo a las entidades que implementan ISoftDelete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Verificar si la entidad participa del borrado lógico
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(ISoftDelete.EstadoRegistro));
                    var condition = Expression.Equal(property, Expression.Constant(true));
                    var lambda = Expression.Lambda(condition, parameter);

                    // Filtro global para soft delete
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var cambios = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditoriaLog
                     && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

            var usuario = UsuarioActual();
            var pendientes = new List<(EntityEntry Entry, AuditoriaLog Log, bool EsDelete)>();
            foreach (var e in cambios)
            {
                var log = new AuditoriaLog
                {
                    NombreTabla = e.Metadata.GetTableName() ?? e.Metadata.ClrType.Name,
                    Operacion = e.State switch
                    {
                        EntityState.Added => "INSERT",
                        EntityState.Modified => "UPDATE",
                        _ => "DELETE",
                    },
                    UsuarioAccion = usuario,
                    RegistroAnterior = e.State == EntityState.Added ? null : Serializar(e.OriginalValues),
                };
                pendientes.Add((e, log, e.State == EntityState.Deleted));
            }

            // Sin cambios de negocio que auditar: guardado normal, no hace falta transacción extra.
            if (pendientes.Count == 0)
                return await base.SaveChangesAsync(cancellationToken);

            if (Database.CurrentTransaction is not null)
                return await GuardarConAuditoriaAsync(pendientes, cancellationToken);

            await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
            var resultado = await GuardarConAuditoriaAsync(pendientes, cancellationToken);
            await transaction.CommitAsync(cancellationToken);   // si algo falla antes, el Dispose hace rollback
            return resultado;
        }

        // Ejecuta los dos guardados: primero el negocio (para obtener PKs generadas y valores finales),
        // luego los logs de auditoría con esos datos ya disponibles.
        private async Task<int> GuardarConAuditoriaAsync(
            List<(EntityEntry Entry, AuditoriaLog Log, bool EsDelete)> pendientes,
            CancellationToken cancellationToken)
        {
            var resultado = await base.SaveChangesAsync(cancellationToken);

            foreach (var (entry, log, esDelete) in pendientes)
            {
                log.IdRegistro = PkComoTexto(entry);
                log.RegistroNuevo = esDelete ? null : Serializar(entry.CurrentValues);
            }
            AuditoriaLogs.AddRange(pendientes.Select(p => p.Log));
            await base.SaveChangesAsync(cancellationToken);   // no se vuelve a auditar (entidad = AuditoriaLog)

            return resultado;
        }

        public override int SaveChanges() => SaveChangesAsync().GetAwaiter().GetResult();

        private Guid? UsuarioActual()
        {
            var sub = httpContextAccessor?.HttpContext?.User?.FindFirst("sub")?.Value;   // MapInboundClaims=false conserva 'sub'
            return Guid.TryParse(sub, out var id) ? id : null;
        }

        private static string Serializar(PropertyValues valores)
        {
            var dict = valores.Properties.ToDictionary(p => p.Name, p => valores[p]);
            return JsonSerializer.Serialize(dict);
        }

        private static string PkComoTexto(EntityEntry entry)
        {
            var pk = entry.Metadata.FindPrimaryKey();
            return pk is null
                ? string.Empty
                : string.Join(",", pk.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString()));
        }
    }
}