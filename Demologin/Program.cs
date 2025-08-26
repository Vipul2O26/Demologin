using Demologin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// DB connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Use ApplicationUser here
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// Google Login
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

        options.Events.OnCreatingTicket = async ctx =>
        {
            string? picture = null;
            if (ctx.User.TryGetProperty("picture", out var pictureElement))
            {
                picture = pictureElement.GetString();
            }

            if (!string.IsNullOrEmpty(picture))
            {
                var userManager = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

                // Get the external login user (or null if new)
                var user = await userManager.GetUserAsync(ctx.Principal);

                if (user != null)
                {
                    // Existing user → just update
                    user.ProfilePictureUrl = picture;
                    await userManager.UpdateAsync(user);
                }
                else
                {
                    // First time login → handled in ExternalLoginCallback
                    ctx.Identity!.AddClaim(new Claim("picture", picture));
                }
            }
        };
    });

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Important: this order matters
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
