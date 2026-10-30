using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Abank.Usuarios.IntegrationTests;

[Collection(ApiCollection.Nombre)]
public class AuthTests(ApiFactory factory)
{
    private readonly HttpClient _cliente = factory.CreateClient();

    [Fact]
    public async Task Login_ConDatosDelSeed_DevuelveTokenConElFormatoDelEnunciado()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { telefono = "70000000", password = "Admin12345" });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        var json = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("bearer", json.GetProperty("token_type").GetString());
        Assert.False(string.IsNullOrEmpty(json.GetProperty("access_token").GetString()));

        var user = json.GetProperty("user");
        Assert.True(user.GetProperty("session_active").GetBoolean());
        Assert.Equal("1990-01-15", user.GetProperty("fechanacimiento").GetString());
        Assert.True(user.TryGetProperty("address", out _));
        Assert.False(user.TryGetProperty("password", out _));
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecto_Devuelve401()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { telefono = "70000000", password = "otra-clave" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Login_UsuarioInactivo_Devuelve401()
    {
        // 76543210 está en el seed con estado I
        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { telefono = "76543210", password = "Prueba123" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Login_SinBody_Devuelve400()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
