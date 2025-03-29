namespace RecruitmentTask.Core.ExchangeRate.Model
{
    public class ExchangeRates
    {
        public ExchangeRates(DateTime quotationDate, DateTime publicationDate, IEnumerable<Currency> currencies)
        {
            QuotationDate = quotationDate;
            PublicationDate = publicationDate;
            Currencies = currencies;
        }

        public DateTime QuotationDate { get; }

        public DateTime PublicationDate { get; }

        public IEnumerable<Currency> Currencies { get; }
    }
}
