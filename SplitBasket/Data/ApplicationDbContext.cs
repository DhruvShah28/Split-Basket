using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SplitBasket.Models;

namespace SplitBasket.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
        public DbSet<Member> Members { get; set; }
        public DbSet<GroceryItem> GroceryItems { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<GroupPurchase> GroupPurchases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define the One-to-Many relationship between GroceryItem and GroupPurchase
            modelBuilder.Entity<GroupPurchase>()
                .HasOne(gp => gp.GroceryItem)
                .WithMany(gi => gi.GroupPurchases)
                .HasForeignKey(gp => gp.GroceryItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Define the One-to-Many relationship between Purchase and GroupPurchase
            modelBuilder.Entity<GroupPurchase>()
                .HasOne(gp => gp.Purchase)
                .WithMany(p => p.GroupPurchases)
                .HasForeignKey(gp => gp.PurchaseId)
                .OnDelete(DeleteBehavior.SetNull); // Allows null values in PurchaseId
        }

    }


}
