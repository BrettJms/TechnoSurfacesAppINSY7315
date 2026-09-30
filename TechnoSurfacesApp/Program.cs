using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechnoSurfacesApp.Identity;
using TechnoSurfacesApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Credential store (ASP.NET Core Identity). Fails at startup rather than running
// without a database - the same fail-closed rule the pricing follows.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(connectionString,
        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", AuthDbContext.Schema)));

// ASP.NET Core Identity - application-managed credentials (NFR-03).
// Accounts are created by the Managing Director; there is no self-registration.
builder.Services
    .AddIdentity<UserAccount, IdentityRole>(options =>
    {
        // Length over composition rules, following NIST SP 800-63B (Task 1 §8.2).
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 4;

        // Lockout (NFR-03). Only takes effect when sign-in passes lockoutOnFailure: true.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;

        // No email service exists (client decision), so accounts are not confirmed by email.
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Session cookie hardening. Estimators work from laptops on networks we do not
// control (NFR-12), so the session has a short idle timeout.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "TechnoSurfaces.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

// Re-validate the security stamp every minute, so a deactivated user's open
// session ends within a minute rather than when the cookie expires.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.FromMinutes(1));

// Every state-changing request must carry an antiforgery token (Task 1 §8.8).
// Applied globally so a new form cannot forget it.
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

// US-27: no page is reachable without an authenticated session. Anything not
// explicitly marked [AllowAnonymous] requires sign-in, so a forgotten attribute
// fails closed rather than open.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISignInService, SignInService>();
builder.Services.AddScoped<TechnoSurfaces.Services.DemoSession>();

var app = builder.Build();

// Roles in every environment; test accounts on developer machines only.
await IdentitySeeder.SeedAsync(app.Services, app.Configuration, app.Logger,
    includeDevelopmentAccounts: app.Environment.IsDevelopment());

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

app.UseAuthentication();
app.UseAuthorization();

// CSS, scripts and the logo hold no data and must load on the sign-in page.
app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");


app.Run();