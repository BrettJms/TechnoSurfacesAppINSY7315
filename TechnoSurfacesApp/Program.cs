using Microsoft.EntityFrameworkCore;
using TechnoSurfacesApp.Identity;

var builder = WebApplication.CreateBuilder(args);

// Credential store (ASP.NET Core Identity). Fails at startup rather than running
// without a database - the same fail-closed rule the pricing follows.
var connectionString = builder.Configuration.GetConnectionString("TechnoSurfaces")
    ?? throw new InvalidOperationException("Connection string 'TechnoSurfaces' is not configured.");

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(connectionString,
        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", AuthDbContext.Schema)));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(8);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});
builder.Services.AddScoped<TechnoSurfaces.Services.DemoSession>();

var app = builder.Build();
// Load the in-memory demo data (no database in the prototype).
TechnoSurfacesApp.Data.Db.Initialise();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");


app.Run();
