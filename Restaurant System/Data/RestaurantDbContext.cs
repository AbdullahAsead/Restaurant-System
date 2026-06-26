using Microsoft.EntityFrameworkCore;
using Restaurant_System.Models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Restaurant_System.Data
{
    public class RestaurantDbContext : DbContext
    {
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<DeliveryEmployee> DeliveryPeople { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<DashboardPass> DashboardPasses { get; set; }
        public DbSet<DeliveryPlace> DeliveryPlaces { get; set; }
        public DbSet<OrderItem>OrderItems { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "restaurant.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ ربط الجداول بالمفاتيح الأساسية
            modelBuilder.Entity<Categories>().ToTable("Categories").HasKey(c => c.Id);
            modelBuilder.Entity<Item>().ToTable("Item").HasKey(i => i.Id);

            // ✅ ربط العلاقة بين Category و Item
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ بيانات افتراضية للـ Admin
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    Id = 1,
                    FullName = "Super Admin",
                    Username = "admin",
                    Password = "1234" // ممكن بعدين نعملها Hash باستخدام ComputeSha256Hash
                }
            );

            // ✅ بيانات افتراضية للـ DashboardPass
            modelBuilder.Entity<DashboardPass>().HasData(
                new DashboardPass
                {
                    Id = 1,
                    Username = "Boda",
                    PasswordHash = "111",
                    CreatedAt = new DateTime(2025, 9, 27, 0, 0, 0) // قيمة ثابتة
                },
                new DashboardPass
                {
                    Id = 2,
                    Username = "Admin2",
                    PasswordHash = "222",
                    CreatedAt = new DateTime(2025, 9, 27, 0, 0, 0) // قيمة ثابتة
                }
            );
        }

        // 🔒 دالة لحساب SHA256 Hash (ممكن نستخدمها لو هنخزن الباسورد مشفر)
        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
