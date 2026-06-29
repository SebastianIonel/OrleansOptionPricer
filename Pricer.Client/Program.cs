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
            Console.WriteLine("=== Pricer Orleans Client ===");
            Console.WriteLine("Connecting to cluster...");
            var logger = ConfigLogger<Program>();
            const string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PricerStorage;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
            const string invariant = "Microsoft.Data.SqlClient";
            var builder = Host.CreateDefaultBuilder(args)
                .UseOrleansClient(client =>
                {
                    client.UseAdoNetClustering(options =>
                    {
                        options.Invariant = invariant;
                        options.ConnectionString = connectionString;
                    });
                })
                .ConfigureLogging(logging => logging.AddConsole());
            using var host = builder.Build();
            await host.StartAsync();
            var client = host.Services.GetRequiredService<IClusterClient>();


            string[] tickers = { "AAPL", "MSFT", "GOOG", "TSLA", "BTC", "ETH", "SOL", "META", "NVDA", "ORCL" };
            Random rnd = new();

            Console.WriteLine($"Found {tickers.Length} tickers to calculate...");

            foreach (var ticker in tickers)
            {
                var calculator = client.GetGrain<IOptionsCalculatorGrain>(ticker);

                double spot = 100.0 + rnd.Next(1, 50);
                double strike = spot + rnd.Next(-5, 5);

                try
                {
                    double result = await calculator.CalculateBlackScholes(spot, strike, 1.0, 0.05, 0.2);
                    Console.WriteLine($"[{ticker}] Price: {result:F4}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error for {ticker}: {ex.Message}");
                }
            }

            Console.WriteLine("\nCalculations complete");
            Console.ReadKey();

            await host.StopAsync();
        }

        private static ILogger<T> ConfigLogger<T>()
        {
            var serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            return new Serilog.Extensions.Logging.SerilogLoggerFactory(serilogLogger)
                .CreateLogger<T>();
        }

    }
}