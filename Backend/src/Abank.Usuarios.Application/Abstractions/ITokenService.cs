using Abank.Usuarios.Domain.Entities;

namespace Abank.Usuarios.Application.Abstractions;

public interface ITokenService
{
    TokenGenerado Generar(Usuario usuario);
}

public record TokenGenerado(string Token, DateTimeOffset Expira);
