using Microsoft.EntityFrameworkCore;
using Api.Datos;

namespace Api.Aplicacion;

public class CategoriasService
{
    private readonly AppDbContext _db;

    public CategoriasService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoriaDto>> Listar(Guid tenantId)
    {
        return await _db.Categorias
            .Where(c => c.TenantId == tenantId && c.Activo)
            .Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                SlaHoras = c.SlaHoras
            })
            .ToListAsync();
    }
}

public class CategoriaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public int SlaHoras { get; set; }
}