using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Aplicacion;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("solicitudes")]
[Authorize]
public class SolicitudesController : ControllerBase
{
    private readonly SolicitudesService _service;

    public SolicitudesController(SolicitudesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? estado,
        [FromQuery] string? prioridad,
        [FromQuery] Guid? categoriaId,
        [FromQuery] Guid? agenteId,
        [FromQuery] string? q,
        [FromQuery] bool? vencidas,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = "-fechaCreacion")
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenantId")!.Value);
            var rol = User.FindFirst("rol")!.Value;
            var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var response = await _service.Listar(
                tenantId,
                rol,
                usuarioId,
                estado,
                prioridad,
                categoriaId,
                agenteId,
                q,
                vencidas,
                page,
                pageSize,
                sort);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                type = "https://mesasitec.local/errores/parametro-invalido",
                title = "Parámetro inválido",
                status = 400,
                detail = ex.Message,
                codigo = "PARAMETRO_INVALIDO"
            });
        }
    }
}