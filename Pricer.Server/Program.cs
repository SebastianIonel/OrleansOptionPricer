using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.WriteLine("Starting Server Pricer (Silo)...");

var builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering();

        silo.ConfigureLogging(logging => logging.AddConsole());
    });

using var host = builder.Build();
await host.RunAsync();