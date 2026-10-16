using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Orleans.Dashboard;
using Serilog;
using Serilog.Events;
using System.Net;



namespace Pricer.Server
{
    public class Program
    {
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
        public static async Task Main(string[] args)
        {
            ConfigureSerilog();

            const string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PricerStorage;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
            const string invariant = "Microsoft.Data.SqlClient";

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();

            Log.Logger.Information("Starting Server Pricer (Silo)...");
            var siloPortNumber = int.Parse(builder.Configuration["SiloPort"] ?? "11111");
            var dashboardPort = 8080 + (siloPortNumber % 10);
            builder.WebHost.UseUrls($"http://localhost:{dashboardPort}");

            builder.Host.UseOrleans((context, silo) =>
            {
                //silo.UseLocalhostClustering();
                var siloName = context.Configuration["SiloName"] ?? "DefaultSilo";
                var siloPort = int.Parse(context.Configuration["SiloPort"] ?? "11111");
                var gatewayPort = int.Parse(context.Configuration["GatewayPort"] ?? "30000");

                silo.ConfigureEndpoints(IPAddress.Loopback, siloPort: siloPort, gatewayPort: gatewayPort);

                silo.UseAdoNetClustering(options =>
                {
                    options.Invariant = invariant;
                    options.ConnectionString = connectionString;
                });

                silo.AddAdoNetGrainStorage("PricerStorage", options =>
                {
                    options.Invariant = invariant;
                    options.ConnectionString = connectionString;
                });

                silo.AddDashboard();

            });

            var app = builder.Build();
            app.MapOrleansDashboard();
            await app.RunAsync();
        }
    }


}





















