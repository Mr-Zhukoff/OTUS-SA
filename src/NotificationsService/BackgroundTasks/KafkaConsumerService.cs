
using Confluent.Kafka;
using Serilog;

namespace NotificationsService.Workers;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly string topic = "order-events";
    private static int count;

    public KafkaConsumerService(IConfiguration configuration)
    {
        topic = configuration.GetSection("Kafka:Topic").Get<string>();

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration.GetSection("Kafka:BootstrapServers").Get<string>(),
            GroupId = configuration.GetSection("Kafka:GroupId").Get<string>(),
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        _consumer.Subscribe(topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            Interlocked.Increment(ref count);

            await Task.Run(() => ProcessKafkaMessage(stoppingToken), stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }

        _consumer.Close();
    }

    public void ProcessKafkaMessage(CancellationToken stoppingToken)
    {
        try
        {
            var consumeResult = _consumer.Consume(stoppingToken);
            var message = consumeResult.Message.Value;
            Log.Information(message);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing Kafka message");
        }
    }
}
