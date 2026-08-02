using Api.Modelos;

namespace Api.Aplicacion;

public class MaquinaEstados
{
    /// <summary>
    /// Ejecuta una transición sobre una solicitud según la acción solicitada.
    /// Retorna la solicitud modificada o lanza una excepción con el código de error correspondiente.
    /// </summary>
    /// <param name="solicitud">La solicitud a modificar (debe estar trackeada por EF Core).</param>
    /// <param name="accion">La acción a ejecutar: asignar, iniciar, resolver, cerrar, reabrir, cancelar.</param>
    /// <param name="rol">El rol del usuario que intenta la acción.</param>
    /// <param name="usuarioId">El Id del usuario que intenta la acción.</param>
    /// <param name="agenteId">El Id del agente a asignar (solo para acción 'asignar').</param>
    /// <param name="motivo">El motivo de resolución o cancelación (solo para 'resolver' o 'cancelar').</param>
    /// <returns>La solicitud actualizada.</returns>
    public Solicitud Ejecutar(Solicitud solicitud, string accion, string rol, Guid usuarioId, Guid? agenteId, string? motivo)
    {
        // Validación de la máquina de estados (RN-02)
        if (!EsTransicionValida(solicitud.Estado, accion))
            throw new TransicionInvalidaException(accion, solicitud.Estado.ToString());

        // Validación de permisos por rol (RN-03)
        if (!TienePermiso(solicitud, accion, rol, usuarioId))
            throw new OperacionNoPermitidaException(rol);

        // Ejecutar la acción específica
        switch (accion)
        {
            case "asignar":
                if (agenteId == null)
                    throw new ArgumentException("Se requiere un agenteId para la acción 'asignar'.");
                // RN-05: La validación de que el agente existe, está activo, pertenece al tenant y tiene rol válido
                // se hace en el servicio o en una validación externa, pero aquí lanzaremos la excepción esperada.
                // Por ahora, simplemente asignamos el agenteId.
                solicitud.AgenteId = agenteId.Value;
                solicitud.Estado = EstadoSolicitud.Asignada;
                break;

            case "iniciar":
                solicitud.Estado = EstadoSolicitud.EnProceso;
                break;

            case "resolver":
                if (string.IsNullOrWhiteSpace(motivo) || motivo.Length < 20)
                    throw new MotivoRequeridoException("motivo", "El motivo de resolución debe tener al menos 20 caracteres.");
                solicitud.MotivoResolucion = motivo;
                solicitud.FechaResolucion = DateTime.UtcNow;
                solicitud.Estado = EstadoSolicitud.Resuelta;
                break;

            case "cerrar":
                // El cierre lo puede hacer cualquiera con permiso, no requiere motivo especial
                solicitud.Estado = EstadoSolicitud.Cerrada;
                break;

            case "reabrir":
                solicitud.Estado = EstadoSolicitud.EnProceso;
                // Limpiar datos de resolución anterior si se reabre
                solicitud.FechaResolucion = null;
                solicitud.MotivoResolucion = null;
                break;

            case "cancelar":
                if (string.IsNullOrWhiteSpace(motivo) || motivo.Length < 10)
                    throw new MotivoRequeridoException("motivo", "El motivo de cancelación debe tener al menos 10 caracteres.");
                solicitud.MotivoCancelacion = motivo;
                solicitud.Estado = EstadoSolicitud.Cancelada;
                break;

            default:
                throw new ArgumentException($"Acción '{accion}' no reconocida.");
        }

        return solicitud;
    }

    private bool EsTransicionValida(EstadoSolicitud estadoActual, string accion)
    {
        return (estadoActual, accion) switch
        {
            (EstadoSolicitud.Nueva, "asignar") => true,
            (EstadoSolicitud.Nueva, "cancelar") => true,
            (EstadoSolicitud.Asignada, "iniciar") => true,
            (EstadoSolicitud.Asignada, "asignar") => true,
            (EstadoSolicitud.Asignada, "cancelar") => true,
            (EstadoSolicitud.EnProceso, "resolver") => true,
            (EstadoSolicitud.EnProceso, "asignar") => true,
            (EstadoSolicitud.EnProceso, "cancelar") => true,
            (EstadoSolicitud.Resuelta, "cerrar") => true,
            (EstadoSolicitud.Resuelta, "reabrir") => true,
            // Cerrada y Cancelada no admiten acciones
            _ => false
        };
    }

    private bool TienePermiso(Solicitud solicitud, string accion, string rol, Guid usuarioId)
    {
        // Admin y Agente pueden hacer todo excepto cancelar (Agente no puede cancelar)
        if (rol == "Admin") return true;
        if (rol == "Agente")
        {
            // Los agentes no pueden cancelar solicitudes según RN-03
            if (accion == "cancelar") return false;
            return true;
        }
        // Solicitante
        if (rol == "Solicitante")
        {
            // Solo puede cerrar sus propias solicitudes
            if (accion == "cerrar" && solicitud.SolicitanteId == usuarioId) return true;
            // No puede hacer ninguna otra acción de transición
            return false;
        }
        return false;
    }
}

// Excepciones personalizadas para los distintos errores de negocio
public class TransicionInvalidaException : Exception
{
    public string Accion { get; }
    public string Estado { get; }
    public TransicionInvalidaException(string accion, string estado)
        : base($"No se puede aplicar '{accion}' sobre una solicitud en estado '{estado}'.")
    {
        Accion = accion;
        Estado = estado;
    }
}

public class OperacionNoPermitidaException : Exception
{
    public OperacionNoPermitidaException(string rol)
        : base($"El rol '{rol}' no tiene permiso para realizar esta operación.") { }
}

public class MotivoRequeridoException : Exception
{
    public string Campo { get; }
    public MotivoRequeridoException(string campo, string mensaje) : base(mensaje)
    {
        Campo = campo;
    }
}