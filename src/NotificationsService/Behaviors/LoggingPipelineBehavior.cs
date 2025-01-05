using MediatR;

namespace NotificationsService.Behaviors;

public sealed class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    private readonly ILogger _logger;

    public LoggingPipelineBehavior(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        _logger.LogTrace("Processing request {RequestName}", requestName);

        try
        {
            TResponse response = await next();
            _logger.LogInformation("Completed request: {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Completed request { RequestName} with error", requestName);
            throw;
        }
    }
}
