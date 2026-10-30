using Abank.Usuarios.Application.Abstractions;
using Abank.Usuarios.Application.Common;
using Abank.Usuarios.Domain.Entities;
using FluentValidation;

namespace Abank.Usuarios.Application.Usuarios;

public interface IUsuarioService
{
    Task<PaginaUsuarios> ListarAsync(int pagina, int tamanio, CancellationToken ct = default);
    Task<UsuarioResponse> ObtenerAsync(int id, CancellationToken ct = default);
    Task<UsuarioResponse> CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default);
    Task<UsuarioResponse> ActualizarAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default);
    Task<UsuarioResponse> EliminarAsync(int id, CancellationToken ct = default);
}

public class UsuarioService(
    IUsuarioRepository repositorio,
    IPasswordHasher hasher,
    IValidator<CrearUsuarioRequest> validadorCrear,
    IValidator<ActualizarUsuarioRequest> validadorActualizar) : IUsuarioService
{
    public const int TamanioPaginaMaximo = 100;

    public async Task<PaginaUsuarios> ListarAsync(int pagina, int tamanio, CancellationToken ct = default)
    {
        pagina = Math.Max(pagina, 1);
        tamanio = Math.Clamp(tamanio, 1, TamanioPaginaMaximo);

        var (usuarios, total) = await repositorio.ListarActivosAsync((pagina - 1) * tamanio, tamanio, ct);

        return new PaginaUsuarios(usuarios.Select(UsuarioResponse.Desde).ToList(), total);
    }

    public async Task<UsuarioResponse> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var usuario = await repositorio.ObtenerActivoPorIdAsync(id, ct) ?? throw NoExiste(id);
        return UsuarioResponse.Desde(usuario);
    }

    public async Task<UsuarioResponse> CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default)
    {
        await validadorCrear.ValidateAndThrowAsync(request, ct);

        var usuario = new Usuario
        {
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            FechaNacimiento = request.FechaNacimiento!.Value,
            Direccion = request.Direccion.Trim(),
            Telefono = request.Telefono.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Password = hasher.Hash(request.Password),
            Estado = EstadoUsuario.Activo
        };

        var creado = await repositorio.CrearAsync(usuario, ct);
        return UsuarioResponse.Desde(creado);
    }

    public async Task<UsuarioResponse> ActualizarAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default)
    {
        await validadorActualizar.ValidateAndThrowAsync(request, ct);

        var usuario = new Usuario
        {
            Id = id,
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            FechaNacimiento = request.FechaNacimiento!.Value,
            Direccion = request.Direccion.Trim(),
            Telefono = request.Telefono.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            // vacío = el repositorio deja el password como estaba
            Password = request.Password is null ? string.Empty : hasher.Hash(request.Password)
        };

        var actualizado = await repositorio.ActualizarAsync(usuario, ct) ?? throw NoExiste(id);
        return UsuarioResponse.Desde(actualizado);
    }

    public async Task<UsuarioResponse> EliminarAsync(int id, CancellationToken ct = default)
    {
        // Borrado lógico: el registro queda con estado I
        var eliminado = await repositorio.DesactivarAsync(id, ct) ?? throw NoExiste(id);
        return UsuarioResponse.Desde(eliminado);
    }

    private static NoEncontradoException NoExiste(int id) => new($"No existe un usuario activo con id {id}.");
}
