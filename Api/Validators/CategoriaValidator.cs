using Api.Dtos;
using FluentValidation;
namespace Api.Validators;

public class CategoriaValidator : AbstractValidator<RegistrarRequestCategoriaDto>
{
    public CategoriaValidator()
    {
        RuleFor(x => x.IdCategoria)
            .NotEmpty().WithMessage("El ID de la categoría es obligatorio.")
            .MaximumLength(3).WithMessage("El ID de la categoría no puede exceder los 3 caracteres.");
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
            .MaximumLength(3).WithMessage("El ID de la categoría no puede exceder los 3 caracteres.");
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(30).WithMessage("El nombre de la categoría no puede exceder los 30 caracteres.");
        RuleFor(x => x.Descripcion)
            .MaximumLength(255).WithMessage("La descripción de la categoría no puede exceder los 255 caracteres.");
    }
}