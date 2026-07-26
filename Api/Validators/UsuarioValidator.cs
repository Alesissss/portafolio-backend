using Api.Dtos;
using FluentValidation;
namespace Api.Validators;

public class RegistrarUsuarioValidator : AbstractValidator<RegistrarRequestUsuarioDto>
{
    public RegistrarUsuarioValidator()
    {
        RuleFor(x => x.ApellidoPaterno)
            .NotEmpty().WithMessage("El apellido paterno es obligatorio.")
            .MaximumLength(30).WithMessage("El apellido paterno no puede exceder los 30 caracteres.");

        RuleFor(x => x.ApellidoMaterno)
            .NotEmpty().WithMessage("El apellido materno es obligatorio.")
            .MaximumLength(30).WithMessage("El apellido materno no puede exceder los 30 caracteres.");

        RuleFor(x => x.Nombres)
            .NotEmpty().WithMessage("Los nombres son obligatorios.")
            .MaximumLength(30).WithMessage("Los nombres no pueden exceder los 30 caracteres.");

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.")
            .MaximumLength(255).WithMessage("El correo no puede exceder los 255 caracteres.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MaximumLength(255).WithMessage("El nombre de usuario no puede exceder los 255 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número.")
            .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe contener al menos un carácter especial.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Debe confirmar la contraseña.")
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");

        RuleFor(x => x.IdRol)
            .NotEmpty().WithMessage("El rol es obligatorio.");
    }
}

public class EditarUsuarioValidator : AbstractValidator<EditarRequestUsuarioDto>
{
    public EditarUsuarioValidator()
    {
        RuleFor(x => x.IdUsuario)
            .NotEmpty().WithMessage("El ID del usuario es obligatorio.");

        RuleFor(x => x.ApellidoPaterno)
            .NotEmpty().WithMessage("El apellido paterno es obligatorio.")
            .MaximumLength(30).WithMessage("El apellido paterno no puede exceder los 30 caracteres.");

        RuleFor(x => x.ApellidoMaterno)
            .NotEmpty().WithMessage("El apellido materno es obligatorio.")
            .MaximumLength(30).WithMessage("El apellido materno no puede exceder los 30 caracteres.");

        RuleFor(x => x.Nombres)
            .NotEmpty().WithMessage("Los nombres son obligatorios.")
            .MaximumLength(30).WithMessage("Los nombres no pueden exceder los 30 caracteres.");

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.")
            .MaximumLength(255).WithMessage("El correo no puede exceder los 255 caracteres.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MaximumLength(255).WithMessage("El nombre de usuario no puede exceder los 255 caracteres.");

        RuleFor(x => x.IdRol)
            .NotEmpty().WithMessage("El rol es obligatorio.");
    }
}
