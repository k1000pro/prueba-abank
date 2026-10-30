using Abank.Usuarios.Domain.Entities;

namespace Abank.Usuarios.Application.Abstractions;

public interface IUsuarioRepository
{
    Task<(IReadOnlyList<Usuario> Usuarios, int Total)> ListarActivosAsync(int offset, int limite, CancellationToken ct = default);

    Task<Usuario?> ObtenerActivoPorIdAsync(int id, CancellationToken ct = default);

    Task<Usuario?> ObtenerActivoPorTelefonoAsync(string telefono, CancellationToken ct = default);

    Task<Usuario> CrearAsync(Usuario usuario, CancellationToken ct = default);

    /// <summary>
    /// Si <see cref="Usuario.Password"/> viene vacío se mantiene el que ya tenía.
    /// Devuelve null si el usuario no existe o está inactivo.
    /// </summary>
    Task<Usuario?> ActualizarAsync(Usuario usuario, CancellationToken ct = default);

    Task<Usuario?> DesactivarAsync(int id, CancellationToken ct = default);
}
