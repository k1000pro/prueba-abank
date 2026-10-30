using System.ComponentModel.DataAnnotations;

namespace Abank.Usuarios.Infrastructure.Security;

public class JwtOptions
{
    public const string Seccion = "Jwt";

    // HS256 necesita una llave de al menos 256 bits
    [Required, MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiracionMinutos { get; set; } = 60;
}
