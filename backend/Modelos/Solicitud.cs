namespace Api.Modelos;

public class Solicitud
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Codigo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public Guid CategoriaId { get; set; }
    public Prioridad Prioridad { get; set; }
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Nueva;
    public Guid SolicitanteId { get; set; }
    public Guid? AgenteId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaLimiteSla { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public string? MotivoResolucion { get; set; }
    public string? MotivoCancelacion { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Categoria Categoria { get; set; } = null!;
    public Usuario Solicitante { get; set; } = null!;
    public Usuario? Agente { get; set; }
}

public enum Prioridad
{
    Baja,
    Media,
    Alta,
    Critica
}

public enum EstadoSolicitud
{
    Nueva,
    Asignada,
    EnProceso,
    Resuelta,
    Cerrada,
    Cancelada
}