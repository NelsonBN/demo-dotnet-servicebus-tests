using Azure.Messaging.ServiceBus;
using Demo;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddSingleton(sp =>
        new ServiceBusClient(sp.GetRequiredService<IConfiguration>().GetConnectionString("SeriveBus")))
    .AddSingleton(sp =>
        sp.GetRequiredService<ServiceBusClient>().CreateSender(
            sp.GetRequiredService<IConfiguration>().GetValue<string>("Queue")))
    .AddSingleton(sp =>
        sp.GetRequiredService<ServiceBusClient>().CreateProcessor(
            sp.GetRequiredService<IConfiguration>().GetValue<string>("Queue"),
            new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false,
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(1),
                PrefetchCount = 1
            }))
    .AddTransient<ProducerService>()
    .AddHostedService<ConsumerWorker>();


var app = builder.Build();

app.MapEndpoints();

app.Run();
