using RecruitmentTask.Api.Wallet;
using RecruitmentTask.Core.ExchangeRate.Model;
using RecruitmentTask.Core.Wallets;

namespace RecruitmentTask.UnitTests.Api.Wallet;

[TestFixture]
public class WalletMapperTests
{
    private readonly Currency _usdRate = new Currency("dolar amerykański", 1, "USD", 3.8385M, 3.9161M);
    private readonly Currency _hufRate = new Currency("forint (Węgry)", 100, "HUF", 1.0416M, 1.0626M);
    private readonly Currency _chfRate = new Currency("frank szwajcarski", 1, "CHF", 4.3513M, 4.4393M);

    [Test]
    public void ToContract_ShouldReturnProperlyMappedObject()
    {
        // Arrange
        var objectToMapped = new List<RecruitmentTask.Core.Wallets.Wallet>
        {
            RecruitmentTask.Core.Wallets.Wallet.RecoverFrom(name: "test",
                balance: new List<Balance> { Balance.Create(_usdRate.Code, amount: 10.0M), Balance.Create(_hufRate.Code, amount:15.0M) }),
            RecruitmentTask.Core.Wallets.Wallet.RecoverFrom(name: "test2", balance: new List<Balance>
                { Balance.Create(_chfRate.Code, amount: 7.0M), Balance.Create(_usdRate.Code, amount: 18.41M) })
        };
        var rates = new ExchangeRates(quotationDate: DateTime.Now, publicationDate: DateTime.Now,
            currencies: new List<Currency>
            {
                _usdRate,
                _hufRate,
                _chfRate
            });

        // Act
        var result = objectToMapped.ToContract(rates);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Wallets.Count(), Is.EqualTo(2));
            Assert.That(result.Wallets.First().Name, Is.EqualTo("test"));
            Assert.That(result.Wallets.First().Balances.First().Amount, Is.EqualTo(10.0M));
            Assert.That(result.Wallets.First().Balances.First().CurrencyCode, Is.EqualTo(_usdRate.Code));
            Assert.That(result.Wallets.First().Balances.First().CurrencyName, Is.EqualTo(_usdRate.Name));
            Assert.That(result.Wallets.First().Balances.Last().Amount, Is.EqualTo(15.0M));
            Assert.That(result.Wallets.First().Balances.Last().CurrencyCode, Is.EqualTo(_hufRate.Code));
            Assert.That(result.Wallets.First().Balances.Last().CurrencyName, Is.EqualTo(_hufRate.Name));
            Assert.That(result.Wallets.Last().Name, Is.EqualTo("test2"));
            Assert.That(result.Wallets.Last().Balances.First().Amount, Is.EqualTo(7.0M));
            Assert.That(result.Wallets.Last().Balances.First().CurrencyCode, Is.EqualTo(_chfRate.Code));
            Assert.That(result.Wallets.Last().Balances.First().CurrencyName, Is.EqualTo(_chfRate.Name));
            Assert.That(result.Wallets.Last().Balances.Last().Amount, Is.EqualTo(18.41M));
            Assert.That(result.Wallets.Last().Balances.Last().CurrencyCode, Is.EqualTo(_usdRate.Code));
            Assert.That(result.Wallets.Last().Balances.Last().CurrencyName, Is.EqualTo(_usdRate.Name));
        });
    }
}