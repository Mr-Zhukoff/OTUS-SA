using Confluent.Kafka;
using CoreLogic.Models;
using HangfireService.Data;
using Newtonsoft.Json;
using Serilog;

namespace HangfireService;

public class ProcessNotificationsJob
{
    private static int count;
    private readonly string _topic = "order-events";
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IConfiguration _config;
    private readonly ConsumerConfig _consumerConfig;

    public ProcessNotificationsJob(IConfiguration configuration, INotificationsRepository notificationsRepository)
    {
        _notificationsRepository = notificationsRepository;
        _config = configuration;
        _topic = _config["Kafka:Topic"];
        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"],
            GroupId = _config["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
    }

    public async Task Execute()
    {
        Log.Information("ProcessNotificationsJob started");
        using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
        {
            try
            {
                //consumer.Subscribe(_topic);
                //var cr = consumer.Consume(context.CancellationToken);
                //var message = cr.Message.Value;
                //var notification = JsonConvert.DeserializeObject<Notification>(message);
                //_notificationsRepository.CreateNotification(notification);
                //Log.Information($"Consumed event from topic {_topic}: key = {cr.Message.Key,-10} value = {cr.Message.Value}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing Kafka message");
            }
            finally
            {
                consumer.Close();
            }
        }
        Log.Information("ProcessNotificationsJob ended");
    }
}
