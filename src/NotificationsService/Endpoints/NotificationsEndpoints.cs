using CoreLogic.Models;
using Microsoft.AspNetCore.Authorization;
using NotificationsService.Data;
using NotificationsService.Models;
using System.IdentityModel.Tokens.Jwt;

namespace NotificationsService.Endpoints;

public static class NotificationsEndpoints
{
    public static void AddNotificationsEndpoints(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapGet("/", () => "NotificationsService");

        app.MapGet("/notifications", [AllowAnonymous] async (INotificationsRepository notificationsRepository) =>
        {
            var notifications = await notificationsRepository.GetAllNotifications();
            return Results.Ok(notifications);
        });

        app.MapGet("/notifications/{id:int}", async (int id, INotificationsRepository notificationsRepository) =>
        {
            var notification = await notificationsRepository.GetNotificationById(id);
            return Results.Ok(notification);
        });

        app.MapGet("/notifications/{title}", async (string title, INotificationsRepository notificationsRepository) =>
        {
            var notification = await notificationsRepository.GetNotificationByTitle(title);
            return Results.Ok(notification);
        });

        app.MapPost("/notifications", async (Notification notification, INotificationsRepository notificationsRepository) =>
        {
            var result = await notificationsRepository.UpdateNotification(notification);
            return Results.Ok(result);
        });

        app.MapPut("/notifications/{id:int}", async (int id, UpdateNotificationForm notificationsForm, INotificationsRepository notificationsRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another user is not allowed!");

            var result = await notificationsRepository.UpdateNotification(notificationsForm.ToNotification(id));
            return Results.Ok(result);
        });
        app.MapPatch("/notifications/{id:int}", async (int id, UpdateNotificationForm userForm, INotificationsRepository notificationsRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another user is not allowed!");

            var result = await notificationsRepository.UpdateNotificationPartial(userForm.ToNotification(id));
            return Results.Ok(result);
        });
        app.MapDelete("/notifications", [Authorize] async (int id, INotificationsRepository userRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Deleting another user is not allowed!");

            var result = await userRepository.DeleteNotification(id);
            return Results.Ok(result);
        });
    }

    private static int GetUserIdFromJwt(string authHeader)
    {
        if (String.IsNullOrEmpty(authHeader))
            return -1;

        var token = authHeader.Replace("Bearer ", "");
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var userId = jwtSecurityToken.Claims.First(claim => claim.Type == "id").Value;
        return int.Parse(userId);
    }
}