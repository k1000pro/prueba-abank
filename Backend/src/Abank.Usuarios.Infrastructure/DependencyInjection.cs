using Abank.Usuarios.Application.Abstractions;
using Abank.Usuarios.Infrastructure.Persistence;
using Abank.Usuarios.Infrastructure.Security;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Abank.Usuarios.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cadena = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'Postgres'.");

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        // un solo data source para toda la app, maneja el pool de conexiones
        services.AddSingleton(_ => NpgsqlDataSource.Create(cadena));
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.Seccion))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
