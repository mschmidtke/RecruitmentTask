using NSubstitute;
using RecruitmentTask.Core;
using RecruitmentTask.Core.ExchangeRate;
using RecruitmentTask.Core.ExchangeRate.Model;
using RecruitmentTask.Core.Validations;
using RecruitmentTask.Core.Wallets;

namespace RecruitmentTask.UnitTests.Core.Wallets;

[TestFixture]
public class WalletTests
{
    private IExchangeRatesRepository _exchangeRatesRepository;

    private readonly Currency _usdRate = new Currency(name: "dolar amerykański", rate: 1, code: "USD", buyCourse: 3.8385M, sellCourse: 3.9161M);
    private readonly Currency _hufRate = new Currency(name: "forint (Węgry)", 100, code: "HUF", buyCourse: 1.0416M, sellCourse: 1.0626M);
    private readonly Currency _chfRate = new Currency(name: "frank szwajcarski", 1, code: "CHF", buyCourse: 4.3513M, sellCourse: 4.4393M);

    private Wallet _sut;

    [SetUp]
    public void Setup()
    {
        _exchangeRatesRepository = Substitute.For<IExchangeRatesRepository>();

        _exchangeRatesRepository.GetMostRecentExchangeRates().Returns(new ExchangeRates(quotationDate: DateTime.Now, publicationDate: DateTime.Now,
           currencies: new List<Currency>
            {
                _usdRate,
                _hufRate,
                _chfRate
            }));

        _sut = Wallet.Create(name: "test");
    }

