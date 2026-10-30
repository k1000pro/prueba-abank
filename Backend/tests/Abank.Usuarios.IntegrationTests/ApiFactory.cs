using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Abank.Usuarios.IntegrationTests;

// Levanta un Postgres en Docker y corre los mismos scripts de la carpeta BaseDeDatos
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public string CadenaConexion => _postgres.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Postgres", CadenaConexion);
        builder.UseSetting("Jwt:Key", "llave-para-tests-de-integracion-1234567890");
        builder.UseSetting("RateLimiting:LoginPorMinuto", "1000");
    }

    public async Task<HttpClient> ClienteAutenticadoAsync()
    {
        var cliente = CreateClient();
        var respuesta = await cliente.PostAsJsonAsync("/api/v1/auth/login", new { telefono = "70000000", password = "Admin12345" });
        respuesta.EnsureSuccessStatusCode();

        var login = await respuesta.Content.ReadFromJsonAsync<LoginDto>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.access_token);
        return cliente;
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await using var conexion = new NpgsqlConnection(CadenaConexion);
        await conexion.OpenAsync();

        foreach (var script in Directory.GetFiles(BuscarCarpetaScripts(), "*.sql").Order(StringComparer.Ordinal))
        {
            await using var comando = new NpgsqlCommand(await File.ReadAllTextAsync(script), conexion);
            await comando.ExecuteNonQueryAsync();
        }
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    private static string BuscarCarpetaScripts()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "BaseDeDatos")))
            dir = dir.Parent;

        return dir is null
            ? throw new DirectoryNotFoundException("No se encontró la carpeta BaseDeDatos")
            : Path.Combine(dir.FullName, "BaseDeDatos");
    }

    private record LoginDto(string access_token);
}

[CollectionDefinition(Nombre)]
public class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Nombre = "api";
}
