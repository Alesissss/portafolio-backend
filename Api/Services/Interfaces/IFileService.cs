using Microsoft.AspNetCore.Http;

namespace Api.Services.Interfaces
{
    public interface IFileService
    {
        // Guarda un archivo en el almacenamiento PRIVADO (fuera de wwwroot) dentro de la subcarpeta indicada
        // y devuelve la ruta RELATIVA que debes persistir en la BD (no la absoluta).
        Task<string> GuardarPrivadoAsync(IFormFile archivo, string subCarpeta);

        // Abre un archivo privado para servirlo desde un endpoint con [Authorize].
        // Devuelve null si no existe. El stream lo cierra quien lo consume.
        (Stream Contenido, string TipoContenido)? AbrirPrivado(string rutaRelativa);

        // Guarda en el almacenamiento PÚBLICO (wwwroot): cualquiera con la URL lo ve, sin JWT.
        // Solo para contenido no sensible, como la foto de un producto.
        Task<string> GuardarPublicoAsync(IFormFile archivo, string subCarpeta);

        // Borra un archivo público. Se usa al reemplazar una foto para no dejar huérfanos.
        void EliminarPublico(string? rutaRelativa);
    }
}
