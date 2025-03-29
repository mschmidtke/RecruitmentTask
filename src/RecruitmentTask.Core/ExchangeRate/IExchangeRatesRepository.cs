namespace RecruitmentTask.Core.ExchangeRate;

public interface IExchangeRatesRepository
{
    Task SaveExchangeRates(Model.ExchangeRates exchangeRates);
    Task<Model.ExchangeRates> GetMostRecentExchangeRates();
    Task<Model.ExchangeRates?> GetExchangeRatesForDate(DateTime quotationDate);
}