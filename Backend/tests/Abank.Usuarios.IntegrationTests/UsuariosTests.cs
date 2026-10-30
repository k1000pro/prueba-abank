using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Abank.Usuarios.IntegrationTests;

[Collection(ApiCollection.Nombre)]
public class UsuariosTests(ApiFactory factory)
{
    [Fact]
    public async Task GetUsers_SinToken_ListaSoloActivos()
    {
        var respuesta = await factory.CreateClient().GetAsync("/api/v1/users?pageSize=100");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.True(respuesta.Headers.Contains("X-Total-Count"));

        var usuarios = await respuesta.Content.ReadFromJsonAsync<JsonElement[]>();
        Assert.NotEmpty(usuarios!);
        Assert.DoesNotContain(usuarios!, u => u.GetProperty("telefono").GetString() == "76543210");
    }

    [Theory]
    [InlineData("GET", "/api/v1/users/1")]
    [InlineData("POST", "/api/v1/users")]
    [InlineData("PUT", "/api/v1/users/1")]
    [InlineData("DELETE", "/api/v1/users/1")]
    public async Task EndpointsProtegidos_SinToken_Devuelven401(string metodo, string url)
    {
        var respuesta = await factory.CreateClient().SendAsync(new HttpRequestMessage(new HttpMethod(metodo), url));

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crud_CompletoDeUnUsuario()
    {
        var cliente = await factory.ClienteAutenticadoAsync();

        var nuevo = new
        {
            nombres = "test",
            apellidos = "test",
            fechanacimiento = "1996-05-10",
            telefono = "73212656",
            email = "Test.Crud@Gmail.com",
            password = "Prueba123",
            direccion = "direccion"
        };

        // crear
        var creado = await cliente.PostAsJsonAsync("/api/v1/users", nuevo);
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        Assert.NotNull(creado.Headers.Location);

        var usuario = await creado.Content.ReadFromJsonAsync<JsonElement>();
        var id = usuario.GetProperty("id").GetInt32();
        Assert.Equal("test.crud@gmail.com", usuario.GetProperty("email").GetString());
        Assert.False(usuario.TryGetProperty("password", out _));

        // mismo teléfono otra vez
        var duplicado = await cliente.PostAsJsonAsync("/api/v1/users", nuevo with { email = "otro@gmail.com" });
        Assert.Equal(HttpStatusCode.Conflict, duplicado.StatusCode);

        // el usuario nuevo puede hacer login
        var login = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login", new { telefono = "73212656", password = "Prueba123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        // actualizar sin mandar password
        var actualizado = await cliente.PutAsJsonAsync($"/api/v1/users/{id}", new
        {
            nombres = "test editado",
            apellidos = "test",
            fechanacimiento = "1996-05-10",
            telefono = "73212656",
            email = "test.crud@gmail.com",
            direccion = "otra direccion"
        });
        Assert.Equal(HttpStatusCode.OK, actualizado.StatusCode);
        Assert.Equal("test editado", (await actualizado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("nombres").GetString());

        // el password anterior tiene que seguir funcionando
        login = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login", new { telefono = "73212656", password = "Prueba123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var obtenido = await cliente.GetAsync($"/api/v1/users/{id}");
        Assert.Equal(HttpStatusCode.OK, obtenido.StatusCode);

        // eliminar
        var eliminado = await cliente.DeleteAsync($"/api/v1/users/{id}");
        Assert.Equal(HttpStatusCode.OK, eliminado.StatusCode);

        Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync($"/api/v1/users/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await cliente.DeleteAsync($"/api/v1/users/{id}")).StatusCode);
    }

    [Fact]
    public async Task Crear_ConDatosInvalidos_Devuelve400ConLosErrores()
    {
        var cliente = await factory.ClienteAutenticadoAsync();

        var respuesta = await cliente.PostAsJsonAsync("/api/v1/users", new { nombres = "", email = "malo", password = "123" });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problema.GetProperty("errors").TryGetProperty("email", out _));
    }

    [Fact]
    public async Task GetUser_IdInexistente_Devuelve404()
    {
        var cliente = await factory.ClienteAutenticadoAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync("/api/v1/users/999999")).StatusCode);
    }
}
