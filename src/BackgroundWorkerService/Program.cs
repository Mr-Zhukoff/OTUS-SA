using BackgroundWorkerService;
using BackgroundWorkerService.Data;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Quartz;

string ordersConnStr = Environment.GetEnvironmentVariable("ORDERS_CONN_STR");
string notificationsConnStr = Environment.GetEnvironmentVariable("NOTIFICATIONS_CONN_STR");
string seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");

var builder = Host.CreateApplicationBuilder(args);

var producerConfig = new ProducerConfig
{
    BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_URL"),
    ClientId = builder.Configuration.GetSection("Kafka:ClientId").Get<string>()
};

builder.Services.AddMemoryCache();

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(ordersConnStr));

builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();

builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseNpgsql(notificationsConnStr));

builder.Services.AddScoped<INotificationsRepository, NotificationsRepository>();

builder.Services.AddQuartz(options =>
{
    var ordersJobKey = JobKey.Create(nameof(ProcessOrdersJob));
    options.AddJob<ProcessOrdersJob>(ordersJobKey)
        .AddTrigger(trigger => trigger.ForJob(ordersJobKey)
        .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(30)
        .WithMisfireHandlingInstructionNextWithRemainingCount()
        .RepeatForever()));

    var notificationsJobKey = JobKey.Create(nameof(ProcessNotificationsJob));
    options.AddJob<ProcessNotificationsJob>(notificationsJobKey)
        .AddTrigger(trigger => trigger.ForJob(notificationsJobKey)
        .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(30)
        .WithMisfireHandlingInstructionNextWithRemainingCount()
        .RepeatForever()));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddSingleton(new ProducerBuilder<string, string>(producerConfig).Build());

var host = builder.Build();
host.Run();
