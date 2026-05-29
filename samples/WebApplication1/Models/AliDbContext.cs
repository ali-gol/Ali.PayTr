using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using Ali.PayTr.EFCore.Extensions;
namespace WebApplication1.Models
{
    public class AliDbContext : DbContext
    {
        public AliDbContext(DbContextOptions options)
            : base(options)
        {

        }
        public DbSet<Product> Product { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyPayTrPaymentModels(options =>
            {
                options.Schema = "_PayTr";
                options.TablePrefix = "_";
            });
        }

    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
    }
}
