namespace CivicFix.Models;

public sealed class AdminUsersViewModel
{
    public string SearchTerm { get; set; } = string.Empty;
    public List<AdminUserItemViewModel> Users { get; set; } = [];
}

public sealed class AdminUserItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
