using Pricer.Interfaces;
using Orleans.TestingHost;
using Orleans.Hosting;
using Xunit;

namespace Pricer.Tests;

public class TestSiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.AddMemoryGrainStorage("PricerStorage");
    }
}

public class OptionsTests
{
    [Fact]
    public async Task CalculateBlackScholes_ShouldReturnCorrectValue()
    {

        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();
        var cluster = builder.Build();
        await cluster.DeployAsync();

        try
        {
            var grain = cluster.GrainFactory.GetGrain<IOptionsCalculatorGrain>("TEST_TICKER");

            double result = await grain.CalculateBlackScholes(100, 100, 1, 0.05, 0.2);

            Assert.True(result > 0, "Option price should be positive.");
            Assert.InRange(result, 10.4, 10.5);
        }
        finally
        {
            await cluster.StopAllSilosAsync();
        }
    }
}