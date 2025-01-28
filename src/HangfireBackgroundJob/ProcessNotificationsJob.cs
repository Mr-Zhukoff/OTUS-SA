using Confluent.Kafka;
using CoreLogic.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Headers;

namespace HangfireService;

public class ProcessNotificationsJob
{
    private static int count;
    private readonly string _authHeaderKey = "authheader";
    private readonly string _notificationsTopic = "notifications";
    private readonly IConfiguration _config;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IProducer<string, string> _producer;
    private readonly AuthenticationHeaderValue _authHeader;
    private readonly IMemoryCache _cache;
    private readonly string _notificationsServiceUrl;
    private readonly string _usersServiceUrl;

    public ProcessNotificationsJob(IConfiguration configuration, IMemoryCache cache, IProducer<string, string> producer)
    {
        _config = configuration;
        _cache = cache;
        _notificationsServiceUrl = Environment.GetEnvironmentVariable("NOTIFICATIONSSVC_URL") ?? _config["Services:NotificationsServiceUrl"];
        _usersServiceUrl = Environment.GetEnvironmentVariable("USERSSVC_URL") ?? _config["Services:UsersServiceUrl"];
        _authHeader = GetAuthHeader().Result; // Добавить кеширование
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_URL") ?? _config["Kafka:BootstrapServers"],
            GroupId = "NotificationsConsumerGroup",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AllowAutoCreateTopics = true
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _producer = producer;
    }

    public async Task Execute()
    {
        Log.Information("ProcessOrdersJob started");
        try
        {
            _consumer.Subscribe(_notificationsTopic);

            while (true)
            {
                var consumeResult = _consumer.Consume();
                if (consumeResult is null)
                    return;

                var newNotification = JsonConvert.DeserializeObject<Notification>(consumeResult.Message.Value);
                Log.Information($"Consumed message {consumeResult.Message.Value}");
                await ProcessNewNotification(newNotification);
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

    private async Task<int> ProcessNewNotification(Notification notification)
    {
        using (var httpClient = new HttpClient())
        {
            JsonContent content = JsonContent.Create(notification);
            httpClient.DefaultRequestHeaders.Authorization = _authHeader;
            using (var transactionResponse = await httpClient.PostAsync($"{_notificationsServiceUrl}/notifications", content))
            {
                string response = await transactionResponse.Content.ReadAsStringAsync();
                if (!transactionResponse.IsSuccessStatusCode)
                {
                    Log.Error($"Create Notification request error!\n\r StatusCode:{transactionResponse.StatusCode}\n\r {response}");
                    return -1;
                }
                var result = JsonConvert.DeserializeObject<Notification>(response);
                return result!.Id;
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
                        Log.Error($"GetAuthHeader request error!\n\r StatusCode:{transactionResponse.StatusCode}\n\r {response}");
                        return null;
                    }
                }
            }
        }
        return authHeader;
    }
}
