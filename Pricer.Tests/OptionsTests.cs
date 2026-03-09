using Pricer.Interfaces;
using Orleans.TestingHost;
using Xunit;

namespace Pricer.Tests;

public class OptionsTests
{
    [Fact]
    public async Task CalculateBlackScholes_ShouldReturnCorrectValue()
    {
        // Arrange: Set up an in-memory test cluster
        var builder = new TestClusterBuilder();
        var cluster = builder.Build();
        await cluster.DeployAsync();

        try
        {
            var grain = cluster.GrainFactory.GetGrain<IOptionsCalculatorGrain>("TEST_TICKER");

            // Act: Calculate price for a standard scenario
            double result = await grain.CalculateBlackScholes(100, 100, 1, 0.05, 0.2);

            // Assert: The price for an At-The-Money option should be positive and roughly ~10.45
            Assert.True(result > 0, "Option price should be positive.");
            Assert.InRange(result, 10.4, 10.5);
        }
        finally
        {
            // Cleanup: Stop the test cluster
            await cluster.StopAllSilosAsync();
        }
    }
}