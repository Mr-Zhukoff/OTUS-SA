using CoreLogic.Models;

namespace HangfireService.Data;

public interface INotificationsRepository
{
    Task<Notification> CreateNotification(Notification notification);
    Task<bool> DeleteNotification(int notificationId);
    Task<List<Notification>> GetAllNotifications();
    Task<Notification> GetNotificationById(int userId);
    Task<Notification> GetNotificationByTitle(string title);
    Task<int> UpdateNotification(Notification notification);
    Task<int> UpdateNotificationPartial(Notification user);
    Task<bool> ResetDb();
    string GetConnectionInfo();
}
