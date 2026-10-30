using System.Text.Json.Serialization;
using Abank.Usuarios.Domain.Entities;

namespace Abank.Usuarios.Application.Auth;

public record LoginRequest
{
    public string Telefono { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record LoginResponse(
    [property: JsonPropertyName("user")] UsuarioLogueado User,
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);

// Respeta el formato del ejemplo del login (session_active, address).
// El hash del password no se devuelve.
public record UsuarioLogueado(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nombres")] string Nombres,
    [property: JsonPropertyName("apellidos")] string Apellidos,
    [property: JsonPropertyName("session_active")] bool SessionActive,
    [property: JsonPropertyName("fechanacimiento")] DateOnly FechaNacimiento,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("telefono")] string Telefono,
    [property: JsonPropertyName("address")] string Address)
{
    public static UsuarioLogueado Desde(Usuario u) =>
        new(u.Id, u.Nombres, u.Apellidos, u.EstaActivo, u.FechaNacimiento, u.Email, u.Telefono, u.Direccion);
}
