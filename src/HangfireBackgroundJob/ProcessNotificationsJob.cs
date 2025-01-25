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

    public ProcessNotificationsJob(IConfiguration configuration, INotificationsRepository notificationsRepository)
    {
        _notificationsRepository = notificationsRepository;
        _config = configuration;
    }

    public async Task Execute()
    {
        Log.Information("ProcessNotificationsJob started");

        Log.Information("ProcessNotificationsJob ended");
    }
}
