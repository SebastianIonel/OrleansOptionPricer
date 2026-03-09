using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pricer.Interfaces;

Console.WriteLine("=== Pricer Orleans Client ===");
Console.WriteLine("Connecting to cluster...");

var builder = Host.CreateDefaultBuilder(args)
    .UseOrleansClient(client =>
    {
        client.UseLocalhostClustering();
    })
    .ConfigureLogging(logging => logging.AddConsole());

using var host = builder.Build();
await host.StartAsync();

var client = host.Services.GetRequiredService<IClusterClient>();

var calculator = client.GetGrain<IOptionsCalculatorGrain>("AAPL");

try
{
    double spot = 100.0;     // Spot Price
    double strike = 105.0;   // Strike Price
    double time = 1.0;       // Time to Expiry (1 year)
    double rate = 0.05;      // Risk-free rate (5%)
    double vol = 0.2;        // Volatility (20%)

    Console.WriteLine($"\nRequesting calculation for Spot: {spot}, Strike: {strike}...");

    double result = await calculator.CalculateBlackScholes(spot, strike, time, rate, vol);

    Console.WriteLine("-------------------------------------");
    Console.WriteLine($"RESULT (Call Option Price): {result:F4}");
    Console.WriteLine("-------------------------------------\n");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred during calculation: {ex.Message}");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

await host.StopAsync();