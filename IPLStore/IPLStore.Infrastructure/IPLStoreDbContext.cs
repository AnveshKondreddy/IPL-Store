using IPLStore.Domain;
using Microsoft.EntityFrameworkCore;

namespace IPLStore.Infrastructure
{
    public class IPLStoreDbContext(DbContextOptions<IPLStoreDbContext> options) : DbContext(options)
    {
        private static readonly Franchise[] SeedFranchises =
    [
        new() { Id = 1, Code = "MI", Name = "Mumbai Indians" },
        new() { Id = 2, Code = "CSK", Name = "Chennai Super Kings" },
        new() { Id = 3, Code = "RCB", Name = "Royal Challengers Bengaluru" },
        new() { Id = 4, Code = "KKR", Name = "Kolkata Knight Riders" }
    ];
        private static readonly Product[] SeedProducts =
[
    new()
    {
        Id = 1,
        Name = "Mumbai Indians Match Jersey",
        Type = "Jersey",
        Description = "Official blue match jersey inspired by the Mumbai Indians home kit.",
        Price = 2499m,
        FranchiseId = 1,
        StockQty = 50,
        IsActive = true
    },
    new()
    {
        Id = 2,
        Name = "Mumbai Indians Supporter Cap",
        Type = "Cap",
        Description = "Adjustable supporter cap with the Mumbai Indians crest embroidered on the front.",
        Price = 799m,
        FranchiseId = 1,
        StockQty = 80,
        IsActive = true
    },
    new()
    {
        Id = 3,
        Name = "Chennai Super Kings Fan Jersey",
        Type = "Jersey",
        Description = "Bright yellow fan jersey for Chennai Super Kings match days.",
        Price = 2299m,
        FranchiseId = 2,
        StockQty = 60,
        IsActive = true
    },
    new()
    {
        Id = 4,
        Name = "Chennai Super Kings Coffee Mug",
        Type = "Mug",
        Description = "Ceramic coffee mug featuring the roaring Chennai Super Kings identity.",
        Price = 499m,
        FranchiseId = 2,
        StockQty = 120,
        IsActive = true
    },
    new()
    {
        Id = 5,
        Name = "RCB Stadium Hoodie",
        Type = "Hoodie",
        Description = "Premium black hoodie celebrating Royal Challengers Bengaluru home support.",
        Price = 2999m,
        FranchiseId = 3,
        StockQty = 35,
        IsActive = true
    },
    new()
    {
        Id = 6,
        Name = "RCB Signature T-Shirt",
        Type = "TShirt",
        Description = "Soft cotton tee with Royal Challengers Bengaluru signature graphics.",
        Price = 999m,
        FranchiseId = 3,
        StockQty = 90,
        IsActive = true
    },
    new()
    {
        Id = 7,
        Name = "KKR Match Day Flag",
        Type = "Flag",
        Description = "Purple and gold hand flag for Kolkata Knight Riders match day support.",
        Price = 349m,
        FranchiseId = 4,
        StockQty = 150,
        IsActive = true
    },
    new()
    {
        Id = 8,
        Name = "KKR Training Jersey",
        Type = "Jersey",
        Description = "Lightweight training jersey inspired by Kolkata Knight Riders practice wear.",
        Price = 1999m,
        FranchiseId = 4,
        StockQty = 45,
        IsActive = true
    }
];
        public DbSet<Franchise> Franchises => Set<Franchise>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Franchise>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Code).HasMaxLength(16).IsRequired();
                entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
                entity.HasIndex(x => x.Code).IsUnique();
                entity.HasData(SeedFranchises);
            });


            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
                entity.Property(x => x.Type).HasMaxLength(60).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
                entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
                entity.Property(x => x.RowVersion).IsRowVersion();
                entity.HasOne(x => x.Franchise)
                .WithMany()
                .HasForeignKey(x => x.FranchiseId)
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => new { x.Type, x.IsActive });
                entity.HasIndex(x => x.FranchiseId);
                entity.HasIndex(x => x.Name);
                entity.HasIndex(x => new { x.FranchiseId, x.Type });
                entity.HasData(SeedProducts);
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UserId).HasMaxLength(128).IsRequired();
                entity.HasIndex(x => x.UserId).IsUnique();
                entity.HasMany(x => x.Items)
                    .WithOne(x => x.Cart)
                    .HasForeignKey(x => x.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
                entity.HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();
                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UserId).HasMaxLength(128).IsRequired();
                entity.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
                entity.HasIndex(x => new { x.UserId, x.OrderedAt });
                entity.HasMany(x => x.Items)
                    .WithOne(x => x.Order)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ProductName).HasMaxLength(200);
                entity.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
                entity.HasIndex(x => x.OrderId);
            });
        }

        
    }
}
