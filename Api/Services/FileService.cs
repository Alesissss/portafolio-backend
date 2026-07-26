using Api.Services.Interfaces;
using Microsoft.AspNetCore.StaticFiles;

namespace Api.Services
{
    public class FileService : IFileService
    {
        private readonly string _rutaBasePrivada;
        private readonly string _rutaBasePublica;

        // Traduce extensión -> content type ("algo.pdf" -> "application/pdf").
        private static readonly FileExtensionContentTypeProvider _tipos = new();

        public FileService(IWebHostEnvironment env)
        {
            // ContentRootPath = raíz del proyecto. A diferencia de wwwroot (WebRootPath), NO se sirve
            // estáticamente al público, así que es el lugar correcto para archivos privados.
            _rutaBasePrivada = Path.Combine(env.ContentRootPath, "Almacenamiento", "Privado");

            // wwwroot SÍ se sirve estáticamente (app.UseStaticFiles): lo que caiga aquí queda
            // accesible por URL directa, sin pasar por un controller y sin JWT.
            _rutaBasePublica = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        }

        public Task<string> GuardarPrivadoAsync(IFormFile archivo, string subCarpeta) =>
            GuardarAsync(archivo, subCarpeta, _rutaBasePrivada);

        public Task<string> GuardarPublicoAsync(IFormFile archivo, string subCarpeta) =>
            GuardarAsync(archivo, subCarpeta, _rutaBasePublica);

        private static async Task<string> GuardarAsync(IFormFile archivo, string subCarpeta, string rutaBase)
        {
            // Nombre único generado por el servidor: nunca uso el nombre original del cliente.
            // Evita colisiones entre archivos y cierra la puerta a path traversal (../../algo).
            var extension = Path.GetExtension(archivo.FileName);
            var nombreArchivo = $"{Guid.NewGuid():N}{extension}";

            var carpetaDestino = Path.Combine(rutaBase, subCarpeta);
            Directory.CreateDirectory(carpetaDestino);   // crea la carpeta si no existe; no-op si ya existe

            var rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);
            await using var stream = new FileStream(rutaCompleta, FileMode.Create);
            await archivo.CopyToAsync(stream);

            // Ruta relativa (con '/') para guardar en BD: no ata la fila a la ruta física de esta máquina.
            return $"{subCarpeta}/{nombreArchivo}";
        }

        public (Stream Contenido, string TipoContenido)? AbrirPrivado(string rutaRelativa)
        {
            var rutaCompleta = ResolverDentroDe(_rutaBasePrivada, rutaRelativa);
            if (rutaCompleta is null || !File.Exists(rutaCompleta)) return null;

            if (!_tipos.TryGetContentType(rutaCompleta, out var tipoContenido))
                tipoContenido = "application/octet-stream";

            return (File.OpenRead(rutaCompleta), tipoContenido);
        }

        public void EliminarPublico(string? rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa)) return;

            var rutaCompleta = ResolverDentroDe(_rutaBasePublica, rutaRelativa);
            if (rutaCompleta is not null && File.Exists(rutaCompleta)) File.Delete(rutaCompleta);
        }

        // Convierte la ruta relativa guardada en BD a ruta física verificando que NO se salga de
        // la carpeta base. Sin esto, un valor manipulado como "../../appsettings.json" permitiría
        // leer o borrar archivos del servidor (path traversal).
        private static string? ResolverDentroDe(string rutaBase, string rutaRelativa)
        {
            var baseCompleta = Path.GetFullPath(rutaBase);
            var candidata = Path.GetFullPath(Path.Combine(baseCompleta, rutaRelativa));

            return candidata.StartsWith(baseCompleta, StringComparison.Ordinal) ? candidata : null;
        }
    }
}
