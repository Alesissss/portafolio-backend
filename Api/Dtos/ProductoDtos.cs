namespace Api.Dtos;

public enum ProductoResultType
{
    Ok,
    ExisteReferencia,
    NombreRepetido,
    NoExiste,
    YaEsBaja,
}

public record ProductoResult(
    ProductoResultType Estado,
    ProductoDto? Data = null
);

// Dto para devolver data de un producto
public record ProductoDto(
    int IdProducto,
    string Nombre,
    string Descripcion,
    decimal Stock,
    decimal Precio,
    bool Estado,
    // Ruta relativa dentro de wwwroot ("imagenes/productos/abc.jpg"), o null si no tiene foto.
    // El front la concatena con la URL base de la API para pintar el <img>.
    string? ArchivoFoto,
    // Categoria
    string IdCategoria,
    string NombreCategoria
);

// Dto para crear un producto
public record RegistrarRequestProductoDto(
    string Nombre,
    string Descripcion,
    decimal Stock,
    decimal Precio,
    bool Estado,
    // Categoria
    string IdCategoria
);

