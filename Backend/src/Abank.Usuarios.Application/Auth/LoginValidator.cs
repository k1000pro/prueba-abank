using FluentValidation;

namespace Abank.Usuarios.Application.Auth;

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Telefono).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(72);
    }
}
