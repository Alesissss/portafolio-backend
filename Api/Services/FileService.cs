using Api.Services.Interfaces;

namespace Api.Services
{
    public class FileService : IFileService
    {
        private readonly string _rutaBasePrivada;

        public FileService(IWebHostEnvironment env)
        {
            // ContentRootPath = raíz del proyecto. A diferencia de wwwroot (WebRootPath), NO se sirve
            // estáticamente al público, así que es el lugar correcto para archivos privados.
            _rutaBasePrivada = Path.Combine(env.ContentRootPath, "Almacenamiento", "Privado");
        }

        public async Task<string> GuardarPrivadoAsync(IFormFile archivo, string subCarpeta)
        {
            // Nombre único generado por el servidor: nunca uso el nombre original del cliente.
            // Evita colisiones entre archivos y cierra la puerta a path traversal (../../algo).
            var extension = Path.GetExtension(archivo.FileName);
            var nombreArchivo = $"{Guid.NewGuid():N}{extension}";

            var carpetaDestino = Path.Combine(_rutaBasePrivada, subCarpeta);
            Directory.CreateDirectory(carpetaDestino);   // crea la carpeta si no existe; no-op si ya existe

            var rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);
            await using var stream = new FileStream(rutaCompleta, FileMode.Create);
            await archivo.CopyToAsync(stream);

            // Ruta relativa (con '/') para guardar en BD: no ata la fila a la ruta física de esta máquina.
            return $"{subCarpeta}/{nombreArchivo}";
        }
    }
}
