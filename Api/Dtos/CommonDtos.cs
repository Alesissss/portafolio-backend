namespace Api.Dtos;

public record PaginacionResponseDto<T>(
    int TotalRegistros,
    int PaginaActual,
    int RegistrosPorPagina,
    int TotalPaginas,
    List<T> Elementos
);