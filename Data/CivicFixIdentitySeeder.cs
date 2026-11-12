using CivicFix.Models;
using Microsoft.AspNetCore.Identity;

namespace CivicFix.Data;

public static class CivicFixIdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var roleName in new[] { CivicFixRoles.Admin, CivicFixRoles.Citizen })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                EnsureSucceeded(roleResult, $"No se pudo crear el rol {roleName}.");
            }
        }

        var email = configuration["CivicFixAdmin:Email"];
        var password = configuration["CivicFixAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Configura CivicFixAdmin:Email y CivicFixAdmin:Password juntos para crear al administrador inicial.");
        }

        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = "Administrador"
            };
            var userResult = await userManager.CreateAsync(admin, password);
            EnsureSucceeded(userResult, "No se pudo crear el administrador inicial; revisa la contraseña configurada.");
        }

        if (!await userManager.IsInRoleAsync(admin, CivicFixRoles.Admin))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, CivicFixRoles.Admin);
            EnsureSucceeded(roleResult, "No se pudo asignar el rol de administrador.");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"{message} {errors}");
        }
    }
}
