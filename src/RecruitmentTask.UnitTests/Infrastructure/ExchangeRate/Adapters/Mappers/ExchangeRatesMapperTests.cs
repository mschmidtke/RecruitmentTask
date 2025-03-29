using System.Globalization;
using RecruitmentTask.Infrastructure.ExchangeRate.Adapters.Mappers;
using RecruitmentTask.Infrastructure.ExchangeRate.Adapters.Models;

namespace RecruitmentTask.UnitTests.Infrastructure.ExchangeRate.Adapters.Mappers
{
    [TestFixture]
    public class ExchangeRatesMapperTests
    {
        [TestCase("en-US", "3,2223", "5,3212")]
        [TestCase("en-US", "3.2223", "5.3212")]
        [TestCase("pl-PL", "3,2223", "5,3212")]
        [TestCase("pl-PL", "3.2223", "5.3212")]
        public void ToCore_ShouldProperlyParsedRates(string cultureName, string buyCourse,
            string sellCourse)
        {
            // Arrange
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            var expectedBuyCourse = 3.2223M;
            var expectedSellCourse = 5.3212M;
            var exchangeRates = new ExchangeRates
            {
                PublicationDate = DateTime.Now,
                QuotationDate = DateTime.Now,
                Currencies =
                [
                    new Currency
                    {
                        Code = "USD",
                        BuyCourse = buyCourse,
                        SellCourse = sellCourse,
                        Name = "dolar amerykański",
                        Rate = 1
                    }
                ]
            };

            // Act
            var result = exchangeRates.ToCore();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Currencies.First().BuyCourse, Is.EqualTo(expectedBuyCourse));
                Assert.That(result.Currencies.First().SellCourse, Is.EqualTo(expectedSellCourse));
            });
        }
    }
}
