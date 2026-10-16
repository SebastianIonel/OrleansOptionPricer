using MediatR;
using Microsoft.Extensions.Logging;
using Pricer.Interfaces;
using Serilog.Core;
using System.Threading;

namespace Pricer.Client
{

    public record CalculateOptionsCommand(IClusterClient Client) : MediatR.IRequest;

    public class CalculateOptionsCommandHandler : MediatR.IRequestHandler<CalculateOptionsCommand>
    {
        private readonly ILogger<CalculateOptionsCommandHandler> _logger;

        public CalculateOptionsCommandHandler(ILogger<CalculateOptionsCommandHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(CalculateOptionsCommand command, CancellationToken cancellationToken)
        {
            string[] tickers = { "AAPL", "MSFT", "GOOG", "TSLA", "BTC", "ETH", "SOL", "META", "NVDA", "ORCL" };
            Random rnd = new();

            _logger.LogInformation($"Found {tickers.Length} tickers to calculate...");

            foreach (var ticker in tickers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var calculator = command.Client.GetGrain<IOptionsCalculatorGrain>(ticker);

                double spot = 100.0 + rnd.Next(1, 50);
                double strike = spot + rnd.Next(-5, 5);

                try
                {
                    double result = await calculator.CalculateBlackScholes(spot, strike, 1.0, 0.05, 0.2);
                    _logger.LogInformation($"[{ticker}] Price: {result:F4}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error for {ticker}: {ex.Message}");
                }
            }

            _logger.LogInformation("\nCalculations complete");
            
        }
    }
}
