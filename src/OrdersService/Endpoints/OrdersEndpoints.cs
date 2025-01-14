﻿using Confluent.Kafka;
using CoreLogic.Models;
using CoreLogic.Security;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using OrdersService.Data;
using OrdersService.Models;
using Serilog;
using System.Net.Http.Headers;

namespace OrdersService.Endpoints;

public static class OrdersEndpoints
{
    private const string _topic = "orderForm-events";

    public static void AddOrdersEndpoints(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapGet("/", [AllowAnonymous] () => "OrderService");

        app.MapGet("/orders", async (IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            var orders = await ordersRepository.GetAllUserOrders(PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]));
            return Results.Ok(orders);
        });

        app.MapGet("/orders/{id:int}", async (int id, IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);
            var order = await ordersRepository.GetOrderById(id);

            if(requestUserId != 0 && order.UserId != requestUserId)
                return Results.BadRequest("Accessing another user order data is not allowed!");

            return Results.Ok(order);
        });

        app.MapPost("/orders", async (UpdateOrderForm orderForm, IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            try
            {
                int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

                var newOrder = orderForm.ToOrder();
                newOrder.UserId = requestUserId;
                newOrder.Status = OrderStatus.New;
                Account account;
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.Headers.Authorization.FirstOrDefault().Replace("Bearer ", ""));
                    using (var accountResponse = await httpClient.GetAsync($"{config.GetSection("Services:BillingServiceUrl").Get<string>()}/accounts/{newOrder.AccountId}"))
                    {
                        string response = await accountResponse.Content.ReadAsStringAsync();
                        account = JsonConvert.DeserializeObject<Account>(response);
                    }
                }

                if(account == null)
                    return Results.BadRequest($"Account {newOrder.AccountId} not found!");

                var result = await ordersRepository.CreateOrder(newOrder);
                return Results.Ok(result);
            }
            catch (Exception ex) {
                Log.Error(ex, "Create order error!");
                return Results.Problem(ex.Message, null, 500, "Error!");
            }
        });

        app.MapPut("/orders/{id:int}", async (int id, UpdateOrderForm userForm, IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != 0 && requestUserId != id)
                return Results.BadRequest("Modifying another orderForm is not allowed!");

            var result = await ordersRepository.UpdateOrder(userForm.ToOrder(id));
            return Results.Ok(result);
        });
        app.MapPatch("/orders/{id:int}", async (int id, UpdateOrderForm orderForm, IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != 0 && requestUserId != id)
                return Results.BadRequest("Modifying another orderForm is not allowed!");

            var result = await ordersRepository.UpdateOrder(orderForm.ToOrder(id));
            return Results.Ok(result);
        });
        app.MapDelete("/orders", [Authorize] async (int id, IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != 0 && requestUserId != id)
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

        //app.MapPost("/sendnotification", [AllowAnonymous] async (Order order, IOrdersRepository ordersRepository, IProducer<string, string> producer) =>
        //{
        //    try
        //    {
        //        Log.Information($"Sending notification");
        //        var kafkaMessage = new Message<string, string>
        //        {
        //            Value = JsonConvert.SerializeObject(order)
        //        };
        //        await producer.ProduceAsync(_topic, kafkaMessage);
        //        return Results.Ok($"Order placed successfully. topic {_topic}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, ex.Message);
        //        return Results.Problem(ex.Message, null, 500, "Sendnotification error!");
        //    }
        //});
    }
}
