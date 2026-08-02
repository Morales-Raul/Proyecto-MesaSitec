using Api.Modelos;

namespace Api.Aplicacion;

public static class SlaCalculator
{
    public static DateTime CalcularFechaLimite(DateTime fechaCreacion, int slaHoras, Prioridad prioridad)
    {
        double factor = prioridad switch
        {
            Prioridad.Critica => 0.5,
            Prioridad.Alta => 0.75,
            Prioridad.Media => 1.0,
            Prioridad.Baja => 2.0,
            _ => 1.0
        };
        return fechaCreacion.AddHours(slaHoras * factor);
    }
}