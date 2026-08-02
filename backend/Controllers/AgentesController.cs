using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Datos;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("usuarios/agentes")]
[Authorize]
public class AgentesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AgentesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> ListarAgentes()
    {
        var tenantId = Guid.Parse(User.FindFirst("tenantId")!.Value);
        var agentes = await _db.Usuarios
            .Where(u => u.TenantId == tenantId && u.Activo && (u.Rol == Modelos.Rol.Agente || u.Rol == Modelos.Rol.Admin))
            .Select(u => new { u.Id, u.Nombre })
            .ToListAsync();

        return Ok(agentes);
    }
}