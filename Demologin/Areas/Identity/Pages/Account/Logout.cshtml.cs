using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demologin.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
         
            await _signInManager.SignOutAsync();

         
            HttpContext.Session.Clear();

            _logger.LogInformation("User logged out and session destroyed.");

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
         
                return RedirectToPage("/Account/Login");
               
            }
        }

    }
}
