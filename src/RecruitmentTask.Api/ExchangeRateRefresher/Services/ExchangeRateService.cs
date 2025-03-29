using RecruitmentTask.Core.ExchangeRate;
using RecruitmentTask.Infrastructure.ExchangeRate.Adapters;

namespace RecruitmentTask.Api.ExchangeRateRefresher.Services
{
    public interface IExchangeRateService
    {
        Task DownloadLatestExchangeRates();
    }

    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IExchangeRateAdapter _exchangeRateAdapter;
        private readonly IExchangeRatesRepository _exchangeRatesRepository;

        public ExchangeRateService(IExchangeRateAdapter exchangeRateAdapter, IExchangeRatesRepository exchangeRatesRepository)
        {
            _exchangeRateAdapter = exchangeRateAdapter;
            _exchangeRatesRepository = exchangeRatesRepository;
        }

        public async Task DownloadLatestExchangeRates()
        {
            var latestExchangeRates = await _exchangeRateAdapter.GetLatestExchangeRates();

            var rates = await _exchangeRatesRepository.GetExchangeRatesForDate(latestExchangeRates.QuotationDate);

            if (rates == null)
            {
                await _exchangeRatesRepository.SaveExchangeRates(latestExchangeRates);
            }
        }
    }
}
