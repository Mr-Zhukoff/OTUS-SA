using Confluent.Kafka;
using CoreLogic.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Principal;
using static Confluent.Kafka.ConfigPropertyNames;

namespace HangfireService;

public class ProcessOrdersJob
{
    private readonly IConfiguration _config;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IProducer<string, string> _producer;
    private readonly AuthenticationHeaderValue _authHeader;
    private readonly IMemoryCache _cache;
    private readonly string _authHeaderKey = "authheader";
    private readonly string _notificationsTopic = "notifications";
    private readonly string _billingServiceUrl;
    private readonly string _usersServiceUrl;
    private readonly string _productsServiceUrl;
    private readonly string _ordersServiceUrl;

    public ProcessOrdersJob(IConfiguration configuration, IMemoryCache cache, IProducer<string, string> producer)
    {
        _config = configuration;
        _cache = cache;
        _ordersServiceUrl = Environment.GetEnvironmentVariable("ORDERSSVC_URL") ?? _config["Services:OrdersServiceUrl"];
        _billingServiceUrl = Environment.GetEnvironmentVariable("BILLINGSVC_URL") ?? _config["Services:BillingServiceUrl"];
        _usersServiceUrl = Environment.GetEnvironmentVariable("USERSSVC_URL") ?? _config["Services:UsersServiceUrl"];
        _productsServiceUrl = Environment.GetEnvironmentVariable("PRODUCTSSVC_URL") ?? _config["Services:ProductsServiceUrl"];
        _authHeader = GetAuthHeader().Result; // Добавить кеширование
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_URL") ?? _config["Kafka:BootstrapServers"],
            GroupId = "ProcessOrdersConsumerGroup",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _producer = producer;
    }

