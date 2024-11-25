using WarehouseWebApp.Entities;
using Microsoft.AspNetCore.Identity;

namespace WarehouseWebApp.Data;

public class DataSeeder(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
{
    
    public async Task seedRoles()
    {
        string[] roles = { "Admin", "User" };

        // Create roles if they do not exist
        foreach (var role in roles)
        {
            var roleExist = await roleManager.RoleExistsAsync(role);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
    
    
    public async Task SeedAdminUser()
    {
        var adminUser = new User
        {
            UserName = "admin@mail.ru",
            Email = "admin@mail.ru",
            fullName = "Admin",
        };

        // Check if the user already exists
        var existingUser = await userManager.FindByNameAsync(adminUser.UserName);
        if (existingUser == null)
        {
            var result = await userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                await  userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}