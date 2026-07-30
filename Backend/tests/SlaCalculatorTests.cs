using Api.Aplicacion;
using Api.Modelos;
using Xunit;

namespace Tests;

public class SlaCalculatorTests
{
    [Fact]
    public void Critica_ReduceSlaALaMitad()
    {
        var fechaBase = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var resultado = SlaCalculator.CalcularFechaLimite(fechaBase, 8, Prioridad.Critica);
        Assert.Equal(fechaBase.AddHours(4), resultado);
    }

    [Fact]
    public void Alta_ReduceSlaEn25PorCiento()
    {
        var fechaBase = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var resultado = SlaCalculator.CalcularFechaLimite(fechaBase, 8, Prioridad.Alta);
        Assert.Equal(fechaBase.AddHours(6), resultado);
    }

    [Fact]
    public void Media_MantieneSla()
    {
        var fechaBase = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var resultado = SlaCalculator.CalcularFechaLimite(fechaBase, 24, Prioridad.Media);
        Assert.Equal(fechaBase.AddHours(24), resultado);
    }

    [Fact]
    public void Baja_DuplicaSla()
    {
        var fechaBase = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var resultado = SlaCalculator.CalcularFechaLimite(fechaBase, 10, Prioridad.Baja);
        Assert.Equal(fechaBase.AddHours(20), resultado);
    }
}