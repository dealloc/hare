using Hare.Extensions;
using Hare.Samples.ExampleConsole;
using Hare.Samples.ExampleConsole.Handles;
using Hare.Samples.ExampleConsole.Messages;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.AddRabbitMQClient("queue");

builder.Services.AddHare(options =>
    {
        options.AutoProvision = true;
        options.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, ExampleJsonContext.Default);
    })
    .AddHareMessage<ExampleMessage, ExampleMessageHandler>(
        configureReceive: options => options.QueueName = "test-queue",
        configureSend: options => options.RoutingKey = "test-queue"
    );


builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunHareProvisioning(CancellationToken.None);

host.Run();