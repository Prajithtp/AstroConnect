using Microsoft.AspNetCore.Identity;


namespace AstroConnect.Persistence.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}