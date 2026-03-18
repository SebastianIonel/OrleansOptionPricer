using Pricer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pricer.Server
{
    public class OptionsCalculatorGrain : Grain, IOptionsCalculatorGrain
    {
        private readonly IPersistentState<OptionState> _state;

        public OptionsCalculatorGrain(
        [PersistentState("OptionState", "PricerStorage")] IPersistentState<OptionState> state)
        {
            _state = state;
        }

        public async Task<double> CalculateBlackScholes(
            double spotPrice,
            double strikePrice,
            double timeToExpiration,
            double riskFreeRate,
            double volatility)
        {
            double d1 = (Math.Log(spotPrice / strikePrice) + (riskFreeRate + Math.Pow(volatility, 2) / 2) * timeToExpiration)
                    / (volatility * Math.Sqrt(timeToExpiration));

            double d2 = d1 - (volatility * Math.Sqrt(timeToExpiration));

            double price = spotPrice * CumulativeNormalDistribution(d1) -
                           strikePrice * Math.Exp(-riskFreeRate * timeToExpiration) * CumulativeNormalDistribution(d2);

            _state.State.LastCalculatedPrice = price;
            _state.State.Timestamp = DateTime.UtcNow;
            _state.State.Name = this.GetPrimaryKeyString();

            await _state.WriteStateAsync();
            Console.WriteLine($"[Silo] Calculated price for {this.GetPrimaryKeyString()}: {price}");

            return price;
        }
        private static double CumulativeNormalDistribution(double x)
        {
            double a1 = 0.319381530;
            double a2 = -0.356563782;
            double a3 = 1.781477937;
            double a4 = -1.821255978;
            double a5 = 1.330274429;
            double L = Math.Abs(x);
            double K = 1.0 / (1.0 + 0.2316419 * L);
            double d = 1.0 / Math.Sqrt(2 * Math.PI) * Math.Exp(-L * L / 2.0);
            double w = 1.0 - d * (a1 * K + a2 * K * K + a3 * Math.Pow(K, 3) + a4 * Math.Pow(K, 4) + a5 * Math.Pow(K, 5));

            return x >= 0 ? w : 1.0 - w;
        }
    }
}