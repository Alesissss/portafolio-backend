using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Tests.Infra
{
    // Fábrica de DbContext para tests, sobre una base de datos EN MEMORIA.
    //
    // ¿Por qué InMemory y no la BD real? Porque estos son tests de la LÓGICA de tus services
    // (estados, stock, reconciliación), no de Postgres. Son rápidos, aislados y no necesitan Docker.
    // Contra: InMemory NO valida FKs, tipos ni constraints; para eso más adelante se usa Postgres real
    // (Testcontainers). Para lo que queremos ahora, es justo lo correcto.
    public static class TestDb
    {
        // Cada test usa un nombre de BD distinto (un Guid) => bases aisladas que no se pisan entre sí.
        public static PortafolioDbContext Nuevo(string nombreBd)
        {
            var options = new DbContextOptionsBuilder<PortafolioDbContext>()
                .UseInMemoryDatabase(nombreBd)
                // Nuestro SaveChangesAsync abre una transacción para la auditoría; InMemory no soporta
                // transacciones, así que ignoramos ese warning (las trata como no-op).
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                // Rellena AuditoriaLog.UsuarioBd, que en producción pone el default SQL de Postgres.
                .AddInterceptors(new AuditoriaBdInterceptor())
                .Options;

            // El httpContextAccessor solo se usa (con ?.) para sacar el usuario en la auditoría.
            // En tests no hay request, así que null es seguro.
            return new PortafolioDbContext(options, httpContextAccessor: null!);
        }
    }
}
