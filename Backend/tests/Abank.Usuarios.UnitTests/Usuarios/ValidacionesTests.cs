using Abank.Usuarios.Application.Usuarios;
using Microsoft.Extensions.Time.Testing;

namespace Abank.Usuarios.UnitTests.Usuarios;

public class ValidacionesTests
{
    private readonly CrearUsuarioValidator _validador =
        new(new FakeTimeProvider(new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero)));

    [Fact]
    public void RequestCompleto_EsValido()
    {
        Assert.True(_validador.Validate(Datos.CrearRequest()).IsValid);
    }

    [Theory]
    [InlineData("7123")]
    [InlineData("7123-4567")]
    [InlineData("telefono1")]
    public void Telefono_ConFormatoInvalido_NoPasa(string telefono)
    {
        var resultado = _validador.Validate(Datos.CrearRequest() with { Telefono = telefono });

        Assert.Contains(resultado.Errors, e => e.PropertyName == "Telefono");
    }

    [Theory]
    [InlineData("71234567")]
    [InlineData("+50371234567")]
    public void Telefono_Valido_Pasa(string telefono)
    {
        Assert.True(_validador.Validate(Datos.CrearRequest() with { Telefono = telefono }).IsValid);
    }

    [Theory]
    [InlineData("corta1")]
    [InlineData("solamenteletras")]
    [InlineData("1234567890")]
    public void Password_Debil_NoPasa(string password)
    {
        var resultado = _validador.Validate(Datos.CrearRequest() with { Password = password });

        Assert.Contains(resultado.Errors, e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("2026-10-29")]
    [InlineData("2030-01-01")]
    [InlineData("1890-01-01")]
    public void FechaNacimiento_FueraDeRango_NoPasa(string fecha)
    {
        var resultado = _validador.Validate(Datos.CrearRequest() with { FechaNacimiento = DateOnly.Parse(fecha) });

        Assert.Contains(resultado.Errors, e => e.PropertyName == "FechaNacimiento");
    }

    [Fact]
    public void Actualizar_SinPassword_EsValido()
    {
        var validador = new ActualizarUsuarioValidator(TimeProvider.System);

        Assert.True(validador.Validate(Datos.ActualizarRequest()).IsValid);
    }
}
