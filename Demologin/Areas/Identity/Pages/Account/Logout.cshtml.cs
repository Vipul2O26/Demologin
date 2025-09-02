using System.Threading.Tasks;
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
            // ✅ Sign out Identity cookies
            await _signInManager.SignOutAsync();

            // ✅ Clear session (if used)
            HttpContext.Session.Clear();

            _logger.LogInformation("User logged out.");

            // Always redirect to Home/Index after logout
            return RedirectToAction("Index", "Home");
        }
    }
}
