using Demologin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Demologin.Extensions
{
    public static class IdentityExtensions
    {
        public static IdentityBuilder AddDefaultIdentityWithRole(this IServiceCollection services)
        {
            return services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }

        public static async Task EnsureUserRoleAsync(this UserManager<ApplicationUser> userManager, ApplicationUser user, string role)
        {
            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
