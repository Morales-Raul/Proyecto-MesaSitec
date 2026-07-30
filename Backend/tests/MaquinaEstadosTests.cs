using Api.Aplicacion;
using Api.Modelos;
using Xunit;

namespace Tests;

public class MaquinaEstadosTests
{
    [Fact]
    public void Nueva_Asignar_EsValida()
    {
        var maquina = new MaquinaEstados();
        var sol = new Solicitud { Estado = EstadoSolicitud.Nueva };
        var resultado = maquina.Ejecutar(sol, "asignar", "Admin", Guid.NewGuid(), Guid.NewGuid(), null);
        Assert.Equal(EstadoSolicitud.Asignada, resultado.Estado);
    }

    [Fact]
    public void Nueva_Resolver_LanzaExcepcion()
    {
        var maquina = new MaquinaEstados();
        var sol = new Solicitud { Estado = EstadoSolicitud.Nueva };
        Assert.Throws<TransicionInvalidaException>(() =>
            maquina.Ejecutar(sol, "resolver", "Admin", Guid.NewGuid(), null, "Motivo suficiente con más de veinte caracteres"));
    }

    [Fact]
    public void Agente_NoPuedeCancelar()
    {
        var maquina = new MaquinaEstados();
        var sol = new Solicitud { Estado = EstadoSolicitud.Asignada };
        Assert.Throws<OperacionNoPermitidaException>(() =>
            maquina.Ejecutar(sol, "cancelar", "Agente", Guid.NewGuid(), null, "Motivo cualquiera"));
    }

    [Fact]
    public void Solicitante_PuedeCerrarSuPropiaSolicitud()
    {
        var maquina = new MaquinaEstados();
        var usuarioId = Guid.NewGuid();
        var sol = new Solicitud { Estado = EstadoSolicitud.Resuelta, SolicitanteId = usuarioId };
        var resultado = maquina.Ejecutar(sol, "cerrar", "Solicitante", usuarioId, null, null);
        Assert.Equal(EstadoSolicitud.Cerrada, resultado.Estado);
    }

    [Fact]
    public void Solicitante_NoPuedeCerrarSolicitudAjena()
    {
        var maquina = new MaquinaEstados();
        var sol = new Solicitud { Estado = EstadoSolicitud.Resuelta, SolicitanteId = Guid.NewGuid() };
        Assert.Throws<OperacionNoPermitidaException>(() =>
            maquina.Ejecutar(sol, "cerrar", "Solicitante", Guid.NewGuid(), null, null));
    }

    [Fact]
    public void Resolver_RequiereMotivoLargo()
    {
        var maquina = new MaquinaEstados();
        var sol = new Solicitud { Estado = EstadoSolicitud.EnProceso };
        Assert.Throws<MotivoRequeridoException>(() =>
            maquina.Ejecutar(sol, "resolver", "Admin", Guid.NewGuid(), null, "Corto"));
    }

    [Fact]
    public void Cancelar_RequiereMotivoSuficiente()
    {
        var maquina = new MaquinaEstados();
        var sol = new Solicitud { Estado = EstadoSolicitud.Nueva };
        Assert.Throws<MotivoRequeridoException>(() =>
            maquina.Ejecutar(sol, "cancelar", "Admin", Guid.NewGuid(), null, "Corto"));
    }

    [Fact]
    public void Cerrada_NoAdmiteReabrir()
    {
        var maquina = new MaquinaEstados();
        var sol = new Solicitud { Estado = EstadoSolicitud.Cerrada };
        Assert.Throws<TransicionInvalidaException>(() =>
            maquina.Ejecutar(sol, "reabrir", "Admin", Guid.NewGuid(), null, null));
    }
}