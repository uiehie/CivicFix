using System.ComponentModel.DataAnnotations;

namespace CivicFix.Models;

public sealed class CreateReportViewModel
{
    [Required(ErrorMessage = "Elige una categoría.")]
    [Display(Name = "Categoría")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Describe dónde ocurre el problema.")]
    [StringLength(240)]
    [Display(Name = "Ubicación")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cuéntanos qué sucede.")]
    [StringLength(1500, MinimumLength = 12, ErrorMessage = "La descripción debe tener entre 12 y 1500 caracteres.")]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Prioridad")]
    public string Priority { get; set; } = "Media";

    [Display(Name = "Fotografía")]
    public IFormFile? Photo { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}