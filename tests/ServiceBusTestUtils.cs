using Azure.Messaging.ServiceBus;
using NSubstitute;
using System.Reflection;

namespace Demo.Tests;

public static class ServiceBusTestUtils
{
    public static async Task TriggerProcessMessage(this ServiceBusProcessor processor, ProcessMessageEventArgs args)
    {
        const string eventName = "ProcessMessageAsync";

        var eventInfo = processor.GetType().GetEvent(eventName);
        if (eventInfo is null)
        {
            throw new InvalidOperationException($"Cannot find event {eventName}");
        }

        var raiseMethod = eventInfo.DeclaringType?.GetMethod(
            $"On{eventName}",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (raiseMethod is null)
        {
            throw new InvalidOperationException($"Cannot find raise method for event {eventName}");
        }

        try
        {
            await (Task)raiseMethod.Invoke(processor, [args])!;
        }
        catch (Exception exception)
        {
            await processor.TriggerProcessError(exception.ToProcessErrorEventArgs());
        }
    }

    public static async Task TriggerProcessError(this ServiceBusProcessor processor, ProcessErrorEventArgs args)
    {
        const string eventName = "ProcessErrorAsync";

        var eventInfo = processor.GetType().GetEvent(eventName);
        if (eventInfo is null)
        {
            throw new InvalidOperationException($"Cannot find event {eventName}");
        }

        var raiseMethod = eventInfo.DeclaringType?.GetMethod(
            $"On{eventName}",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (raiseMethod is null)
        {
            throw new InvalidOperationException($"Cannot find raise method for event {eventName}");
        }

        await (Task)raiseMethod.Invoke(processor, [args])!;
    }

    public static ProcessMessageEventArgs ToProcessMessageEventArgs(this string message)
    {
        return new ProcessMessageEventArgs(
            ServiceBusModelFactory.ServiceBusReceivedMessage(new BinaryData(message)),
            Substitute.For<ServiceBusReceiver>(),
            "",
            CancellationToken.None);
    }

    public static ProcessErrorEventArgs ToProcessErrorEventArgs(this Exception exception)
    {
        return new ProcessErrorEventArgs(
            exception,
            ServiceBusErrorSource.ProcessMessageCallback,
            "",
            "",
            CancellationToken.None);
    }
}