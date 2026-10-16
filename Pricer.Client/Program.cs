using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pricer.Interfaces;
using Serilog;
using Serilog.Events;

namespace Pricer.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ConfigureSerilog();
            const string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PricerStorage;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
            const string invariant = "Microsoft.Data.SqlClient";
            var builder = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Program).Assembly));
                })
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

            var mediator = host.Services.GetRequiredService<IMediator>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Connecting to cluster...");


            await mediator.Send(new CalculateOptionsCommand(client));

            Console.ReadKey();

            await Log.CloseAndFlushAsync();
            await host.StopAsync();
        }

        private static void ConfigureSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()  
                .MinimumLevel.Override("Pricer.Server", LogEventLevel.Verbose)
                .MinimumLevel.Override("Orleans", LogEventLevel.Warning)  
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

    }
}