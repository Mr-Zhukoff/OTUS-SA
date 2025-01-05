using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace NotificationsService.Data;

public interface INotificationsRepository
{
    public Task<Notification> CreateNotification(Notification notification);

    public Task<bool> DeleteNotification(int notificationId);

    public Task<List<Notification>> GetAllNotifications();

    public Task<Notification> GetNotificationById(int userId);

    public Task<Notification> GetNotificationByTitle(string title);

    public Task<int> UpdateNotification(Notification notification);

    public  Task<int> UpdateNotificationPartial(Notification user);

    public Task<bool> ResetDb();
}