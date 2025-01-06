using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace OrdersService.Data;

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

    public async Task<Order> GetOrderById(int orderId)
    {
        var order = await _context.Orders.Where(u => u.Id == orderId).FirstOrDefaultAsync();
        return (order == null) ? null : order;
    }

    public async Task<int> UpdateUser(Order order)
    {
        await _context.Orders.Where(e => e.Id == order.Id)
            .ExecuteUpdateAsync(x => x
            .SetProperty(p => p.UserId, p => order.UserId)
            .SetProperty(p => p.AccountId, p => order.AccountId)
            .SetProperty(p => p.Title, p => order.Title)
            .SetProperty(p => p.Description, p => order.Description)
            .SetProperty(p => p.Amount, p => order.Amount)
            );

        await _context.SaveChangesAsync();

        return order.Id;
    }

    public async Task<int> UpdateUserPartial(Order orderorder)
    {
        var userEntity = await _context.Orders.Where(u => u.Id == orderorder.Id).FirstOrDefaultAsync();

        if (userEntity == null)
            return 0;

        if (orderorder.UserId != 0)
            userEntity.UserId = orderorder.UserId;

        if (orderorder.AccountId != 0)
            userEntity.AccountId = orderorder.AccountId;

        if (orderorder.Title != null)
            userEntity.Title = orderorder.Title;

        if (orderorder.Description != null)
            userEntity.Description = orderorder.Description;

        if (orderorder.Amount != 0)
            userEntity.Amount = orderorder.Amount;

        _context.Orders.Update(userEntity);
        await _context.SaveChangesAsync();
        return orderorder.Id;
    }

    public async Task<bool> ResetDb()
    {
        await _context.Database.EnsureDeletedAsync();
        var result = await _context.Database.EnsureCreatedAsync();
        return result;
    }
}