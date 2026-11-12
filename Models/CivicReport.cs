namespace CivicFix.Models;

public sealed class CivicReport
{
    public int Id { get; set; }
    public string Folio { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = "Media";
    public string Status { get; set; } = "Pendiente";
    public string Description { get; set; } = string.Empty;
    public string CitizenName { get; set; } = string.Empty;
    public string? CitizenUserId { get; set; }
    public string? Crew { get; set; }
    public string? PhotoPath { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}