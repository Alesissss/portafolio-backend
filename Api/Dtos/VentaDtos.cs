namespace Api.Dtos;

public enum VentaResultType
{
    Ok,
};

public record VentaResult(
    VentaResultType Estado,
    VentaDto? Data = null
);

// ---- Lectura: listado de ventas (y su detalle para mostrar) ----

public record VentaDto(
    Guid IdVenta,
    DateTimeOffset FechaEmision,
    decimal Subtotal,
    decimal Igv,
    decimal Total,
    Guid IdVendedor,
    string NombreVendedor,
    string IdEstadoVenta,
    string NombreEstadoVenta,
    List<DetalleVentaDto> Detalles
);

// Detalle tal como se MUESTRA (lleva el nombre del producto, no solo su id)
public record DetalleVentaDto(
    int IdProducto,
    string NombreProducto,
    decimal PrecioVenta,
    decimal Cantidad,
    string? Observacion
);

// ---- Escritura: registrar venta (para más adelante) ----
// El id, la fecha, el precio de venta y los totales los pone el servidor,
// no se reciben del request. Por eso este detalle es distinto al de lectura.

public record RegistrarRequestVentaDto(
    Guid IdVendedor,
    List<RegistrarDetalleVentaDto> Detalles
);

public record RegistrarDetalleVentaDto(
    int IdProducto,
    decimal Cantidad,
    string? Observacion
);
