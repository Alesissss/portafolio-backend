namespace Api.Dtos;

// Filtros del dashboard. Llegan por query string ([FromQuery]) y aplican a TODO lo que
// devuelve el endpoint: si un numero cambia, cambian todos, para que nunca se contradigan.
// Desde/Hasta son obligatorios; el controller pone un default (ultimos 30 dias) si no vienen.
public record ReporteFiltroDto(
    DateTimeOffset Desde,
    DateTimeOffset Hasta,
    string? IdEstadoVenta,
    Guid? IdVendedor
);

// Los 4 numeros de la fila superior. Un numero suelto NO es un grafico: va como tile.
public record ResumenReporteDto(
    decimal TotalVendido,
    int NumeroVentas,
    decimal TicketPromedio,
    decimal PorCobrar
);

// Un punto de la serie de tiempo. Fecha es DateOnly: el front no necesita la hora y asi
// no hay sorpresas de huso horario al serializar.
public record PuntoSerieDto(
    DateOnly Fecha,
    decimal Total,
    int Ventas
);

public record TopProductoDto(
    int IdProducto,
    string Nombre,
    decimal Total,
    decimal Cantidad
);

public record VentasPorEstadoDto(
    string IdEstadoVenta,
    string Nombre,
    int Ventas,
    decimal Total
);

// Todo el dashboard en UNA respuesta. Es deliberado: si cada grafico pidiera su propio
// endpoint podrian llegar en momentos distintos y mostrar tajadas distintas de los datos.
public record ReporteDashboardDto(
    ResumenReporteDto Resumen,
    string Granularidad,               // "dia" o "mes": la decide el backend segun el rango
    List<PuntoSerieDto> Serie,
    List<TopProductoDto> TopProductos,
    List<VentasPorEstadoDto> PorEstado
);
