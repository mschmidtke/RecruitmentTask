using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Xml.Serialization;
using RecruitmentTask.Infrastructure.ExchangeRate.Adapters.Mappers;
using RecruitmentTask.Infrastructure.ExchangeRate.Adapters.Models;

namespace RecruitmentTask.Infrastructure.ExchangeRate.Adapters
{
    public interface IExchangeRateAdapter
    {
        Task<Core.ExchangeRate.Model.ExchangeRates> GetLatestExchangeRates();
    }

    [ExcludeFromCodeCoverage]
    public class ExchangeRateAdapter : IExchangeRateAdapter
    {
        private const string Url = "https://static.nbp.pl";

        public async Task<Core.ExchangeRate.Model.ExchangeRates> GetLatestExchangeRates()
        {
            using var httpClient = new HttpClient();

            Uri.TryCreate($"{Url}/dane/kursy/xml/LastC.xml", UriKind.Absolute, out var baseUrl);

            try
            {
                var response = await httpClient.GetAsync(baseUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();

                    var serializer = new XmlSerializer(typeof(ExchangeRates));

                    var exchangesRates = ((ExchangeRates)serializer.Deserialize(stream)).ToCore();


                    return exchangesRates;
                }
                else
                {
                    // log "Unable to download fresh exchange rates
                    // return empty.list or exception
                    return null;
                }
            }
            catch (Exception ex)
            {
                // log "Unable to download fresh exchange rates
                // return empty.list or exception
                return null;
            }
        }
    }
}
