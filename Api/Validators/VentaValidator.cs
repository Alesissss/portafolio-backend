using Api.Dtos;
using FluentValidation;
namespace Api.Validators;

// Estas reglas son de FORMA (¿el request está bien armado?), no de negocio.
// Lo que depende de la BD -que el producto exista, que haya stock, que la venta
// esté en BORRADOR- se queda en VentaService: el validator no toca base de datos.

public class RegistrarVentaValidator : AbstractValidator<RegistrarRequestVentaDto>
{
    public RegistrarVentaValidator()
    {
        RuleFor(x => x.IdVendedor)
            .NotEmpty().WithMessage("El vendedor es obligatorio.");

        RuleFor(x => x.Detalles)
            .NotEmpty().WithMessage("La venta debe tener al menos un producto.");

        // Un producto repetido en dos renglones haría que el segundo pise al primero
        // al reconciliar por IdProducto. Se corta aquí.
        RuleFor(x => x.Detalles)
            .Must(d => d.Select(x => x.IdProducto).Distinct().Count() == d.Count)
            .WithMessage("Hay productos repetidos en el detalle: use un solo renglón por producto.")
            .When(x => x.Detalles is not null && x.Detalles.Count > 0);

        // RuleForEach aplica el validator hijo a CADA elemento de la lista. FluentValidation
        // nombra el error con su índice (PropertyName = "Detalles[1].Cantidad"); hoy el
        // ValidacionFilter solo devuelve el primer ErrorMessage, así que ese índice se
        // descarta. Está ahí si algún día quieres marcar el renglón exacto en el front.
        RuleForEach(x => x.Detalles).SetValidator(new RegistrarDetalleVentaValidator());
    }
}

public class RegistrarDetalleVentaValidator : AbstractValidator<RegistrarDetalleVentaDto>
{
    public RegistrarDetalleVentaValidator()
    {
        RuleFor(x => x.IdProducto)
            .GreaterThan(0).WithMessage("El producto es obligatorio.");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.")
            // Cantidad es NUMERIC(9,2) en la BD; sin tope, un decimal enorme reventaría
            // al guardar en vez de responder un 400 legible.
            .LessThanOrEqualTo(9_999_999).WithMessage("La cantidad excede el máximo permitido.");

        RuleFor(x => x.Observacion)
            .MaximumLength(200).WithMessage("La observación no puede exceder los 200 caracteres.");
    }
}

public class EditarVentaValidator : AbstractValidator<EditarRequestVentaDto>
{
    public EditarVentaValidator()
    {
        RuleFor(x => x.IdVenta)
            .NotEmpty().WithMessage("El ID de la venta es obligatorio.");

        RuleFor(x => x.IdVendedor)
            .NotEmpty().WithMessage("El vendedor es obligatorio.");

        RuleFor(x => x.Detalles)
            .NotEmpty().WithMessage("La venta debe tener al menos un producto.");

        RuleFor(x => x.Detalles)
            .Must(d => d.Select(x => x.IdProducto).Distinct().Count() == d.Count)
            .WithMessage("Hay productos repetidos en el detalle: use un solo renglón por producto.")
            .When(x => x.Detalles is not null && x.Detalles.Count > 0);

        // Alternativa a SetValidator cuando las reglas del hijo son cortas y no se
        // reutilizan: ChildRules las declara inline, sin crear otra clase.
        RuleForEach(x => x.Detalles).ChildRules(detalle =>
        {
            detalle.RuleFor(d => d.IdProducto)
                .GreaterThan(0).WithMessage("El producto es obligatorio.");

            detalle.RuleFor(d => d.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.")
                .LessThanOrEqualTo(9_999_999).WithMessage("La cantidad excede el máximo permitido.");

            detalle.RuleFor(d => d.Observacion)
                .MaximumLength(200).WithMessage("La observación no puede exceder los 200 caracteres.");
        });
    }
}
