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
        if (page < 1)
            throw new ArgumentException("La página debe ser mayor o igual a 1.");
        if (pageSize > 100)
            throw new ArgumentException("El tamaño de página máximo es 100.");

        var query = _db.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .Where(s => s.TenantId == tenantId);

        if (rol == "Solicitante" && usuarioId.HasValue)
            query = query.Where(s => s.SolicitanteId == usuarioId.Value);

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoSolicitud>(estado, out var estadoEnum))
            query = query.Where(s => s.Estado == estadoEnum);

        if (!string.IsNullOrWhiteSpace(prioridad) && Enum.TryParse<Prioridad>(prioridad, out var prioridadEnum))
            query = query.Where(s => s.Prioridad == prioridadEnum);

        if (categoriaId.HasValue)
            query = query.Where(s => s.CategoriaId == categoriaId.Value);

        if (agenteId.HasValue)
            query = query.Where(s => s.AgenteId == agenteId.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.ToLower();
            query = query.Where(s =>
                s.Titulo.ToLower().Contains(q) ||
                s.Descripcion.ToLower().Contains(q) ||
                s.Codigo.ToLower().Contains(q));
        }

        if (vencidas == true)
        {
            var ahora = DateTime.UtcNow;
            query = query.Where(s =>
                s.FechaLimiteSla < ahora &&
                s.Estado != EstadoSolicitud.Resuelta &&
                s.Estado != EstadoSolicitud.Cerrada &&
                s.Estado != EstadoSolicitud.Cancelada);
        }

        query = sort switch
        {
            "fechaCreacion" => query.OrderBy(s => s.FechaCreacion),
            "-fechaCreacion" => query.OrderByDescending(s => s.FechaCreacion),
            "prioridad" => query.OrderBy(s => s.Prioridad),
            "-prioridad" => query.OrderByDescending(s => s.Prioridad),
            "codigo" => query.OrderBy(s => s.Codigo),
            _ => query.OrderByDescending(s => s.FechaCreacion)
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

    public async Task<SolicitudDetalle> Crear(Guid tenantId, Guid solicitanteId, CrearSolicitudRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo) || request.Titulo.Length < 5 || request.Titulo.Length > 120)
            throw new ArgumentException("El título debe tener entre 5 y 120 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Descripcion) || request.Descripcion.Length < 10 || request.Descripcion.Length > 4000)
            throw new ArgumentException("La descripción debe tener entre 10 y 4000 caracteres.");

        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == request.CategoriaId && c.TenantId == tenantId && c.Activo);
        if (categoria == null)
            throw new ArgumentException("La categoría no existe o no pertenece a la organización.");

        var ahora = DateTime.UtcNow;
        var factor = request.Prioridad switch
        {
            Prioridad.Critica => 0.5,
            Prioridad.Alta => 0.75,
            Prioridad.Media => 1.0,
            Prioridad.Baja => 2.0,
            _ => 1.0
        };
        var fechaLimiteSla = ahora.AddHours(categoria.SlaHoras * factor);

        var año = ahora.Year;
        var correlativo = await _db.Solicitudes
            .Where(s => s.TenantId == tenantId && s.FechaCreacion.Year == año)
            .CountAsync() + 1;
        var codigo = $"SOL-{año}-{correlativo:D5}";

        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = codigo,
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            CategoriaId = categoria.Id,
            Prioridad = request.Prioridad,
            Estado = EstadoSolicitud.Nueva,
            SolicitanteId = solicitanteId,
            AgenteId = null,
            FechaCreacion = ahora,
            FechaLimiteSla = fechaLimiteSla,
            FechaResolucion = null,
            MotivoResolucion = null,
            MotivoCancelacion = null
        };

        _db.Solicitudes.Add(solicitud);
        await _db.SaveChangesAsync();

        var creada = await _db.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Solicitante)
            .FirstAsync(s => s.Id == solicitud.Id);

        return MapToDetalle(creada);
    }

    public async Task<SolicitudDetalle?> Editar(Guid tenantId, Guid usuarioId, string rol, Guid id, EditarSolicitudRequest request)
    {
        var solicitud = await _db.Solicitudes
         .Include(s => s.Categoria)
         .Include(s => s.Solicitante)   // ← agregar esta línea
         .Include(s => s.Agente)
         .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

        if (solicitud == null)
            return null;

        // RN-03: Solicitante solo edita las propias y en estado Nueva
        if (rol == "Solicitante" && (solicitud.SolicitanteId != usuarioId || solicitud.Estado != EstadoSolicitud.Nueva))
            throw new UnauthorizedAccessException("No tenés permiso para editar esta solicitud.");

        // Validaciones de longitud
        if (string.IsNullOrWhiteSpace(request.Titulo) || request.Titulo.Length < 5 || request.Titulo.Length > 120)
            throw new ArgumentException("El título debe tener entre 5 y 120 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Descripcion) || request.Descripcion.Length < 10 || request.Descripcion.Length > 4000)
            throw new ArgumentException("La descripción debe tener entre 10 y 4000 caracteres.");

        // Validar categoría
        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == request.CategoriaId && c.TenantId == tenantId && c.Activo);
        if (categoria == null)
            throw new ArgumentException("La categoría no existe o no pertenece a la organización.");

        // Detectar si cambió categoría o prioridad para recalcular SLA
        var cambioCategoria = solicitud.CategoriaId != categoria.Id;
        var cambioPrioridad = solicitud.Prioridad != request.Prioridad;

        solicitud.Titulo = request.Titulo;
        solicitud.Descripcion = request.Descripcion;
        solicitud.CategoriaId = categoria.Id;
        solicitud.Prioridad = request.Prioridad;

        if ((cambioCategoria || cambioPrioridad) &&
            solicitud.Estado != EstadoSolicitud.Resuelta &&
            solicitud.Estado != EstadoSolicitud.Cerrada &&
            solicitud.Estado != EstadoSolicitud.Cancelada)
        {
            var factor = request.Prioridad switch
            {
                Prioridad.Critica => 0.5,
                Prioridad.Alta => 0.75,
                Prioridad.Media => 1.0,
                Prioridad.Baja => 2.0,
                _ => 1.0
            };
            solicitud.FechaLimiteSla = solicitud.FechaCreacion.AddHours(categoria.SlaHoras * factor);
        }

        await _db.SaveChangesAsync();

        return MapToDetalle(solicitud);
    }

    public async Task<SolicitudDetalle?> ObtenerPorId(Guid tenantId, Guid id)
    {
        var solicitud = await _db.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Solicitante)
            .Include(s => s.Agente)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

        return solicitud == null ? null : MapToDetalle(solicitud);
    }

    private SolicitudDetalle MapToDetalle(Solicitud s)
    {
        return new SolicitudDetalle
        {
            Id = s.Id,
            Codigo = s.Codigo,
            Titulo = s.Titulo,
            Descripcion = s.Descripcion,
            Estado = s.Estado.ToString(),
            Prioridad = s.Prioridad.ToString(),
            Categoria = new CategoriaItem { Id = s.Categoria.Id, Nombre = s.Categoria.Nombre },
            Solicitante = new UsuarioItem { Id = s.Solicitante.Id, Nombre = s.Solicitante.Nombre },
            Agente = s.Agente == null ? null : new AgenteItem { Id = s.Agente.Id, Nombre = s.Agente.Nombre },
            FechaCreacion = s.FechaCreacion,
            FechaLimiteSla = s.FechaLimiteSla,
            FechaResolucion = s.FechaResolucion,
            MotivoResolucion = s.MotivoResolucion,
            MotivoCancelacion = s.MotivoCancelacion
        };
    }

    public async Task<SolicitudDetalle> EjecutarTransicion(Guid tenantId, Guid usuarioId, string rol, Guid id, TransicionRequest request)
{
    var solicitud = await _db.Solicitudes
        .Include(s => s.Categoria)
        .Include(s => s.Solicitante)
        .Include(s => s.Agente)
        .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

    if (solicitud == null)
        throw new KeyNotFoundException("Solicitud no encontrada.");

    // Validación adicional de RN-05 si la acción es 'asignar'
    if (request.Accion == "asignar")
    {
        if (request.AgenteId == null)
            throw new ArgumentException("Se requiere un agenteId para la acción 'asignar'.");

        var agente = await _db.Usuarios.FirstOrDefaultAsync(u =>
            u.Id == request.AgenteId.Value &&
            u.TenantId == tenantId &&
            u.Activo &&
            (u.Rol == Rol.Agente || u.Rol == Rol.Admin));

        if (agente == null)
            throw new AgenteInvalidoException();
    }

    var maquina = new MaquinaEstados();
    maquina.Ejecutar(solicitud, request.Accion, rol, usuarioId, request.AgenteId, request.Motivo);

    await _db.SaveChangesAsync();

    return MapToDetalle(solicitud);
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

public class CrearSolicitudRequest
{
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public Guid CategoriaId { get; set; }
    public Prioridad Prioridad { get; set; }
}

public class EditarSolicitudRequest
{
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public Guid CategoriaId { get; set; }
    public Prioridad Prioridad { get; set; }
}

public class SolicitudDetalle
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Estado { get; set; } = "";
    public string Prioridad { get; set; } = "";
    public CategoriaItem Categoria { get; set; } = null!;
    public UsuarioItem Solicitante { get; set; } = null!;
    public AgenteItem? Agente { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaLimiteSla { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public string? MotivoResolucion { get; set; }
    public string? MotivoCancelacion { get; set; }
}

public class UsuarioItem
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class TransicionRequest
{
    public string Accion { get; set; } = "";
    public Guid? AgenteId { get; set; }
    public string? Motivo { get; set; }
}

public class AgenteInvalidoException : Exception
{
    public AgenteInvalidoException() : base("El agente especificado no es válido.") { }
}