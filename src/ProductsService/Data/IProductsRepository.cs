using CoreLogic.Models;

namespace ProductsService.Data;

public interface IProductsRepository
{
    Task<Product> CreateProduct(Product product);

    Task<bool> DeleteProduct(int productId);

    Task<List<Product>> GetAllProducts();

    Task<Product> GetProductById(int orderId);

    Task<int> UpdateProduct(Product product);

    Task<bool> SetProductStatus(int orderId, OrderStatus orderStatus);

    Task<bool> ResetDb();

    string GetConnectionInfo();
}
