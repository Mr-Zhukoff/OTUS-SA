using Hangfire;
using Hangfire.MemoryStorage;
using HangfireService;
using HangfireService.Data;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

string ordersConnStr = Environment.GetEnvironmentVariable("ORDERS_CONN_STR");
string notificationsConnStr = Environment.GetEnvironmentVariable("NOTIFICATIONS_CONN_STR");
string seqUrl = Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://seq:5341";

builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

builder.Services.AddMemoryCache();

builder.Services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(ordersConnStr));
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();

builder.Services.AddDbContext<NotificationsDbContext>(options => options.UseNpgsql(notificationsConnStr));
builder.Services.AddScoped<INotificationsRepository, NotificationsRepository>();

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
    .Enrich.WithProperty("ApplicationName", Assembly.GetEntryAssembly().GetName().Name)
    .WriteTo.Seq(seqUrl);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHangfireDashboard("/dashboard", new DashboardOptions()
{
    Authorization = new[] { new HangFireAuthorizationFilter() },
    PrefixPath = "/hangfire"
});
RecurringJob.AddOrUpdate(() => Console.WriteLine("Hello world from Hangfire!"), "*/1 * * * *");

RecurringJob.AddOrUpdate<ProcessOrdersJob>(x => x.Execute(), "*/5 * * * *");

//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
    app.UseSwaggerUI();
//}

app.MapHealthChecks("hc", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapGet("/", [AllowAnonymous] () => "HangfireBackgroundJob");

app.Run();
