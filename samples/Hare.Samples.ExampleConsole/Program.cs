using Hare;
using Hare.Extensions;
using Hare.Samples.ExampleConsole;
using Hare.Samples.ExampleConsole.Handles;
using Hare.Samples.ExampleConsole.Messages;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.AddRabbitMQClient("queue");

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource($"{Constants.ACTIVITY_PREFIX}.*"));

builder.Services.AddHare()
    .WithConventionalRouting()
    .WithAutoProvisioning()
    .WithJsonSerializerContext(ExampleJsonContext.Default)
    .AddHareMessage<ExampleMessage, ExampleMessageHandler>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunHareProvisioning(CancellationToken.None);

host.Run();