    [Test]
    public async Task Deposit_ShouldReturn_ErrorForNotSupportedCurrency()
    {
        // Act
        var operationResult = await _sut.Deposit(currencyCode:"EUR", amount: 10, _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.False);
            Assert.That(operationResult.Errors, Is.Not.Empty);
            Assert.That(operationResult.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.NotSupportedCurrency));
            Assert.That(operationResult.Errors.First().PropertyName, Is.EqualTo("currencyCode"));
        });
    }

    [Test]
    public async Task Deposit_ShouldReturn_ErrorForWrongAmount()
    {
        // Act
        var operationResult = await _sut.Deposit(currencyCode: "USD", amount: -1, _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.False);
            Assert.That(operationResult.Errors, Is.Not.Empty);
            Assert.That(operationResult.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.InvalidAmount));
            Assert.That(operationResult.Errors.First().PropertyName, Is.EqualTo("amount"));
        });
    }

    [Test]
    public async Task Deposit_ShouldReturn_SuccessAndProperAmountForSingleDeposit()
    {
        // Arrange
        const decimal expectedAmount = 10.0M;

        // Act
        var operationResult = await _sut.Deposit(currencyCode: "USD", expectedAmount, _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.True);
            Assert.That(operationResult.Errors, Is.Empty);
            Assert.That(_sut.Balances.Count, Is.EqualTo(1));
            Assert.That(_sut.Balances[0].Amount, Is.EqualTo(expectedAmount));
            Assert.That(_sut.Balances[0].CurrencyCode, Is.EqualTo("USD"));
        });
    }

    [Test]
    public async Task Deposit_ShouldReturn_SuccessAndProperAmountForMultipleDeposits()
    {
        // Arrange
        const decimal firstDeposit = 10.0M;
        const decimal secondDeposit = 5.0M;

        // Act
        var operationResults = new List<OperationResult>
        {
            await _sut.Deposit(currencyCode: "USD", firstDeposit, _exchangeRatesRepository),
            await _sut.Deposit(currencyCode: "USD", secondDeposit, _exchangeRatesRepository)
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResults.Select(operation => operation.IsSuccess), Is.All.True);
            Assert.That(operationResults.Select(operation => operation.Errors), Is.All.Empty);
            Assert.That(_sut.Balances.Count, Is.EqualTo(1));
            Assert.That(_sut.Balances[0].Amount, Is.EqualTo(firstDeposit + secondDeposit));
            Assert.That(_sut.Balances[0].CurrencyCode, Is.EqualTo("USD"));
        });
    }

    [Test]
    public async Task Withdrawal_ShouldReturn_ErrorForNotSupportedCurrency()
    {
        // Act
        var operationResult = await _sut.Withdrawal(currencyCode: "EUR", amount: 10, _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.False);
            Assert.That(operationResult.Errors, Is.Not.Empty);
            Assert.That(operationResult.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.NotSupportedCurrency));
            Assert.That(operationResult.Errors.First().PropertyName, Is.EqualTo("currencyCode"));
        });
    }

    [Test]
    public async Task Withdrawal_ShouldReturn_ErrorForWrongAmount()
    {
        // Act
        var operationResult = await _sut.Withdrawal(currencyCode: "USD", amount: -1, _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.False);
            Assert.That(operationResult.Errors, Is.Not.Empty);
            Assert.That(operationResult.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.InvalidAmount));
            Assert.That(operationResult.Errors.First().PropertyName, Is.EqualTo("amount"));
        });
    }

    [Test]
    public async Task Withdrawal_ShouldReturn_ErrorForNotExistingBalanceInWallet()
    {
        // Act
        var operationResult = await _sut.Withdrawal(currencyCode: "USD", amount: 10.0M, _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.False);
            Assert.That(operationResult.Errors, Is.Not.Empty);
            Assert.That(operationResult.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.AccountInCurrencyDoesNotExist));
            Assert.That(operationResult.Errors.First().PropertyName, Is.Null);
        });
    }

    [Test]
    public async Task Withdrawal_ShouldReturn_ErrorForWithdrawalThatExceededTheBalance()
    {
        // Arrange
        _sut = Wallet.RecoverFrom(name: "test",
            balance: new List<Balance> { Balance.Create(currencyCode: "USD", amount: 10.0M) });

        // Act
        var operationResult = await _sut.Withdrawal(currencyCode: "USD", amount: 12.0M, _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.False);
            Assert.That(operationResult.Errors, Is.Not.Empty);
            Assert.That(operationResult.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.NotEnoughBalance));
            Assert.That(operationResult.Errors.First().PropertyName, Is.EqualTo("amount"));
        });
    }

    [Test]
    public async Task Withdrawal_ShouldReturn_SuccessAndProperAmountForSingleWithdrawal()
    {
        // Arrange
        const decimal initialBalance = 10.0M;
        const decimal withdrawal = 8.0M;

        _sut = Wallet.RecoverFrom(name: "test",
            balance: new List<Balance> { Balance.Create(currencyCode: "USD", initialBalance) });
        
        // Act
        var operationResult = await _sut.Withdrawal(currencyCode: "USD", withdrawal, _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.True);
            Assert.That(operationResult.Errors, Is.Empty);
            Assert.That(_sut.Balances.Count, Is.EqualTo(1));
            Assert.That(_sut.Balances[0].Amount, Is.EqualTo(initialBalance - withdrawal));
            Assert.That(_sut.Balances[0].CurrencyCode, Is.EqualTo("USD"));
        });
    }

    [Test]
    public async Task Withdrawal_ShouldReturn_SuccessAndProperAmountForMultipleWithdrawal()
    {
        // Arrange
        const decimal initialBalance = 10.0M;
        const decimal firstWithdrawal = 4.0M;
        const decimal secondWithdrawal = 2.51M;

        _sut = Wallet.RecoverFrom(name: "test",
            balance: new List<Balance> { Balance.Create(currencyCode: "USD", initialBalance) });

        // Act
        var operationResults = new List<OperationResult>
        {
            await _sut.Withdrawal(currencyCode: "USD", firstWithdrawal, _exchangeRatesRepository),
            await _sut.Withdrawal(currencyCode: "USD", secondWithdrawal, _exchangeRatesRepository)
        };

        // Assert
        Assert.That(operationResults.Select(operation => operation.IsSuccess), Is.All.True);
        Assert.That(operationResults.Select(operation => operation.Errors), Is.All.Empty);
        Assert.That(_sut.Balances.Count, Is.EqualTo(1));
        Assert.That(_sut.Balances[0].Amount, Is.EqualTo(initialBalance-(firstWithdrawal + secondWithdrawal)));
        Assert.That(_sut.Balances[0].CurrencyCode, Is.EqualTo("USD"));
    }


    [Test]
    public async Task Convert_ShouldReturn_ErrorForNotSupportedCurrencyInFrom()
    {
        // Act
        var operationResult = await _sut.Convert(fromCurrencyCode: "EUR", toCurrencyCode: "USD", amount: 10,
            _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.False);
            Assert.That(operationResult.Errors, Is.Not.Empty);
            Assert.That(operationResult.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.NotSupportedCurrency));
            Assert.That(operationResult.Errors.First().PropertyName, Is.EqualTo("fromCurrencyCode"));
        });
    }

    [Test]
    public async Task Convert_ShouldReturn_ErrorForNotSupportedCurrencyInTo()
    {
        // Act
        var operationResult = await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "EUR", amount: 10,
            _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.False);
            Assert.That(operationResult.Errors, Is.Not.Empty);
            Assert.That(operationResult.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.NotSupportedCurrency));
            Assert.That(operationResult.Errors.First().PropertyName, Is.EqualTo("toCurrencyCode"));
        });
    }

    [Test]
    public async Task Convert_ShouldReturn_ErrorForWrongAmount()
    {
        // Act
        var operationResult = await _sut.Convert(fromCurrencyCode: "CHF", toCurrencyCode: "USD", amount: -1,
            _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.False);
            Assert.That(operationResult.Errors, Is.Not.Empty);
            Assert.That(operationResult.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.InvalidAmount));
            Assert.That(operationResult.Errors.First().PropertyName, Is.EqualTo("amount"));
        });
    }

    [Test]
    public async Task Convert_ShouldReturn_ErrorForNotExistingBalanceInWallet()
    {
        // Act
        var operationResult = await _sut.Convert(fromCurrencyCode: "CHF", toCurrencyCode: "USD", amount: 10.0M,
            _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.False);
            Assert.That(operationResult.Errors, Is.Not.Empty);
            Assert.That(operationResult.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.AccountInCurrencyDoesNotExist));
            Assert.That(operationResult.Errors.First().PropertyName, Is.Null);
        });
    }

    [Test]
    public async Task Convert_ShouldReturn_SuccessAndProperAmountForSingleConvertBothBalancesExistsAndHaveSameRates()
    {
        // Arrange
        const decimal initialBalanceUsd = 30.0M;
        const decimal initialBalanceChf = 10.0M;
        const decimal amountToConvert = 9.50M;
        
        _sut = Wallet.RecoverFrom(name: "test", balance: new List<Balance>
        {
            Balance.Create(currencyCode: "USD", initialBalanceUsd),
            Balance.Create(currencyCode: "CHF", initialBalanceChf)
        });

        const decimal expectedBalanceUsd = initialBalanceUsd - amountToConvert;
        var expectedBalanceChf = initialBalanceChf + Math.Round(
            Math.Round(amountToConvert * Math.Round(_usdRate.BuyCourse / _usdRate.Rate, 2), 2) /
            Math.Round(_chfRate.SellCourse / _chfRate.Rate, 2), 2);
        
        // Act
        var operationResult = await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "CHF", amountToConvert,
            _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.True);
            Assert.That(operationResult.Errors, Is.Empty);
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("USD")).Amount, Is.EqualTo(expectedBalanceUsd));
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("CHF")).Amount, Is.EqualTo(expectedBalanceChf));
        });
    }

    [Test]
    public async Task Convert_ShouldReturn_SuccessAndProperAmountForMultipleConvertBothBalancesExistsAndHaveSameRates()
    {
        // Arrange
        const decimal initialBalanceUsd = 30.0M;
        const decimal initialBalanceChf = 10.0M;
        const decimal firstAmountToConvert = 5.50M;
        const decimal secondAmountToConvert = 4.0M;

        _sut = Wallet.RecoverFrom(name: "test", balance: new List<Balance>
        {
            Balance.Create(currencyCode: "USD", initialBalanceUsd),
            Balance.Create(currencyCode: "CHF", initialBalanceChf)
        });

        const decimal expectedBalanceUsd = initialBalanceUsd - (firstAmountToConvert + secondAmountToConvert);
        var expectedBalanceChf = initialBalanceChf + Math.Round(
            Math.Round(firstAmountToConvert * Math.Round(_usdRate.BuyCourse / _usdRate.Rate, 2), 2) /
            Math.Round(_chfRate.SellCourse / _chfRate.Rate, 2), 2) + Math.Round(
            Math.Round(secondAmountToConvert * Math.Round(_usdRate.BuyCourse / _usdRate.Rate, 2), 2) /
            Math.Round(_chfRate.SellCourse / _chfRate.Rate, 2), 2);
       
        // Act
        var operationResults = new List<OperationResult>
        {
            await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "CHF", firstAmountToConvert,
                _exchangeRatesRepository),
            await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "CHF", secondAmountToConvert,
                _exchangeRatesRepository)
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResults.Select(operation => operation.IsSuccess), Is.All.True);
            Assert.That(operationResults.Select(operation => operation.Errors), Is.All.Empty);
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("USD")).Amount, Is.EqualTo(expectedBalanceUsd));
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("CHF")).Amount, Is.EqualTo(expectedBalanceChf));
        });
    }

    [Test]
    public async Task Convert_ShouldReturn_SuccessAndProperAmountForSingleConvertOneBalanceExistAndHaveSameRates()
    {
        // Arrange
        const decimal initialBalanceUsd = 30.0M;
        const decimal amountToConvert = 9.50M;

        _sut = Wallet.RecoverFrom(name: "test", balance: new List<Balance>
        {
            Balance.Create("USD", initialBalanceUsd)
        });

        const decimal expectedBalanceUsd = initialBalanceUsd - amountToConvert;
        var expectedBalanceChf = Math.Round(
            Math.Round(amountToConvert * Math.Round(_usdRate.BuyCourse / _usdRate.Rate, 2), 2) /
            Math.Round(_chfRate.SellCourse / _chfRate.Rate, 2), 2);

        // Act
        var operationResult = await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "CHF", amountToConvert, _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.True);
            Assert.That(operationResult.Errors, Is.Empty);
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("USD")).Amount, Is.EqualTo(expectedBalanceUsd));
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("CHF")).Amount, Is.EqualTo(expectedBalanceChf));
        });
    }

    [Test]
    public async Task Convert_ShouldReturn_SuccessAndProperAmountForMultipleConvertOneBalanceExistAndHaveSameRates()
    {
        // Arrange
        const decimal initialBalanceUsd = 30.0M;
        const decimal firstAmountToConvert = 5.50M;
        const decimal secondAmountToConvert = 4.0M;

        _sut = Wallet.RecoverFrom(name: "test", balance: new List<Balance>
        {
            Balance.Create(currencyCode: "USD", initialBalanceUsd)
        });

        const decimal expectedBalanceUsd = initialBalanceUsd - (firstAmountToConvert + secondAmountToConvert);
        var expectedBalanceChf = Math.Round(
            Math.Round(firstAmountToConvert * Math.Round(_usdRate.BuyCourse / _usdRate.Rate, 2), 2) /
            Math.Round(_chfRate.SellCourse / _chfRate.Rate, 2), 2) + Math.Round(
            Math.Round(secondAmountToConvert * Math.Round(_usdRate.BuyCourse / _usdRate.Rate, 2), 2) /
            Math.Round(_chfRate.SellCourse / _chfRate.Rate, 2), 2);

        // Act
        var operationResults = new List<OperationResult>
        {
            await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "CHF", firstAmountToConvert,
                _exchangeRatesRepository),
            await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "CHF", secondAmountToConvert,
                _exchangeRatesRepository)
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResults.Select(operation => operation.IsSuccess), Is.All.True);
            Assert.That(operationResults.Select(operation => operation.Errors), Is.All.Empty);
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("USD")).Amount, Is.EqualTo(expectedBalanceUsd));
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("CHF")).Amount, Is.EqualTo(expectedBalanceChf));
        });
    }

    [Test]
    public async Task Convert_ShouldReturn_SuccessAndProperAmountForSingleConvertBothBalancesExistsAndHaveDifferentRates()
    {
        // Arrange
        const decimal initialBalanceUsd = 30.0M;
        const decimal initialBalanceHuf = 10.0M;
        const decimal amountToConvert = 9.50M;

        _sut = Wallet.RecoverFrom(name: "test", balance: new List<Balance>
        {
            Balance.Create(currencyCode: "USD", initialBalanceUsd),
            Balance.Create(currencyCode: "HUF", initialBalanceHuf)
        });

        const decimal expectedBalanceUsd = initialBalanceUsd - amountToConvert;
        var expectedBalanceHuf = initialBalanceHuf + Math.Round(
            Math.Round(amountToConvert * Math.Round(_usdRate.BuyCourse / _usdRate.Rate, 2), 2) /
            Math.Round(_hufRate.SellCourse / _hufRate.Rate, 2), 2);

        // Act
        var operationResult = await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "HUF", amountToConvert,
            _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.True);
            Assert.That(operationResult.Errors, Is.Empty);
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("USD")).Amount, Is.EqualTo(expectedBalanceUsd));
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("HUF")).Amount, Is.EqualTo(expectedBalanceHuf));
        });
    }

    [Test]
    public async Task Convert_ShouldReturn_SuccessAndProperAmountForMultipleConvertBothBalancesExistsAndHaveDifferentRates()
    {
        // Arrange
        const decimal initialBalanceUsd = 30.0M;
        const decimal initialBalanceHuf = 10.0M;
        const decimal firstAmountToConvert = 5.50M;
        const decimal secondAmountToConvert = 4.0M;

        _sut = Wallet.RecoverFrom(name: "test", balance: new List<Balance>
        {
            Balance.Create(currencyCode: "USD", initialBalanceUsd),
            Balance.Create(currencyCode: "HUF", initialBalanceHuf)
        });

        const decimal expectedBalanceUsd = initialBalanceUsd - (firstAmountToConvert + secondAmountToConvert);
        var expectedBalanceHuf = initialBalanceHuf + Math.Round(
            Math.Round(firstAmountToConvert * Math.Round(_usdRate.BuyCourse / _usdRate.Rate, 2), 2) /
            Math.Round(_hufRate.SellCourse / _hufRate.Rate, 2), 2) + Math.Round(
            Math.Round(secondAmountToConvert * Math.Round(_usdRate.BuyCourse / _usdRate.Rate, 2), 2) /
            Math.Round(_hufRate.SellCourse / _hufRate.Rate, 2), 2);

        // Act
        var operationResults = new List<OperationResult>
        {
            await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "HUF", firstAmountToConvert,
                _exchangeRatesRepository),
            await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "HUF", secondAmountToConvert,
                _exchangeRatesRepository)
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResults.Select(operation => operation.IsSuccess), Is.All.True);
            Assert.That(operationResults.Select(operation => operation.Errors), Is.All.Empty);
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("USD")).Amount, Is.EqualTo(expectedBalanceUsd));
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("HUF")).Amount, Is.EqualTo(expectedBalanceHuf));
        });
    }

    [Test]
    public async Task Convert_ShouldReturn_SuccessAndProperAmountForSingleConvertOneBalanceExistAndHaveDifferentRates()
    {
        // Arrange
        const decimal initialBalanceUsd = 30.0M;
        const decimal amountToConvert = 9.50M;

        _sut = Wallet.RecoverFrom(name: "test", balance: new List<Balance>
        {
            Balance.Create(currencyCode: "USD", initialBalanceUsd)
        });

        const decimal expectedBalanceUsd = initialBalanceUsd - amountToConvert;
        var expectedBalanceHuf = Math.Round(
            Math.Round(amountToConvert * Math.Round(_usdRate.BuyCourse / _usdRate.Rate, 2), 2) /
            Math.Round(_hufRate.SellCourse / _hufRate.Rate, 2), 2);

        // Act
        var operationResult = await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "HUF", amountToConvert,
            _exchangeRatesRepository);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResult.IsSuccess, Is.True);
            Assert.That(operationResult.Errors, Is.Empty);
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("USD")).Amount, Is.EqualTo(expectedBalanceUsd));
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("HUF")).Amount, Is.EqualTo(expectedBalanceHuf));
        });
    }

    [Test]
    public async Task Convert_ShouldReturn_SuccessAndProperAmountForMultipleConvertOneBalanceExistAndHaveDifferentRates()
    {
        // Arrange
        const decimal initialBalanceUsd = 30.0M;
        const decimal firstAmountToConvert = 5.50M;
        const decimal secondAmountToConvert = 4.0M;

        _sut = Wallet.RecoverFrom(name: "test", balance: new List<Balance>
        {
            Balance.Create(currencyCode: "USD", initialBalanceUsd)
        });

        const decimal expectedBalanceUsd = initialBalanceUsd - (firstAmountToConvert + secondAmountToConvert);
        var expectedBalanceHuf = Math.Round(
            Math.Round(firstAmountToConvert * Math.Round(_usdRate.BuyCourse / _usdRate.Rate, 2), 2) /
            Math.Round(_hufRate.SellCourse / _hufRate.Rate, 2), 2) + Math.Round(
            Math.Round(secondAmountToConvert * Math.Round(_usdRate.BuyCourse / _usdRate.Rate, 2), 2) /
            Math.Round(_hufRate.SellCourse / _hufRate.Rate, 2), 2);

        // Act
        var operationResults = new List<OperationResult>
        {
            await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "HUF", firstAmountToConvert,
                _exchangeRatesRepository),
            await _sut.Convert(fromCurrencyCode: "USD", toCurrencyCode: "HUF", secondAmountToConvert,
                _exchangeRatesRepository)
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operationResults.Select(operation => operation.IsSuccess), Is.All.True);
            Assert.That(operationResults.Select(operation => operation.Errors), Is.All.Empty);
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("USD")).Amount, Is.EqualTo(expectedBalanceUsd));
            Assert.That(_sut.Balances.Single(b => b.CurrencyCode.Equals("HUF")).Amount, Is.EqualTo(expectedBalanceHuf));
        });
    }
}