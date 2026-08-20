using AstroConnect.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace AstroConnect.Persistence.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
        }

        if (!await roleManager.RoleExistsAsync(Roles.Receptionist))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Receptionist));
        }

        if (!await roleManager.RoleExistsAsync(Roles.Customer))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Customer));
        }
    }
    public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var adminUser = await userManager.FindByEmailAsync("admin@astroconnect.com");
        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin@astroconnect.com",
                Email = "admin@astroconnect.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, "Admin@123");
            await userManager.AddToRoleAsync(user, Roles.Admin);
        }
    }
}