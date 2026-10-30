using System.Security.Claims;
using System.Text;
using Abank.Usuarios.Application.Abstractions;
using Abank.Usuarios.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Abank.Usuarios.Infrastructure.Security;

public class JwtTokenService(IOptions<JwtOptions> opciones, TimeProvider reloj) : ITokenService
{
    private readonly JwtOptions _jwt = opciones.Value;
    private readonly JsonWebTokenHandler _handler = new();

    public TokenGenerado Generar(Usuario usuario)
    {
        var ahora = reloj.GetUtcNow();
        var expira = ahora.AddMinutes(_jwt.ExpiracionMinutos);

        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            IssuedAt = ahora.UtcDateTime,
            NotBefore = ahora.UtcDateTime,
            Expires = expira.UtcDateTime,
            SigningCredentials = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Name, $"{usuario.Nombres} {usuario.Apellidos}"),
                new Claim(JwtRegisteredClaimNames.PhoneNumber, usuario.Telefono)
            ])
        };

        return new TokenGenerado(_handler.CreateToken(descriptor), expira);
    }
}
