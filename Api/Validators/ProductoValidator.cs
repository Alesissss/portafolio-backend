using Api.Dtos;
using FluentValidation;
namespace Api.Validators;

public class ProductoValidator : AbstractValidator<RegistrarRequestProductoDto>
{
    public ProductoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre del producto no puede exceder los 50 caracteres.");
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción del producto es obligatoria.")
            .MaximumLength(50).WithMessage("La descripción del producto no puede exceder los 50 caracteres.");
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo.");
        RuleFor(x => x.Precio)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0.");
        RuleFor(x => x.IdCategoria)
            .NotEmpty().WithMessage("La categoría es obligatoria.")
            .MaximumLength(3).WithMessage("El ID de la categoría no puede exceder los 3 caracteres.");
    }
}

public class EditarProductoValidator : AbstractValidator<ProductoDto>
{
    public EditarProductoValidator()
    {
        RuleFor(x => x.IdProducto)
            .GreaterThan(0).WithMessage("El ID del producto es obligatorio.");
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre del producto no puede exceder los 50 caracteres.");
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción del producto es obligatoria.")
            .MaximumLength(50).WithMessage("La descripción del producto no puede exceder los 50 caracteres.");
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo.");
        RuleFor(x => x.Precio)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0.");
        RuleFor(x => x.IdCategoria)
            .NotEmpty().WithMessage("La categoría es obligatoria.")
            .MaximumLength(3).WithMessage("El ID de la categoría no puede exceder los 3 caracteres.");
    }
}
