using Abank.Usuarios.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Abank.Usuarios.UnitTests.Seguridad;

public class SeguridadTests
{
    [Fact]
    public void Hasher_VerificaElPasswordCorrectoYRechazaOtro()
    {
        var hasher = new BCryptPasswordHasher();

        var hash = hasher.Hash("Clave1234");

        Assert.NotEqual("Clave1234", hash);
        Assert.True(hasher.Verify("Clave1234", hash));
        Assert.False(hasher.Verify("Clave12345", hash));
    }

    [Fact]
    public void Hasher_ConHashCorrupto_DevuelveFalse()
    {
        Assert.False(new BCryptPasswordHasher().Verify("Clave1234", "no-es-un-hash"));
    }

    [Fact]
    public void Token_TraeLosClaimsYLaExpiracionConfigurada()
    {
        var opciones = Options.Create(new JwtOptions
        {
            Key = "llave-de-pruebas-con-mas-de-32-caracteres",
            Issuer = "tests",
            Audience = "tests",
            ExpiracionMinutos = 30
        });
        var servicio = new JwtTokenService(opciones, TimeProvider.System);

        var generado = servicio.Generar(Datos.Usuario());
        var jwt = new JsonWebToken(generado.Token);

        Assert.Equal("1", jwt.Subject);
        Assert.Equal("tests", jwt.Issuer);
        Assert.Equal("71234567", jwt.GetClaim("phone_number").Value);
        Assert.Equal(generado.Expira.ToUnixTimeSeconds(), new DateTimeOffset(jwt.ValidTo).ToUnixTimeSeconds());
    }
}
