using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Datos;

namespace Api.Controllers;

[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    private readonly AppDbContext _db;
    public DebugController(AppDbContext db) => _db = db;

    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants()
    {
        var tenants = await _db.Tenants.Include(t => t.Usuarios).ToListAsync();
        return Ok(tenants.Select(t => new { t.Id, t.Nombre, Usuarios = t.Usuarios.Select(u => u.Email) }));
    }

    [HttpGet("solicitudes/count")]
    public async Task<IActionResult> GetCount()
    {
        var total = await _db.Solicitudes.CountAsync();
        return Ok(new { total });
    }
}