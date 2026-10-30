using Abank.Usuarios.Application.Abstractions;
using Abank.Usuarios.Application.Common;
using Abank.Usuarios.Domain.Entities;
using Dapper;
using Npgsql;

namespace Abank.Usuarios.Infrastructure.Persistence;

public class UsuarioRepository(NpgsqlDataSource dataSource) : IUsuarioRepository
{
    private const string Columnas = """
        id, nombres, apellidos, fecha_nacimiento AS FechaNacimiento, direccion, password,
        telefono, email, estado, fecha_creacion AS FechaCreacion, fecha_modificacion AS FechaModificacion
        """;

    public async Task<(IReadOnlyList<Usuario> Usuarios, int Total)> ListarActivosAsync(int offset, int limite, CancellationToken ct = default)
    {
        // página y total en un solo viaje a la base
        const string sql = $"""
            SELECT {Columnas} FROM usuarios WHERE estado = 'A' ORDER BY id LIMIT @limite OFFSET @offset;
            SELECT count(*)::int FROM usuarios WHERE estado = 'A';
            """;

        await using var conexion = await dataSource.OpenConnectionAsync(ct);
        await using var resultado = await conexion.QueryMultipleAsync(
            new CommandDefinition(sql, new { limite, offset }, cancellationToken: ct));

        var usuarios = (await resultado.ReadAsync<Usuario>()).AsList();
        var total = await resultado.ReadSingleAsync<int>();

        return (usuarios, total);
    }

    public Task<Usuario?> ObtenerActivoPorIdAsync(int id, CancellationToken ct = default) =>
        UnoAsync($"SELECT {Columnas} FROM usuarios WHERE id = @id AND estado = 'A'", new { id }, ct);

    public Task<Usuario?> ObtenerActivoPorTelefonoAsync(string telefono, CancellationToken ct = default) =>
        UnoAsync($"SELECT {Columnas} FROM usuarios WHERE telefono = @telefono AND estado = 'A'", new { telefono }, ct);

    public async Task<Usuario> CrearAsync(Usuario usuario, CancellationToken ct = default)
    {
        const string sql = $"""
            INSERT INTO usuarios (nombres, apellidos, fecha_nacimiento, direccion, password, telefono, email, estado)
            VALUES (@Nombres, @Apellidos, @FechaNacimiento, @Direccion, @Password, @Telefono, @Email, @Estado)
            RETURNING {Columnas}
            """;

        return (await UnoAsync(sql, usuario, ct))!;
    }

    public Task<Usuario?> ActualizarAsync(Usuario usuario, CancellationToken ct = default)
    {
        const string sql = $"""
            UPDATE usuarios SET
                nombres = @Nombres,
                apellidos = @Apellidos,
                fecha_nacimiento = @FechaNacimiento,
                direccion = @Direccion,
                telefono = @Telefono,
                email = @Email,
                password = COALESCE(NULLIF(@Password, ''), password)
            WHERE id = @Id AND estado = 'A'
            RETURNING {Columnas}
            """;

        return UnoAsync(sql, usuario, ct);
    }

    public Task<Usuario?> DesactivarAsync(int id, CancellationToken ct = default) =>
        UnoAsync($"UPDATE usuarios SET estado = 'I' WHERE id = @id AND estado = 'A' RETURNING {Columnas}", new { id }, ct);

    private async Task<Usuario?> UnoAsync(string sql, object parametros, CancellationToken ct)
    {
        await using var conexion = await dataSource.OpenConnectionAsync(ct);

        try
        {
            return await conexion.QuerySingleOrDefaultAsync<Usuario>(
                new CommandDefinition(sql, parametros, cancellationToken: ct));
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // Dejamos que la base valide los duplicados con los índices únicos en vez
            // de consultar antes, así no hay carrera entre dos requests al mismo tiempo.
            throw ex.ConstraintName switch
            {
                "ux_usuarios_telefono" => new ConflictoException("Ya existe un usuario activo con ese teléfono."),
                "ux_usuarios_email" => new ConflictoException("Ya existe un usuario activo con ese email."),
                _ => new ConflictoException("El usuario ya existe.")
            };
        }
    }
}
