using System;
using System.Collections.Generic;
using System.Text;

namespace Pricer.Interfaces
{
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
