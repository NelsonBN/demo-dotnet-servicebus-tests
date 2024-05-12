using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Demo.Tests;

public class ConsumerWorkerTests
{
    private readonly ILogger<ConsumerWorker> _logger = Substitute.For<ILogger<ConsumerWorker>>();
    private readonly ServiceBusProcessor _processor;


    public ConsumerWorkerTests()
    {
        var client = new ServiceBusClient("Endpoint=sb://localhost/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=1");
        _processor = client.CreateProcessor("fake-queue");

        _ = new ConsumerWorker(
            _logger,
            _processor);
    }


    [Fact]
    public async Task Should_log_info_when_process_successfully_received_message()
    {
        // Arrange
        var message = "this is my fake message";
        var args = message.ToProcessMessageEventArgs();


        // Act
        await _processor.TriggerProcessMessage(args);


        // Assert
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString() == $"[CONSUMER] {message}"),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }


    [Fact]
    public async Task Should_log_error_when_received_message_fails()
    {
        // Arrange
        var args = "something".ToProcessMessageEventArgs();

        var exception = new Exception("Something went wrong");

        _logger.When(c => c.Log(
            Arg.Is<LogLevel>(l => l == LogLevel.Information),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>()))
        .Do(call => throw exception);


        // Act
        await _processor.TriggerProcessMessage(args);


        // Assert
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString() == "[CONSUMER] Error in service bus consumer"),
            Arg.Is(exception),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
