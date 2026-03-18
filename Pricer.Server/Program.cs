using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.WriteLine("Starting Server Pricer (Silo)...");

const string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PricerStorage;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
const string invariant = "Microsoft.Data.SqlClient"; 

var builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering();

        silo.AddAdoNetGrainStorage("PricerStorage", options =>
        {
            options.Invariant = invariant;
            options.ConnectionString = connectionString;
        });

        silo.ConfigureLogging(logging => logging.AddConsole());
    });

using var host = builder.Build();
await host.RunAsync();