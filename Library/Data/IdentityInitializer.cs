using Microsoft.AspNetCore.Identity;

namespace Library.Data;

public class IdentityInitializer
{
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Tworzenie roli SuperAdmin, jeśli nie istnieje
        if (!await roleManager.RoleExistsAsync("SuperAdmin"))
        {
            await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
        }

        // Tworzenie superużytkownika
        var superUser = await userManager.FindByNameAsync("superadmin");
        if (superUser == null)
        {
            var user = new IdentityUser
            {
                UserName = "superadmin",
                Email = "superadmin@example.com",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, "SuperSecurePassword123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "SuperAdmin");
            }
        }
    }
}