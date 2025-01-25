using CoreLogic.Models;
using CoreLogic.Security;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using OrdersService.Data;
using OrdersService.Models;
using Serilog;
using System.Net.Http.Headers;
using System.Reflection;

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
                var _authHeader = new AuthenticationHeaderValue("Bearer", request.Headers.Authorization.FirstOrDefault().Replace("Bearer ", ""));

                var newOrder = orderForm.ToOrder();
                newOrder.UserId = requestUserId;
                newOrder.Status = OrderStatus.New;

                Account account;

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = _authHeader;
                    using (var accountResponse = await httpClient.GetAsync($"{Environment.GetEnvironmentVariable("BILLINGSVC_URL")}/accounts/{newOrder.AccountId}"))
                    {
                        string response = await accountResponse.Content.ReadAsStringAsync();
                        account = JsonConvert.DeserializeObject<Account>(response);
                    }
                }

                if(account == null)
                    return Results.BadRequest($"Account {newOrder.AccountId} not found!");

                var orderResult = await ordersRepository.CreateOrder(newOrder);

                if (account.Balance >= newOrder.Amount)
                {
                    int transactionId = 0;
                    // Создаем транзакцию
                    using (var httpClient = new HttpClient())
                    {
                        var transaction = new Transaction()
                        {
                            UserId = newOrder.UserId,
                            AccountId = newOrder.AccountId,
                            Amount = newOrder.Amount * -1,
                            Description = newOrder.Description,
                            CreatedOn = DateTime.Now
                        };
                        JsonContent content = JsonContent.Create(transaction);
                        httpClient.DefaultRequestHeaders.Authorization = _authHeader;
                        using (var transactionResponse = await httpClient.PostAsync($"{Environment.GetEnvironmentVariable("BILLINGSVC_URL")}/transactions", content))
                        {
                            string response = await transactionResponse.Content.ReadAsStringAsync();
                            if (transactionResponse.IsSuccessStatusCode)
                                int.TryParse(response, out transactionId);
                            else
                                Log.Error($"Create transaction request error!\n\r StatusCode:{transactionResponse.StatusCode}\n\r {response}");
                        }
                    }

                    await ordersRepository.SetOrderStatus(newOrder.Id, OrderStatus.Paid);
                    Log.Information($"Order {newOrder.Id} is paid with transaction {transactionId}");

                    // Отправляем сообщение об успешной обработке товара 
                    Log.Information($"Sending order {newOrder.Id} success payment notification");
                    var notification = new Notification()
                    {
                        UserId = account.UserId,
                        Title = $"Успешная оплата заказа {newOrder.Title}",
                        Body = $"Здраствуйте! Ваш заказ {newOrder.Title} успешно оплачен.",
                        CreatedOn = DateTime.UtcNow
                    };
                    await SendNotification(_authHeader, notification);
                }
                else
                {
                    // Отменяем заказ
                    await ordersRepository.SetOrderStatus(newOrder.Id, OrderStatus.Cancelled);
                    Log.Information($"Insufficient balance on account {account.Id}");
                    // Отправляем сообщение о недостатке баланса
                    var notification = new Notification()
                    {
                        UserId = account.UserId,
                        Title = $"Недостаточно средств для оплаты заказа {newOrder.Title}",
                        Body = $"Здраствуйте! К сожалению, на вашем счете {account.Number} недостаточно недостаточно средств для оплаты заказа {newOrder.Title}.",
                        CreatedOn = DateTime.UtcNow
                    };
                    await SendNotification(_authHeader, notification);
                }

                return Results.Ok(orderResult);
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

        app.MapGet("/health", [AllowAnonymous] (IOrdersRepository ordersRepository) =>
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
                    pgconnstr = ordersRepository.GetConnectionInfo()
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

    private static async Task<int> SendNotification(AuthenticationHeaderValue _authHeader, Notification notification)
    {
        int notificationId = 0;
        using (var httpClient = new HttpClient())
        {
            JsonContent content = JsonContent.Create(notification);
            httpClient.DefaultRequestHeaders.Authorization = _authHeader;
            using (var transactionResponse = await httpClient.PostAsync($"{Environment.GetEnvironmentVariable("NOTIFICATIONSSVC_URL")}/notifications", content))
            {
                string response = await transactionResponse.Content.ReadAsStringAsync();
                if (transactionResponse.IsSuccessStatusCode)
                    int.TryParse(response, out notificationId);
                else
                    Log.Error($"Create Notification request error!\n\r StatusCode:{transactionResponse.StatusCode}\n\r {response}");
            }
        }
        return notificationId;
    }
}
