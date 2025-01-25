using CoreLogic.Models;

namespace DeliveryService.Data;

public interface IDeliveriesRepository
{
    Task<Delivery> CreateDelivery(Delivery delivery);
    Task<bool> DeleteDelivery(int deliveryId);

    Task<List<Delivery>> GetAllDeliveries();

    Task<List<Delivery>> GetAllUserDeliveries(int userId);

    Task<Delivery> GetDeliveryById(int deliveryId);

    Task<int> UpdateDelivery(Delivery delivery);

    Task<bool> SetDeliveryStatus(int deliveryId, DeliveryStatus orderStatus);

    Task<bool> ResetDb();

    string GetConnectionInfo();
}
