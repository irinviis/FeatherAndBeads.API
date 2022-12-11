using FeatherAndBeads.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatherAndBeads.API
{
    public class Database : DbContext
    {
        public Database(DbContextOptions<Database> options) : base(options)
        {
        }

        public DbSet<Product> Product { get; set; }
        public DbSet<Photo> Photo { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderProduct> OrderProduct { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<Right> Right { get; set; }
        public DbSet<UserRight> UserRight { get; set; }
    }
}
