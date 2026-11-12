using CivicFix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicFix.Controllers;

[Authorize(Roles = CivicFixRoles.Admin)]
public sealed class AdminUsersController(UserManager<AppUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda)
    {
        var admins = await userManager.GetUsersInRoleAsync(CivicFixRoles.Admin);
        var adminIds = admins.Select(user => user.Id).ToHashSet(StringComparer.Ordinal);
        var users = userManager.Users.AsNoTracking();
        var searchTerm = busqueda?.Trim() ?? string.Empty;

        if (searchTerm.Length > 0)
        {
            users = users.Where(user =>
                (user.Email != null && user.Email.Contains(searchTerm))
                || user.FullName.Contains(searchTerm));
        }

        var userRows = await users
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Email)
            .Select(user => new AdminUserItemViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName
            })
            .ToListAsync();

        var model = new AdminUsersViewModel
        {
            SearchTerm = searchTerm,
            Users = userRows
        };
        foreach (var user in model.Users)
        {
            user.IsAdmin = adminIds.Contains(user.Id);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Promote(string userId, string? busqueda)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        if (await userManager.IsInRoleAsync(user, CivicFixRoles.Admin))
        {
            TempData["AdminUsersSuccess"] = $"{user.Email} ya tiene el rol Admin.";
            return RedirectToAction(nameof(Index), new { busqueda });
        }

        var result = await userManager.AddToRoleAsync(user, CivicFixRoles.Admin);
        if (!result.Succeeded)
        {
            TempData["AdminUsersError"] = string.Join(" ", result.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Index), new { busqueda });
        }

        TempData["AdminUsersSuccess"] = $"Se asignó el rol Admin a {user.Email}.";
        return RedirectToAction(nameof(Index), new { busqueda });
    }
}
