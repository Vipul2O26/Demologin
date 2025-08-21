using Demologin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IWebHostEnvironment _env;

    public ProfileModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment env)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _env = env;
    }

    [BindProperty]
    public string Username { get; set; }

    [BindProperty]
    public string Email { get; set; }

    [BindProperty]
    public IFormFile? ProfilePicture { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");

        Username = user.UserName;
        Email = user.Email;
        ProfilePictureUrl = user.ProfilePictureUrl;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");

        user.UserName = Username;
        user.Email = Email;

        if (ProfilePicture != null)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ProfilePicture.FileName)}";
            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ProfilePicture.CopyToAsync(stream);
            }

            user.ProfilePictureUrl = $"/uploads/{fileName}";
        }

        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        return RedirectToPage();
    }
}
