using CoreLogic.Models;
using CoreLogic.Security;
using Microsoft.AspNetCore.Authorization;
using NotificationsService.Data;
using NotificationsService.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

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
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another user is not allowed!");

            var result = await notificationsRepository.UpdateNotification(notificationsForm.ToNotification(id));
            return Results.Ok(result);
        });
        app.MapPatch("/notifications/{id:int}", async (int id, UpdateNotificationForm userForm, INotificationsRepository notificationsRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another user is not allowed!");

            var result = await notificationsRepository.UpdateNotificationPartial(userForm.ToNotification(id));
            return Results.Ok(result);
        });
        app.MapDelete("/notifications", [Authorize] async (int id, INotificationsRepository notificationsRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Deleting another user is not allowed!");

            var result = await notificationsRepository.DeleteNotification(id);
            return Results.Ok(result);
        });

        app.MapGet("/resetdb", [AllowAnonymous] async (INotificationsRepository notificationsRepository) =>
        {
            try
            {
                Log.Information($"Resetting Users DB");
                var result = await notificationsRepository.ResetDb();
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Results.Problem(ex.Message, null, 500, "Register error!");
            }
        });

        app.MapGet("/health", [AllowAnonymous] (INotificationsRepository notificationsRepository) =>
        {
            try
            {
                Log.Information($"Health status requested {Environment.MachineName}");

                Assembly assembly = Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

                return Results.Ok(new
                {
                    status = "OK",
                    app = Assembly.GetExecutingAssembly().FullName,
                    version = fvi.FileVersion,
                    machinename = Environment.MachineName,
                    osversion = Environment.OSVersion.VersionString,
                    processid = Environment.ProcessId,
                    timestamp = DateTime.Now,
                    pgconnstr = notificationsRepository.GetConnectionInfo()
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new
                {
                    status = "BAD",
                    machinename = Environment.MachineName,
                    osversion = Environment.OSVersion.VersionString,
                    processid = Environment.ProcessId,
                    message = ex.Message
                });
            }
        });
    }
}