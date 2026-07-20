using Microsoft.AspNetCore.Http;

namespace Api.Services.Interfaces
{
    public interface IFileService
    {
        // Guarda un archivo en el almacenamiento PRIVADO (fuera de wwwroot) dentro de la subcarpeta indicada
        // y devuelve la ruta RELATIVA que debes persistir en la BD (no la absoluta).
        Task<string> GuardarPrivadoAsync(IFormFile archivo, string subCarpeta);
    }
}
