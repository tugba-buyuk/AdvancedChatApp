using Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace SignalRApp.Extensions
{
    public static class ApplicationExtension
    {
        public static async void ConfigureDefaultAdmin(this IApplicationBuilder app)
        {
            const string adminUserName = "Admin";
            const string adminPassword = "9517536Tb*";
            UserManager<User> userManager = app
                .ApplicationServices
                .CreateScope()
                .ServiceProvider
                .GetRequiredService<UserManager<User>>();

            RoleManager<IdentityRole> roleManager= app
                .ApplicationServices
                .CreateScope()
                .ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            User user= await userManager.FindByNameAsync(adminUserName);
            if(user is null)
            {
                user = new User
                {
                    Email = "tugba.buyuk@std.yildiz.edu.tr",
                    PhoneNumber = "05344065963",
                    UserName = adminUserName,
                };
                var result = await userManager.CreateAsync(user, adminPassword);
                if (!result.Succeeded)
                {
                    throw new Exception("Admin could not created.");
                }
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception("Admin role could not be created.");
                    }
                }
                if (!await userManager.IsInRoleAsync(user, "Admin"))
                {
                    var roleResult = await userManager.AddToRoleAsync(user, "Admin");
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception("Failed to add user to Admin role.");
                    }
                }
            }

        }
    }
}
