using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace HangfireService.Data;

public class OrdersRepository(OrdersDbContext context) : IOrdersRepository
{
    private readonly OrdersDbContext _context = context;
    public async Task<Order> CreateOrder(Order order)
    {
        var result = await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<bool> DeleteOrder(int orderId)
    {
        await _context.Orders.Where(e => e.Id == orderId).ExecuteDeleteAsync();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Order>> GetAllOrders()
    {
        var orders = await _context.Orders.AsNoTracking().ToListAsync();

        return orders.ToList();
    }

    public async Task<List<Order>> GetAllUserOrders(int userId)
    {
        if (userId == 0)
            return new List<Order>();

        var orders = await _context.Orders.Where(x => x.UserId == userId).ToListAsync();

        return orders.ToList();
    }

    public async Task<List<Order>> GetOrdersByStatus(OrderStatus status)
    {
        var orders = await _context.Orders.Where(x => x.Status == status).ToListAsync();

        return orders.ToList();
    }

    public async Task<Order> GetOrderById(int orderId)
    {
        var order = await _context.Orders.Where(u => u.Id == orderId).FirstOrDefaultAsync();
        return (order == null) ? null : order;
    }

    public async Task<int> UpdateOrder(Order order)
    {
        var currentOrder = await _context.Orders.Where(u => u.Id == order.Id).FirstOrDefaultAsync();

        if (currentOrder == null)
            return 0;

        if (order.UserId != 0)
            currentOrder.UserId = order.UserId;

        if (order.AccountId != 0)
            currentOrder.AccountId = order.AccountId;

        if (order.Title != null)
            currentOrder.Title = order.Title;

        if (order.Description != null)
            currentOrder.Description = order.Description;

        if (order.Total != 0)
            currentOrder.Total = order.Total;

        _context.Orders.Update(currentOrder);
        await _context.SaveChangesAsync();
        return order.Id;
    }

    public async Task<bool> SetOrderStatus(int orderId, OrderStatus orderStatus)
    {
        await _context.Orders.Where(o => o.Id == orderId).ExecuteUpdateAsync(
            t => t.SetProperty(u => u.Status, u => orderStatus));
        await _context.SaveChangesAsync();
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
