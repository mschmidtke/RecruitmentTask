using NSubstitute;
using RecruitmentTask.Core.ExchangeRate;
using RecruitmentTask.Core.ExchangeRate.Model;
using RecruitmentTask.Core.Validations;
using RecruitmentTask.Core.Wallets;

namespace RecruitmentTask.UnitTests.Core.Wallets
{
    [TestFixture]
    public class WalletDomainServiceTests
    {
        private IWalletsRepository _walletsRepository;
        private IExchangeRatesRepository _exchangeRatesRepository;

        private readonly Currency _usdRate = new Currency("dolar amerykański", 1, "USD", 3.8385M, 3.9161M);
        private readonly Currency _hufRate = new Currency("forint (Węgry)", 100, "HUF", 1.0416M, 1.0626M);
        private readonly Currency _chfRate = new Currency("frank szwajcarski", 1, "CHF", 4.3513M, 4.4393M);

        private IWalletDomainService _sut;

        [SetUp]
        public void Setup()
        {
            _walletsRepository = Substitute.For<IWalletsRepository>();
            _exchangeRatesRepository = Substitute.For<IExchangeRatesRepository>();

            _exchangeRatesRepository.GetMostRecentExchangeRates().Returns(new ExchangeRates(quotationDate: DateTime.Now,
                publicationDate: DateTime.Now,
                currencies: new List<Currency>
                {
                    _usdRate,
                    _hufRate,
                    _chfRate
                }));

            _sut = new WalletDomainService(_walletsRepository, _exchangeRatesRepository);
        }

        [Test]
        public async Task Create_ShouldReturnFailIfWalletAlreadyExist()
        {
            // Arrange
            _walletsRepository.GetWallet(name: Arg.Any<string>()).Returns(Wallet.Create("Nowy"));

            // Act
            var result = await _sut.Create(name: "Nowy");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Errors, Is.Not.Empty);
                Assert.That(result.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.WalletAlreadyExist));
                Assert.That(result.Errors.First().PropertyName, Is.Null);

