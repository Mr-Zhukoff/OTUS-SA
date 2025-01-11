using FluentValidation;
using OrdersService.BackgroundTasks;
using OrdersService.Behaviors;
using Quartz;

namespace OrdersService;

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
            var jobKey = JobKey.Create(nameof(ProcessOrdersJob));
            options.AddJob<ProcessOrdersJob>(jobKey)
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
