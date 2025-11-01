using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Store.Models;

namespace Store.Data
{
    public class StoreContext : DbContext
    {
        public StoreContext (DbContextOptions<StoreContext> options)
            : base(options)
        {
        }
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = default!;
        public DbSet<Review> Reviews { get; set; } = default!;
        public DbSet<Store.Models.User> User { get; set; } = default!;
        public DbSet<Store.Models.Product> Products { get; set; } = default!;
        public DbSet<Store.Models.Category> Categories { get; set; } = default!;
        public DbSet<Store.Models.Brand> Brands { get; set; } = default!;
    }
}
