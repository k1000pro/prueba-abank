using Abank.Usuarios.Application.Abstractions;

namespace Abank.Usuarios.Infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    private const int Costo = 12;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, Costo);

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // hash mal formado en la base, se trata como password incorrecto
            return false;
        }
    }
}
