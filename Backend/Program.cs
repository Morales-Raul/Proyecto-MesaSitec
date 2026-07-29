using Microsoft.EntityFrameworkCore;
using Api.Datos;
using Api.Seed;

var builder = WebApplication.CreateBuilder(args);

// SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=mesasitec.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Migraciones automáticas (usamos EnsureCreated para empezar, luego migraciones reales)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.Inicializar(db);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();

app.Urls.Add("http://0.0.0.0:5080");
app.Run();