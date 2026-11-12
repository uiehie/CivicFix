using System.ComponentModel.DataAnnotations;

namespace CivicFix.Models;

public sealed class LoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Mantener sesión iniciada")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
