using System.Diagnostics;
using System.Text;
using CivicFix.Data;
using CivicFix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicFix.Controllers;

[Authorize(Roles = CivicFixRoles.Admin)]
public sealed class HomeController(CivicFixContext database) : Controller
{
    public async Task<IActionResult> Index(string? estado, string? categoria, string? busqueda)
    {
        var reports = database.Reports.AsNoTracking();
        var model = new ReportDashboardViewModel
        {
            TotalCount = await reports.CountAsync(),
            ResolvedCount = await reports.CountAsync(report => report.Status == "Resuelto"),
            PendingCount = await reports.CountAsync(report => report.Status == "Pendiente"),
            InProgressCount = await reports.CountAsync(report => report.Status == "En Proceso"),
            Categories = await reports.Select(report => report.Category).Distinct().OrderBy(categoryName => categoryName).ToListAsync(),
            SelectedStatus = estado ?? string.Empty,
            SelectedCategory = categoria ?? string.Empty,
            SearchTerm = busqueda ?? string.Empty
        };

        if (!string.IsNullOrWhiteSpace(estado))
        {
            reports = reports.Where(report => report.Status == estado);
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            reports = reports.Where(report => report.Category == categoria);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            reports = reports.Where(report => report.Folio.Contains(busqueda)
                || report.Location.Contains(busqueda)
                || report.Category.Contains(busqueda));
        }

        model.Reports = await reports.OrderByDescending(report => report.CreatedAt).ToListAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int id, string? busqueda, string? estado, string? categoria)
    {
        var report = await database.Reports.FindAsync(id);
        if (report is null)
        {
            return NotFound();
        }

        if (report.Status == "Pendiente")
        {
            report.Status = "En Proceso";
            report.Crew = "Cuadrilla A-7";
            await database.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { busqueda, estado, categoria });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(int id, string? busqueda, string? estado, string? categoria)
    {
        var report = await database.Reports.FindAsync(id);
        if (report is null)
        {
            return NotFound();
        }

        if (report.Status is "Pendiente" or "En Proceso")
        {
            report.Status = "Resuelto";
            report.ResolvedAt = DateTime.UtcNow;
            await database.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { busqueda, estado, categoria });
    }

    public async Task<IActionResult> Export(string? estado, string? categoria, string? busqueda)
    {
        var reports = database.Reports.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(estado))
        {
            reports = reports.Where(report => report.Status == estado);
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            reports = reports.Where(report => report.Category == categoria);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            reports = reports.Where(report => report.Folio.Contains(busqueda)
                || report.Location.Contains(busqueda)
                || report.Category.Contains(busqueda));
        }

        var rows = await reports.OrderByDescending(report => report.CreatedAt).ToListAsync();
        var csv = new StringBuilder("Folio,Categoría,Ubicación,Prioridad,Fecha,Estatus,Cuadrilla\r\n");
        foreach (var report in rows)
        {
            csv.AppendLine(string.Join(',', new[]
            {
                report.Folio,
                report.Category,
                report.Location,
                report.Priority,
                report.CreatedAt.ToString("yyyy-MM-dd"),
                report.Status,
                report.Crew ?? string.Empty
            }.Select(EscapeCsv)));
        }

        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(), "text/csv; charset=utf-8", "civicfix-reportes.csv");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    private static string EscapeCsv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
