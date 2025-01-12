using FluentValidation;
using NotificationsService.BackgroundTasks;
using NotificationsService.Behaviors;
using Quartz;

namespace NotificationsService;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddQuartz(options =>
        {
            var jobKey = JobKey.Create(nameof(ProcessNotificationsJob));
            options.AddJob<ProcessNotificationsJob>(jobKey)
                .AddTrigger(trigger => trigger.ForJob(jobKey)
                .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(30)
                .WithMisfireHandlingInstructionNextWithRemainingCount()
                .RepeatForever()));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
