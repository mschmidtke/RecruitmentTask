using System.Globalization;
using RecruitmentTask.Core.ExchangeRate.Model;

namespace RecruitmentTask.Infrastructure.ExchangeRate.Adapters.Mappers
{
    public static class ExchangeRatesMapper
    {
        private const string Comma = ",";
        private const string Dot = ".";


        public static ExchangeRates ToCore(this Models.ExchangeRates exchangeRates)
        {
            return new ExchangeRates(exchangeRates.QuotationDate, exchangeRates.PublicationDate,
                exchangeRates.Currencies.Select(ToCore));
        }

        private static Currency ToCore(this Models.Currency currency)
        {
            string fromCurrencyDecimalSeparator;
            string toCurrencyDecimalSeparator;

            if (CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator.Equals(Comma))
            {
                fromCurrencyDecimalSeparator = Dot;
                toCurrencyDecimalSeparator = Comma;
            }
            else
            {
                fromCurrencyDecimalSeparator = Comma;
                toCurrencyDecimalSeparator = Dot;
            }

            return new Currency(currency.Name, currency.Rate, currency.Code,
                decimal.Parse(currency.BuyCourse.Replace(fromCurrencyDecimalSeparator, toCurrencyDecimalSeparator)),
                decimal.Parse(currency.SellCourse.Replace(fromCurrencyDecimalSeparator, toCurrencyDecimalSeparator)));
        }
    }
}
