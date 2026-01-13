using Hare.Extensions;
using Hare.Samples.ExampleConsole;
using Hare.Samples.ExampleConsole.Messages;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.AddRabbitMQClient("queue");

builder.Services.AddHare()
    .AddHareMessage<ExampleMessage>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();