using Abank.Usuarios.Application.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Abank.Usuarios.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
public class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    /// <summary>Lista los usuarios activos. El total viene en el header X-Total-Count.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<UsuarioResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UsuarioResponse>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var resultado = await usuarioService.ListarAsync(page, pageSize, ct);
        Response.Headers["X-Total-Count"] = resultado.Total.ToString();
        return Ok(resultado.Usuarios);
    }

    [HttpGet("{id_user:int}")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> GetUser([FromRoute(Name = "id_user")] int id, CancellationToken ct) =>
        Ok(await usuarioService.ObtenerAsync(id, ct));

    [HttpPost]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> CreateUser(CrearUsuarioRequest request, CancellationToken ct)
    {
        var creado = await usuarioService.CrearAsync(request, ct);
        return CreatedAtAction(nameof(GetUser), new { id_user = creado.Id }, creado);
    }

    /// <summary>Actualiza un usuario. El password es opcional, si no viene se deja el actual.</summary>
    [HttpPut("{id_user:int}")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> UpdateUser(
        [FromRoute(Name = "id_user")] int id, ActualizarUsuarioRequest request, CancellationToken ct) =>
        Ok(await usuarioService.ActualizarAsync(id, request, ct));

    /// <summary>Borrado lógico: el usuario pasa a estado I y deja de aparecer en la API.</summary>
    [HttpDelete("{id_user:int}")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> DeleteUser([FromRoute(Name = "id_user")] int id, CancellationToken ct) =>
        Ok(await usuarioService.EliminarAsync(id, ct));
}
