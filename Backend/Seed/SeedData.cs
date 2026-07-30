using Api.Datos;
using Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Api.Seed;

public static class SeedData
{
    public static async Task Inicializar(AppDbContext db)
    {
        Console.WriteLine("=== SEED: Iniciando... ===");
        if (await db.Tenants.AnyAsync())
        {
            Console.WriteLine("=== SEED: Datos ya existen, se omite. ===");
            return;
        }

        Console.WriteLine("=== SEED: Insertando datos... ===");

        var fechaBaseStr = Environment.GetEnvironmentVariable("SEED_FECHA_BASE") ?? "2026-01-15T08:00:00Z";
        var fechaBase = DateTime.Parse(fechaBaseStr).ToUniversalTime();
        Console.WriteLine($"Fecha base: {fechaBase}");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Sitec.2026");

        // --- Tenants ---
        var norteId = Guid.NewGuid();
        var surId = Guid.NewGuid();
        db.Tenants.AddRange(
            new Tenant { Id = norteId, Nombre = "Cooperativa Norte" },
            new Tenant { Id = surId, Nombre = "Bufete Sur" }
        );

        // --- Usuarios ---
        var usuarios = new List<Usuario>
        {
            new() { Id = Guid.NewGuid(), TenantId = norteId, Email = "admin@norte.test", Nombre = "Admin Norte", Rol = Rol.Admin, PasswordHash = passwordHash },
            new() { Id = Guid.NewGuid(), TenantId = norteId, Email = "agente1@norte.test", Nombre = "Agente 1 Norte", Rol = Rol.Agente, PasswordHash = passwordHash },
            new() { Id = Guid.NewGuid(), TenantId = norteId, Email = "agente2@norte.test", Nombre = "Agente 2 Norte", Rol = Rol.Agente, PasswordHash = passwordHash },
            new() { Id = Guid.NewGuid(), TenantId = norteId, Email = "user1@norte.test", Nombre = "Usuario 1 Norte", Rol = Rol.Solicitante, PasswordHash = passwordHash },
            new() { Id = Guid.NewGuid(), TenantId = norteId, Email = "user2@norte.test", Nombre = "Usuario 2 Norte", Rol = Rol.Solicitante, PasswordHash = passwordHash },
            new() { Id = Guid.NewGuid(), TenantId = surId, Email = "admin@sur.test", Nombre = "Admin Sur", Rol = Rol.Admin, PasswordHash = passwordHash },
            new() { Id = Guid.NewGuid(), TenantId = surId, Email = "user1@sur.test", Nombre = "Usuario 1 Sur", Rol = Rol.Solicitante, PasswordHash = passwordHash },
        };
        db.Usuarios.AddRange(usuarios);

        // --- Categorías (ambas organizaciones) ---
        var categoriasNorte = new List<Categoria>
        {
            new() { Id = Guid.NewGuid(), TenantId = norteId, Nombre = "Incidente", SlaHoras = 8 },
            new() { Id = Guid.NewGuid(), TenantId = norteId, Nombre = "Requerimiento", SlaHoras = 40 },
            new() { Id = Guid.NewGuid(), TenantId = norteId, Nombre = "Consulta", SlaHoras = 24 },
            new() { Id = Guid.NewGuid(), TenantId = norteId, Nombre = "Falla crítica", SlaHoras = 4 },
        };
        var categoriasSur = new List<Categoria>
        {
            new() { Id = Guid.NewGuid(), TenantId = surId, Nombre = "Incidente", SlaHoras = 8 },
            new() { Id = Guid.NewGuid(), TenantId = surId, Nombre = "Requerimiento", SlaHoras = 40 },
            new() { Id = Guid.NewGuid(), TenantId = surId, Nombre = "Consulta", SlaHoras = 24 },
            new() { Id = Guid.NewGuid(), TenantId = surId, Nombre = "Falla crítica", SlaHoras = 4 },
        };
        db.Categorias.AddRange(categoriasNorte);
        db.Categorias.AddRange(categoriasSur);

        // --- Solicitudes ---
        Usuario U(string email) => usuarios.First(u => u.Email == email);

        var solicitudesNorte = new List<Solicitud>();
        var solicitudesSur = new List<Solicitud>();

        var estados = new[] { EstadoSolicitud.Nueva, EstadoSolicitud.Asignada, EstadoSolicitud.EnProceso, EstadoSolicitud.Resuelta, EstadoSolicitud.Cerrada, EstadoSolicitud.Cancelada };
        var prioridades = new[] { Prioridad.Baja, Prioridad.Media, Prioridad.Alta, Prioridad.Critica };
        var rnd = new Random(42);

        // Norte: 25 solicitudes
        for (int i = 0; i < 25; i++)
        {
            var estado = estados[i % estados.Length];
            var prioridad = prioridades[i % prioridades.Length];
            var fechaCreacion = fechaBase.AddHours(-i * 10);
            var cat = categoriasNorte[rnd.Next(categoriasNorte.Count)];
            var slaHoras = cat.SlaHoras;
            var factor = prioridad switch
            {
                Prioridad.Critica => 0.5,
                Prioridad.Alta => 0.75,
                Prioridad.Media => 1.0,
                Prioridad.Baja => 2.0,
                _ => 1.0
            };
            var fechaLimite = fechaCreacion.AddHours(slaHoras * factor);

            var sol = new Solicitud
            {
                Id = Guid.NewGuid(),
                TenantId = norteId,
                Codigo = $"SOL-{fechaCreacion.Year}-{(i + 1):D5}",
                Titulo = $"Problema de {cat.Nombre} #{i + 1}",
                Descripcion = $"Descripción detallada del problema número {i + 1} en la Cooperativa Norte. Esto es un texto de prueba que supera los 10 caracteres.",
                CategoriaId = cat.Id,
                Prioridad = prioridad,
                Estado = estado,
                SolicitanteId = U("user1@norte.test").Id,
                AgenteId = (estado == EstadoSolicitud.Nueva || estado == EstadoSolicitud.Cancelada) ? null : U("agente1@norte.test").Id,
                FechaCreacion = fechaCreacion,
                FechaLimiteSla = fechaLimite,
                FechaResolucion = (estado == EstadoSolicitud.Resuelta || estado == EstadoSolicitud.Cerrada) ? fechaCreacion.AddHours(slaHoras * factor * 0.8) : null,
                MotivoResolucion = (estado == EstadoSolicitud.Resuelta || estado == EstadoSolicitud.Cerrada) ? "Se resolvió correctamente tras análisis." : null,
                MotivoCancelacion = estado == EstadoSolicitud.Cancelada ? "Cancelado por el usuario." : null
            };
            solicitudesNorte.Add(sol);
        }

        // Al menos 5 vencidas
        for (int i = 0; i < 5; i++)
        {
            solicitudesNorte[i].FechaLimiteSla = fechaBase.AddHours(-2);
        }

        // Sur: 8 solicitudes
        for (int i = 0; i < 8; i++)
        {
            var estado = i < 3 ? EstadoSolicitud.Nueva : EstadoSolicitud.Asignada;
            var prioridad = prioridades[i % prioridades.Length];
            var fechaCreacion = fechaBase.AddHours(-i * 5);
            var cat = categoriasSur[rnd.Next(categoriasSur.Count)];
            var factor = prioridad switch
            {
                Prioridad.Critica => 0.5,
                Prioridad.Alta => 0.75,
                Prioridad.Media => 1.0,
                Prioridad.Baja => 2.0,
                _ => 1.0
            };
            var fechaLimite = fechaCreacion.AddHours(cat.SlaHoras * factor);
            solicitudesSur.Add(new Solicitud
            {
                Id = Guid.NewGuid(),
                TenantId = surId,
                Codigo = $"SOL-{fechaCreacion.Year}-{(i + 1):D5}",
                Titulo = $"Incidencia Sur #{i + 1}",
                Descripcion = $"Descripción del caso {i + 1} en Bufete Sur. Más de diez caracteres.",
                CategoriaId = cat.Id,
                Prioridad = prioridad,
                Estado = estado,
                SolicitanteId = U("user1@sur.test").Id,
                AgenteId = null,
                FechaCreacion = fechaCreacion,
                FechaLimiteSla = fechaLimite
            });
        }

        db.Solicitudes.AddRange(solicitudesNorte);
        db.Solicitudes.AddRange(solicitudesSur);

        await db.SaveChangesAsync();
        Console.WriteLine("=== SEED: Datos insertados correctamente. ===");
    }
}   