using System.Diagnostics.CodeAnalysis;
using RecruitmentTask.Api.ExchangeRateRefresher.Services;

namespace RecruitmentTask.Api.ExchangeRateRefresher
{
    [ExcludeFromCodeCoverage]
    public class ExchangeRateUpdater(IExchangeRateService exchangeRateService) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            await DoWork();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                await DoWork();
            }
        }, stoppingToken);

        private Task DoWork()
        {
            return exchangeRateService.DownloadLatestExchangeRates();
        }
    }
}
