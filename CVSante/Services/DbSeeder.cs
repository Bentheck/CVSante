using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public class DbSeeder
{
    public static async Task SeedAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Check if the "Superadmin" role exists
        if (!await roleManager.RoleExistsAsync("SuperAdmin"))
        {
            // Create the "Superadmin" role
            await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));

        }

        if (!await roleManager.RoleExistsAsync("Paramedic"))
        {
            // Create the "Superadmin" role
            await roleManager.CreateAsync(new IdentityRole("Paramedic"));

        }

        // Check if the "superadmin" user exists
        var superAdminUser = await userManager.FindByNameAsync("superadmin");
        var testParamedic = await userManager.FindByNameAsync("testparamedic");
        var testCitoyen = await userManager.FindByNameAsync("testcitoyen");
        if (superAdminUser == null && testParamedic == null && testCitoyen == null)
        {
            superAdminUser = new IdentityUser { UserName = "superadmin", NormalizedUserName="SUPERADMIN", Email = "superadmin@test.com", EmailConfirmed=true };
            await userManager.CreateAsync(superAdminUser, "@Test123");
            testParamedic = new IdentityUser { UserName = "testparamedic", NormalizedUserName="TESTPARAMEDIC", Email = "TestParamedic@test.com", EmailConfirmed = true };
            await userManager.CreateAsync(testParamedic, "@Test123");
            testCitoyen = new IdentityUser { UserName = "testcitoyen", NormalizedUserName="TESTCITOYEN", Email = "testcitoyen@test.com", EmailConfirmed = true };
            await userManager.CreateAsync(testCitoyen, "@Test123");

            await userManager.AddToRoleAsync(superAdminUser, "Superadmin");
            await userManager.AddToRoleAsync(testParamedic, "Paramedic");
        }
    }
}