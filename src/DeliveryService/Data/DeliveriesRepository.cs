using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Data;

public class DeliveriesRepository(DeliveryDbContext context) : IDeliveriesRepository
{
    private readonly DeliveryDbContext _context = context;
    public async Task<Delivery> CreateDelivery(Delivery delivery)
    {
        var result = await _context.Deliveries.AddAsync(delivery);
        await _context.SaveChangesAsync();
        return result.Entity;
    }
    public async Task<bool> DeleteDelivery(int deliveryId)
    {
        await _context.Deliveries.Where(e => e.Id == deliveryId).ExecuteDeleteAsync();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Delivery>> GetAllDeliveries()
    {
        var orders = await _context.Deliveries.AsNoTracking().ToListAsync();

        return orders.ToList();
    }

    public async Task<List<Delivery>> GetAllUserDeliveries(int userId)
    {
        if (userId == 0)
            return new List<Delivery>();

        var orders = await _context.Deliveries.Where(x => x.UserId == userId).ToListAsync();

        return orders.ToList();
    }

    public async Task<Delivery> GetDeliveryById(int deliveryId)
    {
        var order = await _context.Deliveries.Where(u => u.Id == deliveryId).FirstOrDefaultAsync();
        return (order == null) ? null : order;
    }

    public async Task<int> UpdateDelivery(Delivery delivery)
    {
        var currentDelivery = await _context.Deliveries.Where(u => u.Id == delivery.Id).FirstOrDefaultAsync();

        if (currentDelivery == null)
            return 0;

        if (delivery.UserId != 0)
            currentDelivery.UserId = delivery.UserId;

        if (delivery.OrderId != 0)
            currentDelivery.OrderId = delivery.OrderId;

        if (delivery.Title != null)
            currentDelivery.Title = delivery.Title;

        if (delivery.Description != null)
            currentDelivery.Description = delivery.Description;

        if (delivery.Address != null)
            currentDelivery.Address = delivery.Address;

        if (delivery.Status != 0)
            currentDelivery.Status = delivery.Status;

        _context.Deliveries.Update(currentDelivery);
        await _context.SaveChangesAsync();
        return delivery.Id;
    }
    
    public async Task<bool> SetDeliveryStatus(int deliveryId, DeliveryStatus orderStatus)
    {
        await _context.Deliveries.Where(o => o.Id == deliveryId).ExecuteUpdateAsync(
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