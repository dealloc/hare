using Hare.Extensions;
using Hare.Samples.ExampleConsole;
using Hare.Samples.ExampleConsole.Handles;
using Hare.Samples.ExampleConsole.Messages;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.AddRabbitMQClient("queue");

builder.Services.AddHare(options =>
    {
        options.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, ExampleJsonContext.Default);
    })
    .AddHareMessage<ExampleMessage, ExampleMessageHandler>();


builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();