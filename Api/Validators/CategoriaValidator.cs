using Api.Dtos;
using FluentValidation;
namespace Api.Validators;

public class CategoriaValidator : AbstractValidator<RegistrarRequestCategoriaDto>
{
    public CategoriaValidator()
    {
        RuleFor(x => x.IdCategoria)
            .NotEmpty().WithMessage("El ID de la categoría es obligatorio.")
            // Length(3) y no MaximumLength(3): el código es de largo FIJO ('LAP', 'COM').
            // Ahora que la columna es VARCHAR, la BD ya no rellena, así que la regla de
            // los 3 caracteres tiene que exigirla el validator.
            .Length(3).WithMessage("El ID de la categoría debe tener exactamente 3 caracteres.");
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(30).WithMessage("El nombre de la categoría no puede exceder los 30 caracteres.");
        RuleFor(x => x.Descripcion)
            .MaximumLength(255).WithMessage("La descripción de la categoría no puede exceder los 255 caracteres.");
    }
}

public class EditarCategoriaValidator : AbstractValidator<CategoriaDto>
{
    public EditarCategoriaValidator()
    {
        RuleFor(x => x.IdCategoria)
            .NotEmpty().WithMessage("El ID de la categoría es obligatorio.")
            // Length(3) y no MaximumLength(3): el código es de largo FIJO ('LAP', 'COM').
            // Ahora que la columna es VARCHAR, la BD ya no rellena, así que la regla de
            // los 3 caracteres tiene que exigirla el validator.
            .Length(3).WithMessage("El ID de la categoría debe tener exactamente 3 caracteres.");
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(30).WithMessage("El nombre de la categoría no puede exceder los 30 caracteres.");
        RuleFor(x => x.Descripcion)
            .MaximumLength(255).WithMessage("La descripción de la categoría no puede exceder los 255 caracteres.");
    }
}