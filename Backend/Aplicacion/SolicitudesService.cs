using Microsoft.EntityFrameworkCore;
using Api.Datos;
using Api.Modelos;

namespace Api.Aplicacion;

public class SolicitudesService
{
    private readonly AppDbContext _db;

    public SolicitudesService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SolicitudesResponse> Listar(
        Guid tenantId,
        string? rol,
        Guid? usuarioId,
        string? estado,
        string? prioridad,
        Guid? categoriaId,
        Guid? agenteId,
        string? q,
        bool? vencidas,
        int page,
        int pageSize,
        string? sort)
    {
        // Validar paginación
        if (page < 1)
            throw new ArgumentException("La página debe ser mayor o igual a 1.");
        if (pageSize > 100)
            throw new ArgumentException("El tamaño de página máximo es 100.");

        // Base query filtrada por tenant
        var query = _db.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .Where(s => s.TenantId == tenantId);

        // Si el rol es Solicitante, solo ve sus propias solicitudes
        if (rol == "Solicitante" && usuarioId.HasValue)
        {
            query = query.Where(s => s.SolicitanteId == usuarioId.Value);
        }

        // Filtros exactos
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoSolicitud>(estado, out var estadoEnum))
            query = query.Where(s => s.Estado == estadoEnum);

        if (!string.IsNullOrWhiteSpace(prioridad) && Enum.TryParse<Prioridad>(prioridad, out var prioridadEnum))
            query = query.Where(s => s.Prioridad == prioridadEnum);

        if (categoriaId.HasValue)
            query = query.Where(s => s.CategoriaId == categoriaId.Value);

        if (agenteId.HasValue)
            query = query.Where(s => s.AgenteId == agenteId.Value);

        // Búsqueda en título, descripción y código (case-insensitive)
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.ToLower();
            query = query.Where(s =>
                s.Titulo.ToLower().Contains(q) ||
                s.Descripcion.ToLower().Contains(q) ||
                s.Codigo.ToLower().Contains(q));
        }

        // Filtro de vencidas
        if (vencidas == true)
        {
            var ahora = DateTime.UtcNow;
            query = query.Where(s =>
                s.FechaLimiteSla < ahora &&
                s.Estado != EstadoSolicitud.Resuelta &&
                s.Estado != EstadoSolicitud.Cerrada &&
                s.Estado != EstadoSolicitud.Cancelada);
        }

        // Ordenamiento
        query = sort switch
        {
            "fechaCreacion" => query.OrderBy(s => s.FechaCreacion),
            "-fechaCreacion" => query.OrderByDescending(s => s.FechaCreacion),
            "prioridad" => query.OrderBy(s => s.Prioridad),
            "-prioridad" => query.OrderByDescending(s => s.Prioridad),
            "codigo" => query.OrderBy(s => s.Codigo),
            _ => query.OrderByDescending(s => s.FechaCreacion) // default
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SolicitudItem
            {
                Id = s.Id,
                Codigo = s.Codigo,
                Titulo = s.Titulo,
                Estado = s.Estado.ToString(),
                Prioridad = s.Prioridad.ToString(),
                Categoria = new CategoriaItem { Id = s.Categoria.Id, Nombre = s.Categoria.Nombre },
                Agente = s.Agente == null ? null : new AgenteItem { Id = s.Agente.Id, Nombre = s.Agente.Nombre },
                FechaCreacion = s.FechaCreacion,
                FechaLimiteSla = s.FechaLimiteSla,
                Vencida = s.FechaLimiteSla < DateTime.UtcNow &&
                          s.Estado != EstadoSolicitud.Resuelta &&
                          s.Estado != EstadoSolicitud.Cerrada &&
                          s.Estado != EstadoSolicitud.Cancelada
            })
            .ToListAsync();

        return new SolicitudesResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)pageSize)
        };
    }
}

public class SolicitudesResponse
{
    public List<SolicitudItem> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPaginas { get; set; }
}

public class SolicitudItem
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Estado { get; set; } = "";
    public string Prioridad { get; set; } = "";
    public CategoriaItem Categoria { get; set; } = null!;
    public AgenteItem? Agente { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaLimiteSla { get; set; }
    public bool Vencida { get; set; }
}

public class CategoriaItem
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class AgenteItem
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
}