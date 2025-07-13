using GoEStores.Repositories.ConfigContext;
using GoEStores.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using SalesApp.Models.Entities;

namespace SalesApp.DAL.Data
{
    public class SalesAppDbContext : DbContext
    {
        public SalesAppDbContext(DbContextOptions<SalesAppDbContext> options)
            : base(options)
        {
            base.Database.Migrate();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<StoreLocation> StoreLocations { get; set; }
        public DbSet<ChatHub> ChatHubs { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            EntityConfig.ApplyAll(modelBuilder);
        }
    }
}
