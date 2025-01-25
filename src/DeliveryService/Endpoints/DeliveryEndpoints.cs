using CoreLogic.Models;
using CoreLogic.Security;
using DeliveryService.Data;
using DeliveryService.Models;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using System.Reflection;

namespace DeliveryService.Endpoints;

public static class DeliveryEndpoints
{
    public static void AddDeliveryEndpoints(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapGet("/", () => "DeliveryService");

        app.MapGet("/deliveries", [AllowAnonymous] async (IDeliveriesRepository deliveriesRepository) =>
        {
            var deliveries = await deliveriesRepository.GetAllDeliveries();
            return Results.Ok(deliveries);
        });

        app.MapGet("/deliveries/{id:int}", async (int id, IDeliveriesRepository deliveriesRepository) =>
        {
            var delivery = await deliveriesRepository.GetDeliveryById(id);
            return Results.Ok(delivery);
        });

        app.MapPost("/deliveries", async (Delivery delivery, IDeliveriesRepository deliveriesRepository) =>
        {
            var result = await deliveriesRepository.CreateDelivery(delivery);
            return Results.Ok(result);
        });

        app.MapPut("/deliveries/{id:int}", async (int id, UpdateDeliveryForm notificationsForm, IDeliveriesRepository deliveriesRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another user is not allowed!");

            var result = await deliveriesRepository.UpdateDelivery(notificationsForm.ToDelivery(id));
            return Results.Ok(result);
        });
        app.MapPatch("/deliveries/{id:int}", async (int id, UpdateDeliveryForm userForm, IDeliveriesRepository deliveriesRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another user is not allowed!");

            var result = await deliveriesRepository.UpdateDelivery(userForm.ToDelivery(id));
            return Results.Ok(result);
        });
        app.MapDelete("/deliveries", async (int id, IDeliveriesRepository deliveriesRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Deleting another user is not allowed!");

            var result = await deliveriesRepository.DeleteDelivery(id);
            return Results.Ok(result);
        });

        app.MapGet("/resetdb", [AllowAnonymous] async (IDeliveriesRepository deliveriesRepository) =>
        {
            try
            {
                Log.Information($"Resetting Deliveries DB");
                var result = await deliveriesRepository.ResetDb();
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Results.Problem(ex.Message, null, 500, "resetdb error!");
            }
        });

        app.MapGet("/health", [AllowAnonymous] (IDeliveriesRepository deliveriesRepository) =>
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
                    pgconnstr = deliveriesRepository.GetConnectionInfo()
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
