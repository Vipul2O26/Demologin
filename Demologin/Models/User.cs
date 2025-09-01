using Demologin.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

public class ApplicationUser : IdentityUser
{
    public string? ProfilePictureUrl { get; set; }

    // ✅ Navigation properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
}
