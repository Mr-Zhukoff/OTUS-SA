using CoreLogic.Models;

namespace OrdersService.Data;

public interface IOrdersRepository
{
    public Task<Order> CreateOrder(Order order);

    public Task<bool> DeleteOrder(int orderId);

    public Task<List<Order>> GetAllOrders();

    public Task<List<Order>> GetAllUserOrders(int userId);

    Task<List<Order>> GetOrdersByStatus(OrderStatus status);

    Task<bool> SetOrderStatus(int orderId, OrderStatus orderStatus);

    public Task<Order> GetOrderById(int orderId);

    public Task<int> UpdateOrder(Order order);

    public Task<bool> ResetDb();
}