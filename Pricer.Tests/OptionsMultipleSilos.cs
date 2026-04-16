using System;
using System.Threading.Tasks;
using Xunit;
using Orleans.TestingHost;
using Orleans.Hosting;
using Microsoft.Extensions.Configuration;
using Pricer.Interfaces;

namespace Pricer.Tests
{
    public class SiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            const string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PricerStorage;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
            const string invariant = "Microsoft.Data.SqlClient";

            siloBuilder.AddAdoNetGrainStorage("PricerStorage", options =>
            {
                options.Invariant = invariant;
                options.ConnectionString = connectionString;
            });
        }
    }

    public class OptionsMultipleSilos : IAsyncLifetime
    {
        private readonly TestCluster _cluster;

        public OptionsMultipleSilos()
        {
            var builder = new TestClusterBuilder(initialSilosCount: 2);
            builder.AddSiloBuilderConfigurator<SiloConfigurator>();
            _cluster = builder.Build();
        }

        public async Task InitializeAsync()
        {
            await _cluster.DeployAsync();
        }

        public async Task DisposeAsync()
        {
            await _cluster.StopAllSilosAsync();
            await _cluster.DisposeAsync();
        }

        [Fact]
        public async Task AreDistributiveSilosRunningAndCalculatingCorrectly()
        {
            // Assert the cluster client is functioning and multiple silos are deployed
            Assert.NotNull(_cluster.Client);

            // Arrange calculations
            var calculator = _cluster.Client.GetGrain<IOptionsCalculatorGrain>("Option1");
            double spotPrice = 100.0;
            double strikePrice = 100.0;
            double timeToExpiration = 1.0;
            double riskFreeRate = 0.05;
            double volatility = 0.2;

            // Act
            double result = await calculator.CalculateBlackScholes(spotPrice, strikePrice, timeToExpiration, riskFreeRate, volatility);

            // Assert
            // The Black-Scholes formula for a european call option with these inputs results in approximately 10.4506
            Assert.True(result > 0.0);
            Assert.InRange(result, 10.45, 10.46);
        }
    }
}
