using Blog;
using Blog.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Leer configuración de la base de datos
var databaseConfig = builder.Configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>();
builder.Services.AddSingleton(databaseConfig!);

if (databaseConfig!.UseInMemoryDatabase)
{
    // Usar repositorio en memoria
    builder.Services.AddSingleton<IArticleRepository, MemoryArticleRepository>();
}
else
{
    // Configurar Entity Framework Core con SQLite
    builder.Services.AddDbContext<BlogDbContext>(options =>
        options.UseSqlite(databaseConfig.DefaultConnectionString));

    // Registrar el repositorio con EF Core usando AddScoped
    builder.Services.AddScoped<IArticleRepository, EfCoreArticleRepository>();
}

var app = builder.Build();

// Asegurar que la base de datos esté creada y aplicar migraciones
if (!databaseConfig.UseInMemoryDatabase)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        context.Database.EnsureCreated(); // O usa context.Database.Migrate() si tienes migraciones
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Articles}/{action=Index}/{id?}");

app.Run();