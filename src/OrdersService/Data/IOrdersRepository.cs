using CoreLogic.Models;

namespace OrdersService.Data;

public interface IOrdersRepository
{
    public Task<Order> CreateOrder(Order order);

    public Task<bool> DeleteOrder(int orderId);

    public Task<List<Order>> GetAllOrders();

    public Task<Order> GetOrderById(int orderId);

    public Task<int> UpdateUser(Order order);

    public Task<int> UpdateUserPartial(Order orderorder);

    public Task<bool> ResetDb();
}