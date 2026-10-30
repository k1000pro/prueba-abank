using System.Threading.RateLimiting;
using Abank.Usuarios.Api.Errores;
using Abank.Usuarios.Api.Extensions;
using Abank.Usuarios.Application;
using Abank.Usuarios.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorErrores>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAutenticacionJwt();
builder.Services.AddSwagger();

builder.Services.AddHealthChecks().AddNpgSql();

// Limite de intentos de login por IP para frenar fuerza bruta
var intentosLogin = builder.Configuration.GetValue("RateLimiting:LoginPorMinuto", 10);
builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opciones.AddPolicy("login", http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = intentosLogin,
            Window = TimeSpan.FromMinutes(1)
        }));
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Abank Usuarios v1");
    opciones.DocumentTitle = "Abank - API de usuarios";
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
