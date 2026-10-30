using System.Text.Json.Serialization;
using Abank.Usuarios.Domain.Entities;

namespace Abank.Usuarios.Application.Usuarios;

// Los nombres de las propiedades JSON siguen los ejemplos de la prueba (fechanacimiento todo en minúscula).

public record UsuarioResponse(
    int Id,
    string Nombres,
    string Apellidos,
    [property: JsonPropertyName("fechanacimiento")] DateOnly FechaNacimiento,
    string Telefono,
    string Email,
    string Direccion)
{
    public static UsuarioResponse Desde(Usuario u) =>
        new(u.Id, u.Nombres, u.Apellidos, u.FechaNacimiento, u.Telefono, u.Email, u.Direccion);
}

public record CrearUsuarioRequest
{
    public string Nombres { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;

    [JsonPropertyName("fechanacimiento")]
    public DateOnly? FechaNacimiento { get; init; }

    public string Telefono { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
}

public record ActualizarUsuarioRequest
{
    public string Nombres { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;

    [JsonPropertyName("fechanacimiento")]
    public DateOnly? FechaNacimiento { get; init; }

    public string Telefono { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    /// <summary>Opcional. Si no se manda, se conserva el password actual.</summary>
    public string? Password { get; init; }

    public string Direccion { get; init; } = string.Empty;
}

public record PaginaUsuarios(IReadOnlyList<UsuarioResponse> Usuarios, int Total);
