using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace BackgroundWorkerService.Data;

public class NotificationsRepository(NotificationsDbContext context) : INotificationsRepository
{
    private readonly NotificationsDbContext _context = context;

    public async Task<Notification> CreateNotification(Notification notification)
    {
        var result = await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<bool> DeleteNotification(int notificationId)
    {
        await _context.Notifications.Where(e => e.Id == notificationId).ExecuteDeleteAsync();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Notification>> GetAllNotifications()
    {
        var userEntities = await _context.Notifications.AsNoTracking().ToListAsync();

        return userEntities.ToList();
    }

    public async Task<Notification> GetNotificationById(int userId)
    {
        var userEntity = await _context.Notifications.Where(u => u.Id == userId).FirstOrDefaultAsync();
        return (userEntity == null) ? null : userEntity;
    }

    public async Task<Notification> GetNotificationByTitle(string title)
    {
        var userEntity = await _context.Notifications.Where(u => u.Title.ToLower() == title.ToLower()).FirstOrDefaultAsync();
        return (userEntity == null) ? null : userEntity;
    }

    public async Task<int> UpdateNotification(Notification notification)
    {
        await _context.Notifications.Where(e => e.Id == notification.Id)
            .ExecuteUpdateAsync(x => x
            .SetProperty(p => p.UserId, p => notification.UserId)
            .SetProperty(p => p.Title, p => notification.Title)
            .SetProperty(p => p.Body, p => notification.Body)
            );

        await _context.SaveChangesAsync();

        return notification.Id;
    }

    public async Task<int> UpdateNotificationPartial(Notification user)
    {
        var userEntity = await _context.Notifications.Where(u => u.Id == user.Id).FirstOrDefaultAsync();

        if (userEntity == null)
            return 0;

        if (user.UserId != 0)
            userEntity.UserId = user.UserId;

        if (user.Title != null)
            userEntity.Title = user.Title;

        if (user.Body != null)
            userEntity.Body = user.Body;

        _context.Notifications.Update(userEntity);
        await _context.SaveChangesAsync();
        return user.Id;
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