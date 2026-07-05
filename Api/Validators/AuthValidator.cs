using Api.Dtos;
using FluentValidation;
namespace Api.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre de usuario no puede exceder los 50 caracteres.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}

// Para cuando haga el registro
public class RegistroRequestValidator : AbstractValidator<RegistrarRequestDto>
{
    public RegistroRequestValidator()
    {
        RuleFor(x => x.ApellidoPaterno)
            .NotEmpty().WithMessage("El apellido paterno es obligatorio.");

        RuleFor(x => x.ApellidoMaterno)
            .NotEmpty().WithMessage("El apellido materno es obligatorio.");

        RuleFor(x => x.Nombres)
            .NotEmpty().WithMessage("Los nombres son obligatorios.");

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.");

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

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.");

    }

}