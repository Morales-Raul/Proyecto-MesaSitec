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

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearSolicitudRequest request)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenantId")!.Value);
            var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var detalle = await _service.Crear(tenantId, usuarioId, request);

            return CreatedAtAction(nameof(Detalle), new { id = detalle.Id }, detalle);
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

    [HttpGet("{id}")]
    public async Task<IActionResult> Detalle(Guid id)
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")!.Value);
        var detalle = await _service.ObtenerPorId(tenantId, id);

        if (detalle == null)
            return NotFound(new
            {
                type = "https://mesasitec.local/errores/recurso-no-encontrado",
                title = "Recurso no encontrado",
                status = 404,
                detail = "La solicitud no existe o no pertenece a la organización.",
                codigo = "RECURSO_NO_ENCONTRADO"
            });

        return Ok(detalle);
    }
}   