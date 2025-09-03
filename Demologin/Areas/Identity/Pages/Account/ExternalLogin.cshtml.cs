using System;
using System.Linq;
using System.Threading.Tasks;
using Demologin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demologin.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }
        // ✅ This is needed for the Razor Page form
        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public string ProviderDisplayName { get; set; }

        public class InputModel
        {
            public string Email { get; set; }
        }
     

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Try to sign in user with existing external login
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false);

            if (result.Succeeded)
            {
                var existingUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                return await RedirectBasedOnRole(existingUser, returnUrl);
            }

            // If user does not exist, create new one
            var email = info.Principal.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            if (email != null)
            {
                var user = new ApplicationUser { UserName = email, Email = email };
                var identityResult = await _userManager.CreateAsync(user);

                if (identityResult.Succeeded)
                {
                    identityResult = await _userManager.AddLoginAsync(user, info);
                    if (identityResult.Succeeded)
                    {
                        // ✅ Assign "Farmer" role automatically
                        if (!await _userManager.IsInRoleAsync(user, "Farmer"))
                        {
                            await _userManager.AddToRoleAsync(user, "Farmer");
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return await RedirectBasedOnRole(user, returnUrl);
                    }
                }
            }

            // 🚨 If no email claim, fall back to Razor Page
            ReturnUrl = returnUrl;
            ProviderDisplayName = info.ProviderDisplayName;
            return Page();
        }

        private async Task<IActionResult> RedirectBasedOnRole(ApplicationUser user, string returnUrl)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (await _userManager.IsInRoleAsync(user, "Farmer"))
            {
                return RedirectToAction("Dashboard", "Farmer");
            }

            return LocalRedirect(returnUrl);
        }
    }
}
