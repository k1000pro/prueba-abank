using Abank.Usuarios.Application.Abstractions;
using Abank.Usuarios.Application.Common;
using FluentValidation;

namespace Abank.Usuarios.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public class AuthService(
    IUsuarioRepository repositorio,
    IPasswordHasher hasher,
    ITokenService tokens,
    IValidator<LoginRequest> validador,
    TimeProvider reloj) : IAuthService
{
    // si el teléfono no existe se compara contra este hash igual, así el tiempo
    // de respuesta no delata qué teléfonos están registrados
    private const string HashDeRelleno = "$2a$12$YhFF2.Eeap4DRyoOOH7oUu3NWt1xn4ZWtphGSRZk/TmtingiKINuC";

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(request, ct);

        var usuario = await repositorio.ObtenerActivoPorTelefonoAsync(request.Telefono.Trim(), ct);

        var passwordOk = hasher.Verify(request.Password, usuario?.Password ?? HashDeRelleno);
        if (usuario is null || !passwordOk)
            throw new CredencialesInvalidasException();

        var token = tokens.Generar(usuario);
        var segundos = (int)(token.Expira - reloj.GetUtcNow()).TotalSeconds;

        return new LoginResponse(UsuarioLogueado.Desde(usuario), token.Token, "bearer", segundos);
    }
}
