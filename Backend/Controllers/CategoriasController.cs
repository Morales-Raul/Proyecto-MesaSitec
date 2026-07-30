using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Aplicacion;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("categorias")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly CategoriasService _service;

    public CategoriasController(CategoriasService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")!.Value);
        var categorias = await _service.Listar(tenantId);
        return Ok(categorias);
    }
}