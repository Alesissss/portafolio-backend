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

