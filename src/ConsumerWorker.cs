using Azure.Messaging.ServiceBus;

namespace Demo;

public class ConsumerWorker : IHostedService, IAsyncDisposable
{
    private readonly ILogger<ConsumerWorker> _logger;
    private readonly ServiceBusProcessor _processor;

    public ConsumerWorker(
        ILogger<ConsumerWorker> logger,
        ServiceBusProcessor processor)
    {
        _logger = logger;
        _processor = processor;

        _processor.ProcessMessageAsync += _processMessageAsync;
        _processor.ProcessErrorAsync += (args) =>
        {
            _logger.LogError(
                args.Exception,
                "[CONSUMER] Error in service bus consumer");

            return Task.CompletedTask;
        };
    }

    private Task _processMessageAsync(ProcessMessageEventArgs args)
    {
        var mesaage = args.Message.Body.ToString() ?? throw new ArgumentException();

        _logger.LogInformation("[CONSUMER] {Message}", mesaage);

        // Just complete the message if no exception was thrown
        return args.CompleteMessageAsync(args.Message);
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[CONSUMER] Starting...");
        await _processor.StartProcessingAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[CONSUMER] Stopping...");
        await _processor.StopProcessingAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_processor is not null)
        {
            await _processor.DisposeAsync().ConfigureAwait(false);
        }
    }
}
