using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Tests.Infra
{
    // Solo para tests. En producción, AuditoriaLog.UsuarioBd lo rellena Postgres con su default
    // SQL (CURRENT_USER); InMemory no ejecuta SQL, así que lo llenamos nosotros antes de guardar.
    // Vive en el proyecto de tests: el código de producción no se entera de esto.
    public class AuditoriaBdInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, InterceptionResult<int> result)
        {
            Rellenar(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Rellenar(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void Rellenar(DbContext? context)
        {
            if (context is null) return;

            foreach (var entry in context.ChangeTracker.Entries<AuditoriaLog>())
                if (entry.State == EntityState.Added && string.IsNullOrEmpty(entry.Entity.UsuarioBd))
                    entry.Entity.UsuarioBd = "test";
        }
    }
}
