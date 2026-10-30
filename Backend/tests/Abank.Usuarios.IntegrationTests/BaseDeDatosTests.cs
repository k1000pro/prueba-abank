using Dapper;
using Npgsql;

namespace Abank.Usuarios.IntegrationTests;

// Pruebas directas sobre los scripts: secuencia, constraint y triggers
[Collection(ApiCollection.Nombre)]
public class BaseDeDatosTests(ApiFactory factory)
{
    private const string Insert = """
        INSERT INTO usuarios (nombres, apellidos, fecha_nacimiento, direccion, password, telefono, email, estado, fecha_creacion)
        VALUES ('Prueba', 'Trigger', DATE '1990-01-01', 'x', 'x', @telefono, @email, @estado, @fechaCreacion)
        RETURNING id, fecha_creacion AS FechaCreacion, fecha_modificacion AS FechaModificacion
        """;

    [Fact]
    public async Task Estado_SoloAceptaAoI()
    {
        await using var conexion = new NpgsqlConnection(factory.CadenaConexion);

        var ex = await Assert.ThrowsAsync<PostgresException>(() => conexion.QueryAsync(Insert, new
        {
            telefono = "79000001",
            email = "constraint@test.com",
            estado = "X",
            fechaCreacion = DateTime.UtcNow
        }));

        Assert.Equal("ck_usuarios_estado", ex.ConstraintName);
    }

    [Fact]
    public async Task Triggers_LlenanFechaCreacionYFechaModificacion()
    {
        await using var conexion = new NpgsqlConnection(factory.CadenaConexion);

        // se manda una fecha de creación vieja a propósito, el trigger la tiene que ignorar
        var insertado = await conexion.QuerySingleAsync<Fechas>(Insert, new
        {
            telefono = "79000002",
            email = "trigger@test.com",
            estado = "A",
            fechaCreacion = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        Assert.True(insertado.FechaCreacion > DateTime.UtcNow.AddMinutes(-5));
        Assert.Null(insertado.FechaModificacion);

        var actualizado = await conexion.QuerySingleAsync<Fechas>("""
            UPDATE usuarios SET nombres = 'Cambiado', fecha_creacion = now() - interval '10 days'
            WHERE id = @Id
            RETURNING id, fecha_creacion AS FechaCreacion, fecha_modificacion AS FechaModificacion
            """, new { insertado.Id });

        Assert.Equal(insertado.FechaCreacion, actualizado.FechaCreacion);
        Assert.NotNull(actualizado.FechaModificacion);
    }

    private record Fechas(int Id, DateTime FechaCreacion, DateTime? FechaModificacion);
}
