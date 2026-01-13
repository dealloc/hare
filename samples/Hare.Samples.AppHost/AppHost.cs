var builder = DistributedApplication.CreateBuilder(args);

var queue = builder.AddLavinMQ("queue");

builder.AddProject<Projects.Hare_Samples_ExampleConsole>("exampleconsole")
    .WithReference(queue);

builder.Build().Run();