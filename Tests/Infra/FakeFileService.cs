using Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Tests.Infra
{
    // Doble de prueba ("fake") de IFileService: NO toca el disco.
    // Solo devuelve una ruta fija y recuerda si lo llamaron. Así podemos probar PagarVenta
    // sin escribir archivos reales. Esto es justo por qué el service depende de la INTERFAZ
    // y no de la clase concreta: en tests le inyectamos otra implementación.
    public class FakeFileService : IFileService
    {
        public bool FueLlamado { get; private set; }
        public string RutaADevolver { get; set; } = "comprobantes/fake.pdf";

        // Lo que "hay" en el disco falso: ruta relativa -> contenido.
        public Dictionary<string, string> Archivos { get; } = new();

        public Task<string> GuardarPrivadoAsync(IFormFile archivo, string subCarpeta)
        {
            FueLlamado = true;
            return Task.FromResult(RutaADevolver);
        }

        public Task<string> GuardarPublicoAsync(IFormFile archivo, string subCarpeta)
        {
            FueLlamado = true;
            return Task.FromResult(RutaADevolver);
        }

        public (Stream Contenido, string TipoContenido)? AbrirPrivado(string rutaRelativa)
        {
            if (!Archivos.TryGetValue(rutaRelativa, out var contenido)) return null;

            var bytes = System.Text.Encoding.UTF8.GetBytes(contenido);
            return (new MemoryStream(bytes), "application/pdf");
        }

        public void EliminarPublico(string? rutaRelativa)
        {
            if (rutaRelativa is not null) Archivos.Remove(rutaRelativa);
        }
    }
}
