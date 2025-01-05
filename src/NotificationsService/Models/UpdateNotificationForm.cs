using CoreLogic.Models;

namespace NotificationsService.Models;

public class UpdateNotificationForm
{
    public int UserId { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public override string ToString()
    {
        return $"{UserId}: {Title}";
    }
    public Notification ToNotification(int notificationId)
    {
        return new Notification
        {
            Id = notificationId,
            UserId = UserId,
            Title = Title,
            Body = Body
        };
    }
}
