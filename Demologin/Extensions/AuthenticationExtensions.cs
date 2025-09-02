using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Demologin.Extensions
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddGoogleWithRole(this AuthenticationBuilder builder, IServiceCollection services, IConfiguration config)
        {
            builder.AddGoogle(options =>
            {
                options.ClientId = config["Authentication:Google:ClientId"];
                options.ClientSecret = config["Authentication:Google:ClientSecret"];
                options.CallbackPath = "/signin-google";

                options.Events.OnCreatingTicket = async ctx =>
                {
                    var userManager = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                    var user = await userManager.FindByLoginAsync(
                        ctx.Scheme.Name,
                        ctx.Principal.FindFirstValue(ClaimTypes.NameIdentifier));

                    // ✅ Auto-assign "Farmer" role to Google users
                    if (user != null && !await userManager.IsInRoleAsync(user, "Farmer"))
                    {
                        await userManager.AddToRoleAsync(user, "Farmer");
                    }

                    // ✅ Save Google profile picture
                    if (ctx.User.TryGetProperty("picture", out var pictureElement))
                    {
                        var picture = pictureElement.GetString();
                        if (!string.IsNullOrEmpty(picture))
                        {
                            ctx.Identity!.AddClaim(new Claim("picture", picture));
                        }
                    }
                };
            });

            return builder;
        }
    }
}
