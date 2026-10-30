using Abank.Usuarios.Application.Abstractions;
using Abank.Usuarios.Application.Common;
using Abank.Usuarios.Application.Usuarios;
using Abank.Usuarios.Domain.Entities;
using FluentValidation;
using NSubstitute;

namespace Abank.Usuarios.UnitTests.Usuarios;

public class UsuarioServiceTests
{
    private readonly IUsuarioRepository _repositorio = Substitute.For<IUsuarioRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _hasher.Hash(Arg.Any<string>()).Returns(x => $"hash:{x.Arg<string>()}");

        _service = new UsuarioService(
            _repositorio,
            _hasher,
            new CrearUsuarioValidator(TimeProvider.System),
            new ActualizarUsuarioValidator(TimeProvider.System));
    }

    [Fact]
    public async Task Crear_GuardaPasswordHasheadoYEmailEnMinuscula()
    {
        _repositorio.CrearAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>())
            .Returns(x => { var u = x.Arg<Usuario>(); u.Id = 10; return u; });

        var request = Datos.CrearRequest() with { Email = "  Test@Gmail.COM ", Password = "Secreta123" };

        var creado = await _service.CrearAsync(request);

        Assert.Equal(10, creado.Id);
        Assert.Equal("test@gmail.com", creado.Email);
        await _repositorio.Received(1).CrearAsync(
            Arg.Is<Usuario>(u => u.Password == "hash:Secreta123" && u.Estado == EstadoUsuario.Activo),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_ConDatosInvalidos_NoLlegaAlRepositorio()
    {
        var request = Datos.CrearRequest() with { Email = "no-es-email", Password = "123" };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CrearAsync(request));

        Assert.Contains(ex.Errors, e => e.PropertyName == "Email");
        Assert.Contains(ex.Errors, e => e.PropertyName == "Password");
        await _repositorio.DidNotReceiveWithAnyArgs().CrearAsync(default!, default);
    }

    [Fact]
    public async Task Obtener_SiNoExiste_LanzaNoEncontrado()
    {
        _repositorio.ObtenerActivoPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        await Assert.ThrowsAsync<NoEncontradoException>(() => _service.ObtenerAsync(99));
    }

    [Fact]
    public async Task Actualizar_SinPassword_NoLoHashea()
    {
        _repositorio.ActualizarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>())
            .Returns(x => x.Arg<Usuario>());

        var request = Datos.ActualizarRequest() with { Password = null };

        await _service.ActualizarAsync(5, request);

        _hasher.DidNotReceiveWithAnyArgs().Hash(default!);
        await _repositorio.Received(1).ActualizarAsync(
            Arg.Is<Usuario>(u => u.Id == 5 && u.Password == string.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_ConPassword_LoHashea()
    {
        _repositorio.ActualizarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>())
            .Returns(x => x.Arg<Usuario>());

        await _service.ActualizarAsync(5, Datos.ActualizarRequest() with { Password = "NuevaClave1" });

        await _repositorio.Received(1).ActualizarAsync(
            Arg.Is<Usuario>(u => u.Password == "hash:NuevaClave1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Eliminar_SiNoExiste_LanzaNoEncontrado()
    {
        _repositorio.DesactivarAsync(3, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        await Assert.ThrowsAsync<NoEncontradoException>(() => _service.EliminarAsync(3));
    }

    [Theory]
    [InlineData(0, 0, 0, 1)]
    [InlineData(3, 20, 40, 20)]
    [InlineData(1, 500, 0, UsuarioService.TamanioPaginaMaximo)]
    public async Task Listar_CalculaOffsetYLimitaElTamanio(int pagina, int tamanio, int offsetEsperado, int limiteEsperado)
    {
        _repositorio.ListarActivosAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<Usuario>(), 0));

        await _service.ListarAsync(pagina, tamanio);

        await _repositorio.Received(1).ListarActivosAsync(offsetEsperado, limiteEsperado, Arg.Any<CancellationToken>());
    }
}
