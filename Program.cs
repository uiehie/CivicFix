using CivicFix.Data;
using CivicFix.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CivicFixContext>(options =>
{
    var provider = builder.Configuration["Database:Provider"] ?? "SqlServer";
    if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
    {
        options.UseInMemoryDatabase("CivicFixDemo");
    }
    else if (provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
    {
        var dbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "civicfix.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        options.UseSqlite($"Data Source={dbPath}");
    }
    else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        var connectionString = builder.Configuration.GetConnectionString("CivicFix");
        options.UseSqlServer(connectionString);
    }
    else
    {
        throw new InvalidOperationException($"Proveedor de base de datos no compatible: {provider}");
    }
});
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    })
    .AddEntityFrameworkStores<CivicFixContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}")
    .WithStaticAssets();

await using (var scope = app.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<CivicFixContext>();
    await database.Database.EnsureCreatedAsync();
    await CivicFixIdentitySeeder.SeedAsync(scope.ServiceProvider, builder.Configuration);
    if (app.Environment.IsDevelopment())
    {
        await CivicFixSeeder.SeedAsync(database);
    }
}

app.Run();
