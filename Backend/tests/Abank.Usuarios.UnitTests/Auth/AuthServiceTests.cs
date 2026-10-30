using Abank.Usuarios.Application.Abstractions;
using Abank.Usuarios.Application.Auth;
using Abank.Usuarios.Application.Common;
using Abank.Usuarios.Domain.Entities;
using FluentValidation;
using NSubstitute;

namespace Abank.Usuarios.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly IUsuarioRepository _repositorio = Substitute.For<IUsuarioRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _tokens.Generar(Arg.Any<Usuario>())
            .Returns(new TokenGenerado("token-falso", DateTimeOffset.UtcNow.AddHours(1)));

        _service = new AuthService(_repositorio, _hasher, _tokens, new LoginValidator(), TimeProvider.System);
    }

    [Fact]
    public async Task Login_ConCredencialesCorrectas_DevuelveToken()
    {
        _repositorio.ObtenerActivoPorTelefonoAsync("71234567", Arg.Any<CancellationToken>())
            .Returns(Datos.Usuario("hash-guardado"));
        _hasher.Verify("Clave1234", "hash-guardado").Returns(true);

        var respuesta = await _service.LoginAsync(new LoginRequest { Telefono = "71234567", Password = "Clave1234" });

        Assert.Equal("token-falso", respuesta.AccessToken);
        Assert.Equal("bearer", respuesta.TokenType);
        Assert.True(respuesta.User.SessionActive);
        Assert.InRange(respuesta.ExpiresIn, 3500, 3600);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecto_LanzaCredencialesInvalidas()
    {
        _repositorio.ObtenerActivoPorTelefonoAsync("71234567", Arg.Any<CancellationToken>())
            .Returns(Datos.Usuario("hash-guardado"));
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        await Assert.ThrowsAsync<CredencialesInvalidasException>(() =>
            _service.LoginAsync(new LoginRequest { Telefono = "71234567", Password = "otra" }));

        _tokens.DidNotReceiveWithAnyArgs().Generar(default!);
    }

    [Fact]
    public async Task Login_TelefonoInexistente_IgualVerificaPassword()
    {
        _repositorio.ObtenerActivoPorTelefonoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Usuario?)null);

        await Assert.ThrowsAsync<CredencialesInvalidasException>(() =>
            _service.LoginAsync(new LoginRequest { Telefono = "79999999", Password = "Clave1234" }));

        // se compara contra el hash de relleno para no responder más rápido
        _hasher.Received(1).Verify("Clave1234", Arg.Any<string>());
    }

    [Fact]
    public async Task Login_SinDatos_FallaValidacion()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _service.LoginAsync(new LoginRequest()));
    }
}
