namespace Abank.Usuarios.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateOnly FechaNacimiento { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Estado { get; set; } = EstadoUsuario.Activo;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public bool EstaActivo => Estado == EstadoUsuario.Activo;
}
