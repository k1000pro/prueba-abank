using Abank.Usuarios.Application.Auth;
using Abank.Usuarios.Application.Usuarios;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Abank.Usuarios.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton);

        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
