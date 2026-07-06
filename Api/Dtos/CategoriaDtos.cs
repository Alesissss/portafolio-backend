namespace Api.Dtos;

public enum CategoriaResultType
{
    Ok,
    ExisteReferencia,
    IdRepetido,
    NombreRepetido,
    NoExiste,
    YaEsBaja,
}

public record CategoriaResult(
    CategoriaResultType Estado,
    CategoriaDto? Data = null
);

// Dto para devolver una categoría en Data y para el EDITAR
public record CategoriaDto(
    string IdCategoria,
    string Nombre,
    string? Descripcion,
    bool Estado
);

// Registrar
public record RegistrarRequestCategoriaDto(
    string IdCategoria,
    string Nombre,
    string? Descripcion,
    bool Estado = true
);