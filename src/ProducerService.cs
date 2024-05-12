using Azure.Messaging.ServiceBus;

namespace Demo;

public class ProducerService(
    ILogger<ProducerService> logger,
    ServiceBusSender sender)
{
    private readonly ILogger<ProducerService> _logger = logger;
    private readonly ServiceBusSender _sender = sender;

    public async Task SendMessageAsync(MessageRequest request)
    {
        await _sender.SendMessageAsync(new ServiceBusMessage(request.Body));
        _logger.LogInformation("[PRODUCER][SENT]: {Message}", request.Body);
    }
}