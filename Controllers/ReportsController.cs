using System.Security.Cryptography;
using CivicFix.Data;
using CivicFix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicFix.Controllers;

[Authorize]
public sealed class ReportsController(
    CivicFixContext database,
    IWebHostEnvironment environment,
    UserManager<AppUser> userManager) : Controller
{
    private static readonly string[] Categories = ["Alumbrado", "Agua", "Limpieza", "Parques", "Vialidad", "Otro"];
    private static readonly IReadOnlyDictionary<string, string> ImageTypes = new Dictionary<string, string>
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateOptionsAsync();
        return View(new CreateReportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReportViewModel model)
    {
        var citizen = await userManager.GetUserAsync(User);
        if (citizen is null)
        {
            return Challenge();
        }

        if (!Categories.Contains(model.Category))
        {
            ModelState.AddModelError(nameof(model.Category), "Elige una categoría válida.");
        }

        if (model.Priority is not ("Baja" or "Media" or "Alta"))
        {
            ModelState.AddModelError(nameof(model.Priority), "Elige una prioridad válida.");
        }

        string? photoExtension = null;
        if (model.Photo is { Length: > 0 } photo)
        {
            if (photo.Length > 5 * 1024 * 1024 || !ImageTypes.TryGetValue(photo.ContentType, out photoExtension))
            {
                ModelState.AddModelError(nameof(model.Photo), "Adjunta una imagen JPG, PNG o WEBP de hasta 5 MB.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync();
            return View(model);
        }

        string? photoPath = null;
        if (model.Photo is { Length: > 0 } uploadedPhoto && photoExtension is not null)
        {
            var uploadDirectory = Path.Combine(environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadDirectory);
            var fileName = $"{Guid.NewGuid():N}{photoExtension}";
            var filePath = Path.Combine(uploadDirectory, fileName);
            await using var stream = System.IO.File.Create(filePath);
            await uploadedPhoto.CopyToAsync(stream);
            photoPath = $"/uploads/{fileName}";
        }

        var report = new CivicReport
        {
            Folio = $"CF-{DateTime.UtcNow:yyyy}-{RandomNumberGenerator.GetInt32(10000, 100000)}",
            Category = model.Category,
            Location = model.Location.Trim(),
            Priority = model.Priority,
            Description = model.Description.Trim(),
            CitizenName = citizen.FullName,
            CitizenUserId = citizen.Id,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            PhotoPath = photoPath,
            Status = "Pendiente",
            CreatedAt = DateTime.UtcNow
        };

        database.Reports.Add(report);
        await database.SaveChangesAsync();
        return RedirectToAction(nameof(Confirmation), new { folio = report.Folio });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(string folio)
    {
        if (string.IsNullOrWhiteSpace(folio))
        {
            return RedirectToAction(nameof(Create));
        }

        var report = await database.Reports.AsNoTracking()
            .Where(item => item.Folio == folio)
            .Select(item => new { item.CitizenUserId })
            .FirstOrDefaultAsync();
        if (report is null)
        {
            return NotFound();
        }

        if (!User.IsInRole(CivicFixRoles.Admin))
        {
            var citizen = await userManager.GetUserAsync(User);
            if (citizen is null)
            {
                return Challenge();
            }

            if (report.CitizenUserId != citizen.Id)
            {
                return Forbid();
            }
        }

        ViewData["ReportCount"] = await database.Reports.CountAsync();
        return View(model: folio);
    }

    private async Task PopulateOptionsAsync()
    {
        ViewBag.Categories = Categories;
        ViewBag.Priorities = new[] { "Baja", "Media", "Alta" };
        ViewData["ReportCount"] = await database.Reports.CountAsync();
    }
}