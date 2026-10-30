using Abank.Usuarios.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Abank.Usuarios.Api.Errores;

// Convierte las excepciones de la capa de aplicación en respuestas ProblemDetails
public class ManejadorErrores(IProblemDetailsService problemDetails, ILogger<ManejadorErrores> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        ProblemDetails problema = exception switch
        {
            ValidationException ex => new ValidationProblemDetails(
                ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Hay errores de validación."
            },
            NoEncontradoException ex => Problema(StatusCodes.Status404NotFound, "No encontrado", ex.Message),
            ConflictoException ex => Problema(StatusCodes.Status409Conflict, "Conflicto", ex.Message),
            CredencialesInvalidasException ex => Problema(StatusCodes.Status401Unauthorized, "No autorizado", ex.Message),
            _ => Problema(StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado.")
        };

        if (problema.Status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error no controlado en {Ruta}", http.Request.Path);

        http.Response.StatusCode = problema.Status!.Value;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            ProblemDetails = problema,
            Exception = exception
        });
    }

    private static ProblemDetails Problema(int status, string titulo, string detalle) =>
        new() { Status = status, Title = titulo, Detail = detalle };
}
