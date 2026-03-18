using System;
using System.Collections.Generic;
using System.Text;

namespace Pricer.Interfaces
{
    [GenerateSerializer]
    public record OptionState
    {
        [Id(0)] public double LastCalculatedPrice { get; set; }
        [Id(1)] public DateTime Timestamp { get; set; }
        [Id(2)] public string Name { get; set; }
    }
    public interface IOptionsCalculatorGrain : IGrainWithStringKey
    {
        Task<double> CalculateBlackScholes(
            double spotPrice,
            double strikePrice,
            double timeToExpiration,
            double riskFreeRate,
            double volatility);
    }
}
