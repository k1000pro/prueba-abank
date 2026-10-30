using Abank.Usuarios.Application.Usuarios;
using Abank.Usuarios.Domain.Entities;

namespace Abank.Usuarios.UnitTests;

internal static class Datos
{
    public static CrearUsuarioRequest CrearRequest() => new()
    {
        Nombres = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateOnly(1996, 5, 10),
        Telefono = "71234567",
        Email = "juan@correo.com",
        Password = "Clave1234",
        Direccion = "San Salvador"
    };

    public static ActualizarUsuarioRequest ActualizarRequest() => new()
    {
        Nombres = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateOnly(1996, 5, 10),
        Telefono = "71234567",
        Email = "juan@correo.com",
        Direccion = "San Salvador"
    };

    public static Usuario Usuario(string password = "hash") => new()
    {
        Id = 1,
        Nombres = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateOnly(1996, 5, 10),
        Telefono = "71234567",
        Email = "juan@correo.com",
        Direccion = "San Salvador",
        Password = password,
        Estado = EstadoUsuario.Activo
    };
}
