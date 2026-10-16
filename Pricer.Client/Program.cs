using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pricer.Interfaces;
using Serilog;

namespace Pricer.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ConfigureSerilog();
            const string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PricerStorage;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
            const string invariant = "Microsoft.Data.SqlClient";
            var builder = Host.CreateDefaultBuilder(args).UseSerilog()
                .UseOrleansClient(client =>
                {
                    client.UseAdoNetClustering(options =>
                    {
                        options.Invariant = invariant;
                        options.ConnectionString = connectionString;
                    });
                });
            using var host = builder.Build();
            await host.StartAsync();
            var client = host.Services.GetRequiredService<IClusterClient>();


            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Connecting to cluster...");


            string[] tickers = { "AAPL", "MSFT", "GOOG", "TSLA", "BTC", "ETH", "SOL", "META", "NVDA", "ORCL" };
            Random rnd = new();

            logger.LogInformation($"Found {tickers.Length} tickers to calculate...");

            foreach (var ticker in tickers)
            {
                var calculator = client.GetGrain<IOptionsCalculatorGrain>(ticker);

                double spot = 100.0 + rnd.Next(1, 50);
                double strike = spot + rnd.Next(-5, 5);

                try
                {
                    double result = await calculator.CalculateBlackScholes(spot, strike, 1.0, 0.05, 0.2);
                    logger.LogInformation($"[{ticker}] Price: {result:F4}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error for {ticker}: {ex.Message}");
                }
            }

            logger.LogInformation("\nCalculations complete");
            Console.ReadKey();

            await Log.CloseAndFlushAsync();
            await host.StopAsync();
        }

        private static void ConfigureSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

    }
}