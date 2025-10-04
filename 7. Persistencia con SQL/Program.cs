using Blog;
using Blog.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


var databaseConfig = builder.Configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>();
builder.Services.AddSingleton(databaseConfig!);

if (databaseConfig!.UseInMemoryDatabase)
{

    builder.Services.AddSingleton<IArticleRepository, MemoryArticleRepository>();
}
else
{
\
    builder.Services.AddDbContext<BlogDbContext>(options =>
        options.UseSqlite(databaseConfig.DefaultConnectionString));


    builder.Services.AddScoped<IArticleRepository, EfCoreArticleRepository>();
}

var app = builder.Build();


if (!databaseConfig.UseInMemoryDatabase)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        context.Database.EnsureCreated(); 
    }
}


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
