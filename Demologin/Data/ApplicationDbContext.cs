using Demologin.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Demologin.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User → Product (cascade)
            builder.Entity<Product>()
                .HasOne(p => p.User)
                .WithMany(u => u.Products) // ✅ add navigation
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product → Cart (cascade)
            builder.Entity<Cart>()
                .HasOne(c => c.Product)
                .WithMany(p => p.Carts) // ✅ add navigation
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → Cart (restrict to avoid multiple cascade paths)
            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts) // ✅ add navigation
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
