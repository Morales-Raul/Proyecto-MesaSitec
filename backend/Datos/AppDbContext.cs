using Microsoft.EntityFrameworkCore;
using Api.Modelos;

namespace Api.Datos;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique email
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();

        // Relaciones
        modelBuilder.Entity<Usuario>().HasOne(u => u.Tenant).WithMany(t => t.Usuarios).HasForeignKey(u => u.TenantId);
        modelBuilder.Entity<Categoria>().HasOne(c => c.Tenant).WithMany(t => t.Categorias).HasForeignKey(c => c.TenantId);
        modelBuilder.Entity<Solicitud>().HasOne(s => s.Tenant).WithMany(t => t.Solicitudes).HasForeignKey(s => s.TenantId);
        modelBuilder.Entity<Solicitud>().HasOne(s => s.Categoria).WithMany().HasForeignKey(s => s.CategoriaId);
        modelBuilder.Entity<Solicitud>().HasOne(s => s.Solicitante).WithMany().HasForeignKey(s => s.SolicitanteId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Solicitud>().HasOne(s => s.Agente).WithMany().HasForeignKey(s => s.AgenteId).OnDelete(DeleteBehavior.NoAction);
    }
}