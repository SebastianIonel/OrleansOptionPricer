using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using Orleans.Dashboard;

Console.WriteLine("Starting Server Pricer (Silo)...");

const string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PricerStorage;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
const string invariant = "Microsoft.Data.SqlClient";

var builder = WebApplication.CreateBuilder(args);
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

        silo.ConfigureLogging(logging => logging.AddConsole());
    });

var app = builder.Build();
app.MapOrleansDashboard();
await app.RunAsync();