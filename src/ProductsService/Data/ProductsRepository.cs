using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;
using ProductsService.Data;

namespace OrdersService.Data;

public class ProductsRepository(ProductsDbContext context) : IProductsRepository
{
    private readonly ProductsDbContext _context = context;
    public async Task<Product> CreateProduct(Product product)
    {
        var result = await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<bool> DeleteProduct(int productId)
    {
        await _context.Products.Where(e => e.Id == productId).ExecuteDeleteAsync();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Product>> GetAllProducts()
    {
        var products = await _context.Products.AsNoTracking().ToListAsync();

        return products.ToList();
    }

    public async Task<Product> GetProductById(int productiId)
    {
        var product = await _context.Products.Where(u => u.Id == productiId).FirstOrDefaultAsync();
        return (product == null) ? null : product;
    }

    public async Task<int> UpdateProduct(Product product)
    {
        var currentProduct = await _context.Products.Where(u => u.Id == product.Id).FirstOrDefaultAsync();

        if (currentProduct == null)
            return 0;

        if (product.Title != null)
            currentProduct.Title = product.Title;

        if (product.Description != null)
            currentProduct.Description = product.Description;

        if (currentProduct.Quantity != product.Quantity)
            currentProduct.Quantity = product.Quantity;

        if (currentProduct.Price != product.Price)
            currentProduct.Price = product.Price;

        _context.Products.Update(currentProduct);
        await _context.SaveChangesAsync();
        return product.Id;
    }

    public async Task<bool> SetProductStatus(int orderId, OrderStatus orderStatus)
    {
        //await _context.Products.Where(o => o.Id == productiId).ExecuteUpdateAsync(
        //    t => t.SetProperty(u => u.Status, u => orderStatus));
        //await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetDb()
    {
        await _context.Database.EnsureDeletedAsync();
        var result = await _context.Database.EnsureCreatedAsync();
        return result;
    }

    public string GetConnectionInfo()
    {
        return _context.Database.GetDbConnection().ConnectionString;
    }
}