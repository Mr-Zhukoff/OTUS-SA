using Confluent.Kafka;
using CoreLogic.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using OrdersService.Data;
using OrdersService.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

namespace OrdersService.Endpoints;

public static class OrdersEndpoints
{
    private const string _topic = "orderForm-events";

    public static void AddOrdersEndpoints(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapGet("/", [AllowAnonymous] () => "OrderService");

        app.MapGet("/orders", async (IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            var orders = await ordersRepository.GetAllUserOrders(GetUserIdFromJwt(request.Headers["Authorization"]));
            return Results.Ok(orders);
        });

        app.MapGet("/orders/{id:int}", async (int id, IOrdersRepository ordersRepository) =>
        {
            var order = await ordersRepository.GetOrderById(id);
            return Results.Ok(order);
        });

        app.MapPost("/orders", async (UpdateOrderForm orderForm, IOrdersRepository ordersRepository) =>
        {
            var result = await ordersRepository.CreateOrder(orderForm.ToOrder());
            return Results.Ok(result);
        });

        app.MapPut("/orders/{id:int}", async (int id, UpdateOrderForm userForm, IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another orderForm is not allowed!");

            var result = await ordersRepository.UpdateOrder(userForm.ToOrder(id));
            return Results.Ok(result);
        });
        app.MapPatch("/orders/{id:int}", async (int id, UpdateOrderForm orderForm, IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another orderForm is not allowed!");

            var result = await ordersRepository.UpdateOrder(orderForm.ToOrder(id));
            return Results.Ok(result);
        });
        app.MapDelete("/orders", [Authorize] async (int id, IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Deleting another orderForm is not allowed!");

            var result = await ordersRepository.DeleteOrder(id);
            return Results.Ok(result);
        });

        app.MapGet("/resetdb", [AllowAnonymous] async (IOrdersRepository ordersRepository) =>
        {
            try
            {
                Log.Information($"Resetting Users DB");
                var result = await ordersRepository.ResetDb();
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Results.Problem(ex.Message, null, 500, "ResetDb error!");
            }
        });

        app.MapPost("/sendnotification", [AllowAnonymous] async (Order order, IOrdersRepository ordersRepository, IProducer<string, string> producer) =>
        {
            try
            {
                Log.Information($"Sending notification");
                var kafkaMessage = new Message<string, string>
                {
                    Value = JsonConvert.SerializeObject(order)
                };
                await producer.ProduceAsync(_topic, kafkaMessage);
                return Results.Ok($"Order placed successfully. topic {_topic}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Results.Problem(ex.Message, null, 500, "Sendnotification error!");
            }
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
