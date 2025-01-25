using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductsService.Data;
public class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Product> Products { get; set; }

}