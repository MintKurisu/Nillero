using Nillero.Core.Domain.Common.Enum;
using Nillero.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Nillero.Infrastructure.Identity.Seeds
{
    public static class DefaultUser
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser basicUser = new()
            {
                FirstName = "Cris",
                LastName = "Kuw",
                Email = "cristest@gmail.com",
                EmailConfirmed = true,
                Phone = "08080808",
                PhoneNumberConfirmed = true,
                UserName = "BasicCrisUser"
            };

            if (await userManager.FindByEmailAsync(basicUser.Email) == null)
            {
                await userManager.CreateAsync(basicUser, "Kuriowo123!");
                await userManager.AddToRoleAsync(basicUser, Roles.User.ToString());
            }

            // ----------------------------------------------------------------------

            ApplicationUser basicUser2 = new()
            {
                FirstName = "Wrong",
                LastName = "Neighboorhood",
                Email = "helloneighboor@gmail.com",
                EmailConfirmed = true,
                Phone = "08080808",
                PhoneNumberConfirmed = true,
                UserName = "SinisK"
            };

            if (await userManager.FindByEmailAsync(basicUser2.Email) == null)
            {
                await userManager.CreateAsync(basicUser2, "Kuriowo123!");
                await userManager.AddToRoleAsync(basicUser2, Roles.User.ToString());
            }

        }
    }
}
