using Microsoft.AspNetCore.Identity;

namespace CivicFix.Models;

public sealed class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