    public async Task Execute()
    {
        Log.Information("ProcessOrdersJob started");
        try
        {
            _consumer.Subscribe("new-orders");

            while (true)
            {
                var consumeResult = _consumer.Consume();
                if (consumeResult is null)
                    return;

                var newOrder = JsonConvert.DeserializeObject<Order>(consumeResult.Message.Value);
                Log.Information($"Consumed message {consumeResult.Message.Value}");
                await ProcessNewOrderMessage(newOrder);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ProcessOrdersJob error!");
        }
        finally
        {
            _consumer.Close();
            Log.Information("ProcessOrdersJob ended");
        }
    }

    private async Task<bool> ProcessNewOrderMessage(Order order)
    {
        try
        {
            Log.Information($"Processing new order {order.Id}");
            order.Status = await SetOrderStatus(order.Id, OrderStatus.Processing);

            // Получаем счет
            Account account;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = _authHeader;
                using (var accountResponse = await httpClient.GetAsync($"{_billingServiceUrl}/accounts/{order.AccountId}"))
                {
                    string response = await accountResponse.Content.ReadAsStringAsync();
                    if (accountResponse.IsSuccessStatusCode)
                    {
                        order.StatusReason = $"Get account request error!\n\r StatusCode:{accountResponse.StatusCode}\n\r {response}";
                        Log.Error(order.StatusReason);
                        // Отменяем заказ
                        await CancelOrder(order);
                        return false;
                    }
                    account = JsonConvert.DeserializeObject<Account>(response);
                }
            }

            if (account == null)
            {
                order.StatusReason = $"Processing order {order.Id} error! Account with Id:{order.Id} not found.";
                Log.Warning(order.StatusReason);
                await CancelOrder(order);
                return false;
            }

            // Проверяем счет 
            if (account.Balance < order.Total)
            {
                order.StatusReason = $"Insufficient balance on account {account.Number}";
                Log.Warning(order.StatusReason);
                // Отменяем заказ
                await CancelOrder(order);
                return false;
            }

            int transactionId;
            // Создаем транзакцию
            using (var httpClient = new HttpClient())
            {
                var transaction = new Transaction()
                {
                    UserId = order.UserId,
                    AccountId = order.AccountId,
                    OrderId = order.Id,
                    Amount = order.Total,
                    Description = order.Description,
                    CreatedOn = DateTime.Now
                };
                JsonContent content = JsonContent.Create(transaction);
                httpClient.DefaultRequestHeaders.Authorization = _authHeader;
                using (var transactionResponse = await httpClient.PostAsync($"{_billingServiceUrl}/transactions", content))
                {
                    string response = await transactionResponse.Content.ReadAsStringAsync();
                    if (!transactionResponse.IsSuccessStatusCode)
                    {
                        Log.Error($"Create transaction request error!\n\r StatusCode:{transactionResponse.StatusCode}\n\r {response}");
                        return false;
                    }
                    int.TryParse(response, out transactionId);
                }
            }

            order.Status = await SetOrderStatus(order.Id, OrderStatus.Paid);
            Log.Information($"Order {order.Id} is paid with transaction {transactionId}");
            // Отправляем сообщение об успешной обработке товара 
            await SendMessage(order.UserId,
                $"Успешная оплата заказа {order.Title}",
                $"Здраствуйте! Ваш заказ {order.Title} успешно оплачен.");

            // Резервируем товар
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = _authHeader;
                foreach (var product in order.Products)
                {
                    using (var productResponse = await httpClient.GetAsync($"{_productsServiceUrl}/products/{product.ProductId}/reserve/{product.Quantity}"))
                    {
                        string response = await productResponse.Content.ReadAsStringAsync();
                        if (!productResponse.IsSuccessStatusCode)
                        {
                            order.StatusReason = $"Reserve product {product.ProductId} request error!\n\r StatusCode:{productResponse.StatusCode}\n\r {response}";
                            order.Status = OrderStatus.Error;
                            Log.Error(order.StatusReason);
                            await CancelOrder(order);
                            return false;
                        }
                        product.IsReserved = true;
                    }
                }
            }
            order.Status = await SetOrderStatus(order.Id, OrderStatus.Reserved);
            await SendMessage(order.UserId,
                $"Товар для заказа {order.Title} зарезервирован",
                $"Здраствуйте! Товар для заказа {order.Title} зарезервирован");

            // Резервируем доставку




            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Processing new order error!");
            return false;
        }
    }

    private async Task SendMessage(int userId, string title, string body)
    {
        var notification = new Notification()
        {
            UserId = userId,
            Title = title,
            Body = body,
            CreatedOn = DateTime.UtcNow
        };

        var kafkaMessage = new Message<string, string> { Value = JsonConvert.SerializeObject(notification) };
        await _producer.ProduceAsync(_notificationsTopic, kafkaMessage);
    }

    private async Task<OrderStatus> SetOrderStatus(int orderId, OrderStatus status, string reason = "")
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Authorization = _authHeader;
            JsonContent content = JsonContent.Create(reason);
            using (var accountResponse = await httpClient.PostAsync($"{_ordersServiceUrl}/orders/{orderId}/status/{(int)status}", content))
            {
                string response = await accountResponse.Content.ReadAsStringAsync();
                if (accountResponse.IsSuccessStatusCode)
                {
                    return status;
                }
                else
                {
                    Log.Error($"Get account request error!\n\r StatusCode:{accountResponse.StatusCode}\n\r {response}");
                    return OrderStatus.Error;
                }
            }
        }
    }

    private async Task<AuthenticationHeaderValue> GetAuthHeader()
    {
        AuthenticationHeaderValue authHeader;

        if (!_cache.TryGetValue(_authHeaderKey, out authHeader))
        {
            using (var httpClient = new HttpClient())
            {
                var loginForm = new LoginForm("admin@zhukoff.pro", "P@ssw0rd");
                JsonContent content = JsonContent.Create(loginForm);
                using (var transactionResponse = await httpClient.PostAsync($"{_usersServiceUrl}/login", content))
                {
                    string response = await transactionResponse.Content.ReadAsStringAsync();
                    if (transactionResponse.IsSuccessStatusCode)
                    {
                        authHeader = new AuthenticationHeaderValue("Bearer", response.TrimStart('"').TrimEnd('"'));

                        var cacheEntryOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                            SlidingExpiration = TimeSpan.FromMinutes(2)
                        };

                        _cache.Set(_authHeaderKey, authHeader, cacheEntryOptions);

                        return authHeader;
                    }
                    else
                    {
                        Log.Error($"Create transaction request error!\n\r StatusCode:{transactionResponse.StatusCode}\n\r {response}");
                        return null;
                    }
                }
            }
        }
        return authHeader;
    }

    private async Task CancelOrder(Order order)
    {
        switch (order.Status)
        {
            case OrderStatus.New:
                break;
            case OrderStatus.Processing:

                order.Status = await SetOrderStatus(order.Id, OrderStatus.Cancelled, order.StatusReason);
                // Отправляем сообщение о недостатке баланса
                await SendMessage(order.UserId,
                    $"Недостаточно средств для оплаты заказа {order.Title}",
                    $"Здраствуйте! К сожалению, на вашем счете {order.AccountId} недостаточно недостаточно средств для оплаты заказа {order.Title}.");
                break;
            case OrderStatus.Paid:
                break;
            case OrderStatus.Sent:
                break;
            case OrderStatus.Delivered:
                break;
            default:
                break;
        }
    }
}
