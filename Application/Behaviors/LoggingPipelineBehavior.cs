using Mediator;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Behaviors;

public sealed class LoggingPipelineBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
{
    private readonly ILogger<LoggingPipelineBehavior<TMessage, TResponse>> _logger;

    public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TMessage, TResponse>> logger)
    {
        _logger = logger;
    }
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TMessage).Name;
        var timer = Stopwatch.StartNew();

        _logger.LogInformation(
            "[START] Processing request {RequestName}",
            requestName);

        try
        {
            var response = await next(message, cancellationToken);

            timer.Stop();

            _logger.LogInformation(
                "[END] Processed request {RequestName} in {ElapsedMilliseconds} ms",
                requestName,
                timer.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();

            _logger.LogError(
                ex,
                "[ERROR] Request {RequestName} failed after {ElapsedMilliseconds} ms",
                requestName,
                timer.ElapsedMilliseconds);

            throw;
        }
    }
}