using System.Reflection;
using marketimnet.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using marketimnet.Data.EntityConfigurations;

namespace marketimnet.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            ChangeTracker.LazyLoadingEnabled = false;
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Slider> Sliders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PaymentInformation> PaymentInformation { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=marketimnetDb;Trusted_Connection=True;TrustServerCertificate=True;",
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                        sqlOptions.CommandTimeout(30);
                        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    })
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.DiscountedPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name);
            
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CategoryId);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Password)
                .IsRequired();

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Surname)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Phone)
                .HasMaxLength(20);

            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    Id = 1,
                    Name = "Admin",
                    Surname = "Admin",
                    Email = "admin@marketimnet.com",
                    Password = "AQAAAAIAAYagAAAAELbhHh4kk/yqWX1MJmhAWqA6PupAyO5csF8WGRiCRYhNBd8OGEYGVxf5Yw6Jz1vPxA==",
                    IsActive = true,
                    IsAdmin = true,
                    CreatedDate = DateTime.Now,
                    UserGuid = Guid.NewGuid()
                }
            );

            base.OnModelCreating(modelBuilder);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    builder.Append(hashedBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
} 