                _walletsRepository.Received(0).SaveWallet(Arg.Any<Wallet>());
            });
        }

        [Test]
        public async Task Create_ShouldReturnSuccessIfWalletNotExist()
        {
            // Arrange
            const string expectedName = "Nowy";
            const int expectedId = 1;

            _walletsRepository.GetWallet(name: Arg.Any<string>()).Returns((Wallet)null);
            _walletsRepository
                .SaveWallet(Arg.Is<Wallet>(wallet => wallet.Name.Equals(expectedName) && !wallet.Balances.Any()))
                .Returns(expectedId);

            // Act
            var result = await _sut.Create(expectedName);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Errors, Is.Empty);
                Assert.That(result.Value, Is.EqualTo(1));

                _walletsRepository.Received(1)
                    .SaveWallet(Arg.Is<Wallet>(wallet => wallet.Name.Equals(expectedName) && !wallet.Balances.Any()));
            });
        }

        [Test]
        public async Task Deposit_ShouldReturnFailIfWalletDoesNotExist()
        {
            // Arrange
            _walletsRepository.GetWallet(name: Arg.Any<string>()).Returns((Wallet)null);

            // Act
            var result = await _sut.Deposit(id: 1, currencyCode: "USD", amount: 10.0M);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Errors, Is.Not.Empty);
                Assert.That(result.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.WalletDoesNotExist));
                Assert.That(result.Errors.First().PropertyName, Is.Null);

                _walletsRepository.Received(0).SaveWallet(Arg.Any<Wallet>());
            });
        }

        [Test]
        public async Task Deposit_ShouldReturnSuccessIfWalletExist()
        {
            // Arrange
            const string expectedName = "Nowy";
            const int expectedId = 1;
            const decimal expectedAmount = 10.0M;
            const string expectedCurrencyCode = "USD";

            _walletsRepository.GetWallet(expectedId).Returns(Wallet.Create(expectedName));
            _walletsRepository.SaveWallet(Arg.Any<Wallet>()).Returns(expectedId);

            // Act
            var result = await _sut.Deposit(expectedId, expectedCurrencyCode, expectedAmount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Errors, Is.Empty);

                _walletsRepository.Received(1)
                    .SaveWallet(Arg.Is<Wallet>(wallet =>
                        wallet.Name.Equals(expectedName) && wallet.Balances.Any(balance =>
                            balance.Amount == expectedAmount && balance.CurrencyCode.Equals(expectedCurrencyCode))));
            });
        }

        [Test]
        public async Task Withdrawal_ShouldReturnFailIfWalletDoesNotExist()
        {
            // Arrange
            _walletsRepository.GetWallet(name: Arg.Any<string>()).Returns((Wallet)null);

            // Act
            var result = await _sut.Withdrawal(id: 1, currencyCode: "USD", amount: 10.0M);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Errors, Is.Not.Empty);
                Assert.That(result.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.WalletDoesNotExist));
                Assert.That(result.Errors.First().PropertyName, Is.Null);

                _walletsRepository.Received(0).SaveWallet(Arg.Any<Wallet>());
            });
        }

        [Test]
        public async Task Withdrawal_ShouldReturnFailIfAmountExceededBalance()
        {
            // Arrange
            const string expectedName = "Nowy";
            const int expectedId = 1;
            const decimal initialBalance = 10.0M;
            const decimal expectedAmount = 30.0M;
            const string expectedCurrencyCode = "USD";

            _walletsRepository.GetWallet(expectedId).Returns(Wallet.RecoverFrom(expectedName, new Balance[]
            {
                Balance.Create(expectedCurrencyCode, initialBalance),
            }));
            _walletsRepository.SaveWallet(Arg.Any<Wallet>()).Returns(expectedId);

            // Act
            var result = await _sut.Withdrawal(id: 1, currencyCode: "USD", expectedAmount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Errors, Is.Not.Empty);
                Assert.That(result.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.NotEnoughBalance));
                Assert.That(result.Errors.First().PropertyName, Is.EqualTo("amount"));

                _walletsRepository.Received(0).SaveWallet(Arg.Any<Wallet>());
            });
        }

        [Test]
        public async Task Withdrawal_ShouldReturnSuccessIfWalletExist()
        {
            // Arrange
            const string expectedName = "Nowy";
            const int expectedId = 1;
            const decimal initialBalance = 30.0M;
            const decimal expectedAmount = 10.0M;
            const string expectedCurrencyCode = "USD";

            _walletsRepository.GetWallet(expectedId).Returns(Wallet.RecoverFrom(expectedName, new Balance[]
            {
                Balance.Create(expectedCurrencyCode, initialBalance), 
            }));
            _walletsRepository.SaveWallet(Arg.Any<Wallet>()).Returns(expectedId);

            // Act
            var result = await _sut.Withdrawal(expectedId, expectedCurrencyCode, expectedAmount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Errors, Is.Empty);

                _walletsRepository.Received(1)
                    .SaveWallet(Arg.Is<Wallet>(wallet =>
                        wallet.Name.Equals(expectedName) && wallet.Balances.Any(balance =>
                            balance.Amount == initialBalance - expectedAmount &&
                            balance.CurrencyCode.Equals(expectedCurrencyCode))));
            });
        }

        [Test]
        public async Task Convert_ShouldReturnFailIfWalletDoesNotExist()
        {
            // Arrange
            _walletsRepository.GetWallet(name: Arg.Any<string>()).Returns((Wallet)null);

            // Act
            var result = await _sut.Convert(id: 1, "USD", "EUR", 10.0M);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Errors, Is.Not.Empty);
                Assert.That(result.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.WalletDoesNotExist));
                Assert.That(result.Errors.First().PropertyName, Is.Null);

                _walletsRepository.Received(0).SaveWallet(Arg.Any<Wallet>());
            });
        }

        [Test]
        public async Task Convert_ShouldReturn_ErrorIfBalanceDoesNotExistInWallet()
        {
            // Arrange
            _walletsRepository.GetWallet(id: Arg.Any<int>()).Returns(Wallet.RecoverFrom("Test", new Balance[]
            {
                Balance.Create(currencyCode: "USD", amount: 10.0M), 
            }));

            // Act
            var operationResult = await _sut.Convert(id: 1, "CHF", "USD", 10.0M);

            // Assert
            Assert.That(operationResult.IsSuccess, Is.False);
            Assert.That(operationResult.Errors, Is.Not.Empty);
            Assert.That(operationResult.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.AccountInCurrencyDoesNotExist));
            Assert.That(operationResult.Errors.First().PropertyName, Is.Null);

            _walletsRepository.Received(0).SaveWallet(Arg.Any<Wallet>());
        }

        [Test]
        public async Task Convert_ShouldReturn_ErrorIfBalanceDoesExistInWallet()
        {
            // Arrange
            const string expectedName = "Nowy";
            const string expectedWithdrawalCurrencyCode = "CHF";
            const string expectedDepositCurrencyCode = "USD";

            _walletsRepository.GetWallet(Arg.Any<int>()).Returns(Wallet.RecoverFrom(expectedName, new List<Balance>
            {
                Balance.Create(currencyCode: expectedWithdrawalCurrencyCode, amount: 10.0M),
            }));

            // Act
            var operationResult = await _sut.Convert(id: 1, expectedWithdrawalCurrencyCode, expectedDepositCurrencyCode, 10.0M);

            // Assert
            Assert.That(operationResult.IsSuccess, Is.True);
            Assert.That(operationResult.Errors, Is.Empty);

            _walletsRepository.Received(1).SaveWallet(Arg.Is<Wallet>(wallet =>
                wallet.Name.Equals(expectedName) 
                && wallet.Balances
                    .Single(balance => balance.CurrencyCode.Equals(expectedWithdrawalCurrencyCode)).Amount == 0.0M
                && wallet.Balances.Single(balance => balance.CurrencyCode.Equals(expectedDepositCurrencyCode)).Amount == 11.10M));
        }

        [TestCase(1, 10, 0, 10)]
        [TestCase(2, 10, 10, 10)]
        [TestCase(3, 10, 20, 10)]
        public async Task GetWallets_ShouldExecuteGetWalletsWithProperParameters(int pageNumber, short count,
            int expectedOffset, short expectedCount)
        {
            // Act
            await _sut.GetWallets(pageNumber, count);

            // Assert
            _walletsRepository.Received(1).GetWallets(expectedOffset, expectedCount);
        }
    }
}
