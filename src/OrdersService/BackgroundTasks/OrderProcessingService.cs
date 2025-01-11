using Confluent.Kafka;
using CoreLogic.Models;
using Newtonsoft.Json;
using OrdersService.Data;
using Serilog;
using System.Net.Http.Headers;

namespace OrdersService.BackgroundTasks;

public class OrderProcessingService : BackgroundService
{
    private static int count;
    private readonly string _topic = "orderForm-events";
    private readonly TimeSpan _period = TimeSpan.FromSeconds(5);
    private readonly IOrdersRepository _ordersRepository;
    private readonly IConfiguration _config;
    private readonly IProducer<string, string> _producer;

    public OrderProcessingService(IConfiguration configuration, IOrdersRepository ordersRepository, IProducer<string, string> producer)
    {
        _period = TimeSpan.FromSeconds(configuration.GetSection("BackgroundWorkers:OrderProcessingService:Period").Get<int>());
        _ordersRepository = ordersRepository;
        _config = configuration;
        _producer = producer;
        _topic = configuration.GetSection("Kafka:Topic").Get<string>();
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
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
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.GetSection("BackgroundWorkers:OrderProcessingService:Period").Get<string>());
                        using (var accountResponse = await httpClient.GetAsync($"{_config.GetSection("Services:BillingServiceUrl").Get<string>()}/accounts/{order.AccountId}"))
                        {
                            string response = await accountResponse.Content.ReadAsStringAsync();
                            account = JsonConvert.DeserializeObject<Account>(response);
                        }
                    }

                    if (account.Balance >= order.Amount)
                    {
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
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.GetSection("BackgroundWorkers:OrderProcessingService:Period").Get<string>());
                            using (var accountResponse = await httpClient.PostAsync($"{_config.GetSection("Services:BillingServiceUrl").Get<string>()}/transactions", content))
                            {
                                string response = await accountResponse.Content.ReadAsStringAsync();
                                account = JsonConvert.DeserializeObject<Account>(response);
                            }
                        }

                        await _ordersRepository.SetOrderStatus(order.Id, OrderStatus.Paid);
                        Log.Information($"Order {order.Id} is paid.");

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
    }
}
