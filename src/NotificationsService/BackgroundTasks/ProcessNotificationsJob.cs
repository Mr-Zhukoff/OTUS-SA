using Confluent.Kafka;
using CoreLogic.Models;
using Newtonsoft.Json;
using NotificationsService.Data;
using Quartz;
using Serilog;

namespace NotificationsService.BackgroundTasks;


[DisallowConcurrentExecution]
public class ProcessNotificationsJob : IJob
{
    private static int count;
    private readonly string _topic = "order-events";
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IConfiguration _config;
    private readonly string authHeaderKey = "authheader";
    private readonly IConsumer<Ignore, string> _consumer;

    public ProcessNotificationsJob(IConfiguration configuration, INotificationsRepository notificationsRepository)
    {
        _notificationsRepository = notificationsRepository;
        _config = configuration;
        _topic = _config["Kafka:Topic"];

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"],
            GroupId = _config["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }

    public async Task Execute(IJobExecutionContext context)
    {
        
        while (!context.CancellationToken.IsCancellationRequested)
        {
            try
            {
                _consumer.Subscribe(_topic);

                Interlocked.Increment(ref count);

                var consumeResult = _consumer.Consume(context.CancellationToken);
                var message = consumeResult.Message.Value;
                var notification = JsonConvert.DeserializeObject<Notification>(message);
                _notificationsRepository.CreateNotification(notification);
                Log.Information(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing Kafka message");
            }
            finally 
            {
                _consumer.Close();
            }
        }
    }

    private void ProcessKafkaMessage(CancellationToken stoppingToken)
    {

    }
}
