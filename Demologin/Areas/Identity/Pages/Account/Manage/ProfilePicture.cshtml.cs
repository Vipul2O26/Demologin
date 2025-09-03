using Demologin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demologin.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class ProfilePictureModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProfilePictureModel(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        [BindProperty]
        public IFormFile ProfilePicture { get; set; }

        public string CurrentPictureUrl { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            CurrentPictureUrl = user.ProfilePictureUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ProfilePicture != null)
            {
                // Save file in wwwroot/images/profile
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "profile");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ProfilePicture.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(stream);
                }

                // ✅ Save relative URL in DB
                user.ProfilePictureUrl = $"/images/profile/{fileName}";
                await _userManager.UpdateAsync(user);
            }

            return RedirectToPage();
        }
    }
}
