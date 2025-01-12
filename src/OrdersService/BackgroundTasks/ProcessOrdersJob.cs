using Confluent.Kafka;
using CoreLogic.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OrdersService.Data;
using Quartz;
using Serilog;
using System.Net.Http.Headers;

namespace OrdersService.BackgroundTasks;

[DisallowConcurrentExecution]
public class ProcessOrdersJob : IJob
{
    private static int count;
    private readonly string _topic = "order-events";
    private readonly IOrdersRepository _ordersRepository;
    private readonly IConfiguration _config;
    private readonly IProducer<string, string> _producer;
    private readonly AuthenticationHeaderValue _authHeader;
    private readonly IMemoryCache _cache;
    private readonly string authHeaderKey = "authheader";

    public ProcessOrdersJob(IConfiguration configuration, IMemoryCache cache, IOrdersRepository ordersRepository, IProducer<string, string> producer)
    {
        _ordersRepository = ordersRepository;
        _config = configuration;
        _producer = producer;
        _cache = cache;
        _topic = _config["Kafka:Topic"];
        _authHeader = GetAuthHeader().Result; // Добавить кеширование
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var newOrders = await _ordersRepository.GetOrdersByStatus(OrderStatus.New);
            Log.Information($"Processing new orders {newOrders.Count} qty.");

            foreach (var order in newOrders)
            {
                Log.Information($"Processing new order {order.Id}");
                await _ordersRepository.SetOrderStatus(order.Id, OrderStatus.Processing);
                Account account;

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = _authHeader;
                    using (var accountResponse = await httpClient.GetAsync($"{_config.GetSection("Services:BillingServiceUrl").Get<string>()}/accounts/{order.AccountId}"))
                    {
                        string response = await accountResponse.Content.ReadAsStringAsync();
                        if (accountResponse.IsSuccessStatusCode)
                        {
                            account = JsonConvert.DeserializeObject<Account>(response);
                        }
                        else
                        {
                            Log.Error($"Get account request error!\n\r StatusCode:{accountResponse.StatusCode}\n\r {response}");
                            continue;
                        }
                    }
                }

                if (account == null)
                {
                    Log.Warning($"Processing order {order.Id} error! Account with Id:{order.Id} not found.");
                    continue;
                }

                if (account.Balance >= order.Amount)
                {
                    int transactionId;
                    // Создаем транзакцию
                    using (var httpClient = new HttpClient())
                    {
                        var transaction = new Transaction()
                        {
                            UserId = order.UserId,
                            AccountId = order.AccountId,
                            Amount = order.Amount,
                            Description = order.Description,
                            CreatedOn = DateTime.Now
                        };
                        JsonContent content = JsonContent.Create(transaction);
                        httpClient.DefaultRequestHeaders.Authorization = _authHeader;
                        using (var transactionResponse = await httpClient.PostAsync($"{_config.GetSection("Services:BillingServiceUrl").Get<string>()}/transactions", content))
                        {
                            string response = await transactionResponse.Content.ReadAsStringAsync();
                            if (transactionResponse.IsSuccessStatusCode)
                            {
                                int.TryParse(response, out transactionId);
                            }
                            else
                            {
                                Log.Error($"Create transaction request error!\n\r StatusCode:{transactionResponse.StatusCode}\n\r {response}");
                                continue;
                            }
                        }
                    }

                    await _ordersRepository.SetOrderStatus(order.Id, OrderStatus.Paid);
                    Log.Information($"Order {order.Id} is paid with transaction {transactionId}");

                    // Отправляем сообщение об успешной обработке товара 
                    Log.Information($"Sending order {order.Id} success payment notification");

                    var notification = new Notification()
                    {
                        UserId = account.UserId,
                        Title = $"Успешная оплата заказа {order.Title}",
                        Body = $"Здраствуйте! Ваш заказ {order.Title} успешно оплачен.",
                        CreatedOn = DateTime.UtcNow
                    };

                    var kafkaMessage = new Message<string, string> { Value = JsonConvert.SerializeObject(notification) };
                    await _producer.ProduceAsync(_topic, kafkaMessage);
                }
                else
                {
                    // Отправляем сообщение о недостатке баланса
                    Log.Information($"Insufficient balance on account {account.Id}");

                    var notification = new Notification()
                    {
                        UserId = account.UserId,
                        Title = $"Недостаточно средств для оплаты заказа {order.Title}",
                        Body = $"Здраствуйте! К сожалению, на вашем счете {account.Number} недостаточно недостаточно средств для оплаты заказа {order.Title}.",
                        CreatedOn = DateTime.UtcNow
                    };

                    var kafkaMessage = new Message<string, string> { Value = JsonConvert.SerializeObject(notification) };
                    await _producer.ProduceAsync(_topic, kafkaMessage);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Processing new orders error!");
        }
    }

    private async Task<AuthenticationHeaderValue> GetAuthHeader()
    {
        AuthenticationHeaderValue authHeader;

        if (!_cache.TryGetValue(authHeaderKey, out authHeader))
        {
            using (var httpClient = new HttpClient())
            {
                var loginForm = new LoginForm("admin@zhukoff.pro", "P@ssw0rd");
                JsonContent content = JsonContent.Create(loginForm);
                using (var transactionResponse = await httpClient.PostAsync($"{_config.GetSection("Services:UsersServiceUrl").Get<string>()}/login", content))
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

                        _cache.Set(authHeaderKey, authHeader, cacheEntryOptions);

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
}
