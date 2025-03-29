using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using RecruitmentTask.Core.ExchangeRate;
using RecruitmentTask.Core.ExchangeRate.Model;
using RecruitmentTask.Infrastructure.Configuration;
using RecruitmentTask.Infrastructure.ExchangeRate.Exceptions;

namespace RecruitmentTask.Infrastructure.ExchangeRate.Repositories
{
    public class ExchangeRatesRepository : IExchangeRatesRepository
    {
        private const short DaysLimit = 30;
        public async Task SaveExchangeRates(ExchangeRates exchangeRates)
        {
            const string sql =
                "INSERT INTO dbo.ExchangesRates (QuotationDate,PublicationDate,Rates) VALUES (@quotationDate, @publicationDate, @rates)";

            using var sqlConnection = new SqlConnection(DbConfig.DbConnection);

            var ratesJson = JsonConvert.SerializeObject(exchangeRates.Currencies);

            await sqlConnection.ExecuteAsync(sql,
                new
                {
                    quotationDate = exchangeRates.QuotationDate, publicationDate = exchangeRates.PublicationDate,
                    rates = ratesJson
                });
        }

        public async Task<ExchangeRates> GetMostRecentExchangeRates()
        {
            const string sql =
                "SELECT QuotationDate,PublicationDate,Rates FROM dbo.ExchangesRates WHERE QuotationDate = @date";

            using var sqlConnection = new SqlConnection(DbConfig.DbConnection);

            sbyte backDays = 0;

            dynamic? result;

            do
            {
                result = await sqlConnection.QuerySingleOrDefaultAsync(sql, new { date = DateTime.UtcNow.Date.AddDays(backDays) });
                backDays--;

                if (Math.Abs(backDays) > DaysLimit)
                {
                    throw new TooOldRatesException(DaysLimit);
                }

            } while (result == null);

            return new ExchangeRates(result.QuotationDate, result.PublicationDate,
                JsonConvert.DeserializeObject<IEnumerable<Currency>>(result.Rates));

        }

        public async Task<ExchangeRates?> GetExchangeRatesForDate(DateTime quotationDate)
        {
            const string sql =
                "SELECT QuotationDate,PublicationDate,Rates FROM dbo.ExchangesRates WHERE QuotationDate = @quotationDate";

            using var sqlConnection = new SqlConnection(DbConfig.DbConnection);
            
            var result = await sqlConnection.QuerySingleOrDefaultAsync(sql, new { quotationDate = quotationDate.Date});

            if (result == null)
            {
                return null;
            }

            return new ExchangeRates(result.QuotationDate, result.PublicationDate,
                    JsonConvert.DeserializeObject<IEnumerable<Currency>>(result.Rates));
        }
    }
}
