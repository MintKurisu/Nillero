using Nillero.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Nillero.Infrastructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.Roles.AnyAsync(r => r.Name == Roles.User.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.User.ToString()));
            }
        }
    }
}
