namespace Api.Dtos;

public enum VentaResultType
{
    Ok,
    NoEncontrada,
    ProductoNoExiste,   // un detalle apunta a un producto inexistente
    NoEditable,         // se intentó editar una venta que no está en BO
    EstadoInvalido,     // transición desde un estado que no corresponde
    StockInsuficiente,  // al generar, algún producto no alcanza
    ArchivoRequerido,   // al pagar, falta el comprobante
    NoAutorizado,       // acción restringida (ej. anular sin ser admin)
    ErrorRegistro,
};

public record VentaResult(
    VentaResultType Estado,
    VentaDto? Data = null
);

// Resultado de pedir el comprobante de pago. Lleva el stream abierto del archivo:
// el controller lo entrega con File(...) y ASP.NET se encarga de cerrarlo.
public record ComprobanteResult(
    VentaResultType Estado,
    Stream? Contenido = null,
    string? TipoContenido = null,
    string? NombreDescarga = null
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

// ---- Escritura: editar venta (solo en estado Borrador) ----
// DTO propio para tener control total: el cliente solo manda la venta y su lista
// de detalles deseada; el servidor reconcilia contra lo que hay en BD (alta/baja/modif).
// Precio y totales los recalcula el servidor, igual que en el registro.

public record EditarRequestVentaDto(
    Guid IdVenta,
    Guid IdVendedor,
    List<EditarDetalleVentaDto> Detalles
);

public record EditarDetalleVentaDto(
    int IdProducto,
    decimal Cantidad,
    string? Observacion
);
