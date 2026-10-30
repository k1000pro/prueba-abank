namespace Abank.Usuarios.Application.Common;

public class NoEncontradoException(string mensaje) : Exception(mensaje);

public class ConflictoException(string mensaje) : Exception(mensaje);

public class CredencialesInvalidasException() : Exception("Teléfono o contraseña incorrectos.");
