using CoreLogic.Models;

namespace BackgroundWorkerService.Data;

public interface IOrdersRepository
{
    Task<Order> CreateOrder(Order order);
    Task<bool> DeleteOrder(int orderId);
    Task<List<Order>> GetAllOrders();
    Task<List<Order>> GetAllUserOrders(int userId);
    Task<List<Order>> GetOrdersByStatus(OrderStatus status);
    Task<bool> SetOrderStatus(int orderId, OrderStatus orderStatus);
    Task<Order> GetOrderById(int orderId);
    Task<int> UpdateOrder(Order order);
    Task<bool> ResetDb();
    string GetConnectionInfo();
}
