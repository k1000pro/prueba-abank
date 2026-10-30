using FluentValidation;

namespace Abank.Usuarios.Application.Usuarios;

public class CrearUsuarioValidator : AbstractValidator<CrearUsuarioRequest>
{
    public CrearUsuarioValidator(TimeProvider reloj)
    {
        RuleFor(x => x.Nombres).Nombre();
        RuleFor(x => x.Apellidos).Nombre();
        RuleFor(x => x.FechaNacimiento).FechaNacimiento(reloj);
        RuleFor(x => x.Telefono).Telefono();
        RuleFor(x => x.Email).Correo();
        RuleFor(x => x.Direccion).Direccion();
        RuleFor(x => x.Password).NotEmpty().Password();
    }
}

public class ActualizarUsuarioValidator : AbstractValidator<ActualizarUsuarioRequest>
{
    public ActualizarUsuarioValidator(TimeProvider reloj)
    {
        RuleFor(x => x.Nombres).Nombre();
        RuleFor(x => x.Apellidos).Nombre();
        RuleFor(x => x.FechaNacimiento).FechaNacimiento(reloj);
        RuleFor(x => x.Telefono).Telefono();
        RuleFor(x => x.Email).Correo();
        RuleFor(x => x.Direccion).Direccion();
        RuleFor(x => x.Password!).Password().When(x => x.Password is not null);
    }
}

// Reglas compartidas entre crear y actualizar, los largos salen de 01_tablas.sql
internal static class ReglasUsuario
{
    public static IRuleBuilderOptions<T, string> Nombre<T>(this IRuleBuilder<T, string> regla) =>
        regla.NotEmpty().MaximumLength(100);

    public static IRuleBuilderOptions<T, string> Direccion<T>(this IRuleBuilder<T, string> regla) =>
        regla.NotEmpty().MaximumLength(250);

    public static IRuleBuilderOptions<T, string> Correo<T>(this IRuleBuilder<T, string> regla) =>
        regla.NotEmpty().MaximumLength(150).EmailAddress();

    public static IRuleBuilderOptions<T, string> Telefono<T>(this IRuleBuilder<T, string> regla) =>
        regla.NotEmpty()
            .Matches(@"^\+?[0-9]{8,15}$")
            .WithMessage("El teléfono debe tener entre 8 y 15 dígitos, puede empezar con +.");

    // bcrypt solo toma en cuenta los primeros 72 bytes, por eso el máximo
    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> regla) =>
        regla.MinimumLength(8)
            .MaximumLength(72)
            .Matches("[A-Za-z]").WithMessage("La contraseña debe tener al menos una letra.")
            .Matches("[0-9]").WithMessage("La contraseña debe tener al menos un número.");

    public static IRuleBuilderOptions<T, DateOnly?> FechaNacimiento<T>(this IRuleBuilder<T, DateOnly?> regla, TimeProvider reloj) =>
        regla.NotNull()
            .Must(fecha =>
            {
                var hoy = DateOnly.FromDateTime(reloj.GetLocalNow().Date);
                return fecha < hoy && fecha > hoy.AddYears(-120);
            })
            .WithMessage("La fecha de nacimiento no es válida.");
}
