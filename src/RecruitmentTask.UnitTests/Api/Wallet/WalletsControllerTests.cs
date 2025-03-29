using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using RecruitmentTask.Api.Wallet;
using RecruitmentTask.Contracts.Wallets.Requests;
using RecruitmentTask.Core;
using RecruitmentTask.Core.ExchangeRate;
using RecruitmentTask.Core.ExchangeRate.Model;
using RecruitmentTask.Core.Validations;
using RecruitmentTask.Core.Wallets;
using ValidationResult = RecruitmentTask.Contracts.Validations.ValidationResult;

namespace RecruitmentTask.UnitTests.Api.Wallet
{
    [TestFixture]
    public class WalletsControllerTests
    {
        private IWalletDomainService _walletDomainService;
        private IExchangeRatesRepository _exchangeRatesRepository;

        private WalletsController _sut;

        [SetUp]
        public void Setup()
        {
            _walletDomainService = Substitute.For<IWalletDomainService>();
            _exchangeRatesRepository = Substitute.For<IExchangeRatesRepository>();

            _sut = new WalletsController(_walletDomainService,
                _exchangeRatesRepository);
        }

        [TestCase(0, 10, ErrorCodes.ValueCannotBeLessOrEqualZero, "pageNumber")]
        [TestCase(-2, 10, ErrorCodes.ValueCannotBeLessOrEqualZero, "pageNumber")]
        [TestCase(1, -10, ErrorCodes.ValueCannotBeLessOrEqualZero, "countPerPage")]
        [TestCase(1, 0, ErrorCodes.ValueCannotBeLessOrEqualZero, "countPerPage")]
        [TestCase(1, 101, ErrorCodes.ValueTooLong, "countPerPage")]
        public async Task Get_ShouldReturnBadRequestForNotProperInputParameters(int pageNumber, short countPerPage, string expectedErrorCode, string expectedPropertyName)
        {
            // Act
            var result = await _sut.Get(pageNumber, countPerPage);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
                var response = result as BadRequestObjectResult;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                var body = response.Value as ValidationResult;
                Assert.That(body.Errors, Is.Not.Empty);
                Assert.That(body.Errors.First().ErrorCode, Is.EqualTo(expectedErrorCode));
                Assert.That(body.Errors.First().PropertyName, Is.EqualTo(expectedPropertyName));
            });
        }

        [Test]
        public async Task Get_ShouldReturnOkWithWalletsData()
        {
            // Arrange
            var expectedWallets = new[]
            {
                RecruitmentTask.Core.Wallets.Wallet.RecoverFrom(name: "testowy", balance:
                [
                    Balance.Create(currencyCode: "USD", amount: 1512.34M),
                    Balance.Create(currencyCode: "CHF", amount: 123.14M)
                ]),
                RecruitmentTask.Core.Wallets.Wallet.RecoverFrom(name: "testowy2", balance:
                [
                    Balance.Create(currencyCode: "HUF", amount: 14212.32M)
                ]),
            };
            var currencyDictionary = new[]
            {
                new Currency(name: "dolar amerykański", rate: 1, "USD", buyCourse: 3.8385M, sellCourse: 3.9161M),
                new Currency(name: "forint (Węgry)", rate: 100, "HUF", buyCourse: 1.0416M, sellCourse: 1.0626M),
                new Currency(name: "frank szwajcarski", rate: 1, "CHF", buyCourse: 4.3513M, sellCourse: 4.4393M)
            };
            _exchangeRatesRepository.GetMostRecentExchangeRates().Returns(new ExchangeRates(DateTime.Now, DateTime.Now, currencyDictionary));

            _walletDomainService.GetWallets(pageNumber: 1, count: 10).Returns(expectedWallets);

            // Act
            var result = await _sut.Get(pageNumber: 1, countPerPage: 10);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<OkObjectResult>());
                var response = result as OkObjectResult;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                var body = response.Value as Contracts.Wallets.Responses.GetWallets;
                Assert.That(body.Wallets, Is.Not.Empty);
                Assert.That(body.Wallets.Count(), Is.EqualTo(2));
                var wallets = body.Wallets.ToArray();
                Assert.That(wallets[0].Name, Is.EqualTo(expectedWallets[0].Name));
                Assert.That(wallets[0].Balances.ToArray()[0].CurrencyCode, Is.EqualTo(expectedWallets[0].Balances[0].CurrencyCode));
                Assert.That(wallets[0].Balances.ToArray()[0].CurrencyName,
                    Is.EqualTo(currencyDictionary
                        .Single(cd => cd.Code.Equals(expectedWallets[0].Balances[0].CurrencyCode)).Name));
                Assert.That(wallets[0].Balances.ToArray()[0].Amount, Is.EqualTo(expectedWallets[0].Balances[0].Amount));
                Assert.That(wallets[0].Balances.ToArray()[1].CurrencyCode, Is.EqualTo(expectedWallets[0].Balances[1].CurrencyCode));
                Assert.That(wallets[0].Balances.ToArray()[1].CurrencyName,
                    Is.EqualTo(currencyDictionary
                        .Single(cd => cd.Code.Equals(expectedWallets[0].Balances[1].CurrencyCode)).Name));
                Assert.That(wallets[0].Balances.ToArray()[1].Amount, Is.EqualTo(expectedWallets[0].Balances[1].Amount));

                Assert.That(wallets[1].Name, Is.EqualTo(expectedWallets[1].Name));
                Assert.That(wallets[1].Balances.ToArray()[0].CurrencyCode, Is.EqualTo(expectedWallets[1].Balances[0].CurrencyCode));
                Assert.That(wallets[1].Balances.ToArray()[0].CurrencyName,
                    Is.EqualTo(currencyDictionary
                        .Single(cd => cd.Code.Equals(expectedWallets[1].Balances[0].CurrencyCode)).Name));
                Assert.That(wallets[1].Balances.ToArray()[0].Amount, Is.EqualTo(expectedWallets[1].Balances[0].Amount));
            });
        }

        [Test]
        public async Task Create_ShouldReturnBadRequestWhenNameIsTooLong()
        {
            // Act
            var result =
                await _sut.Create(new CreateWalletRequest { Name = string.Concat(Enumerable.Repeat("a", 151)) });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
                var response = result as BadRequestObjectResult;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                var body = response.Value as ValidationResult;
                Assert.That(body.Errors, Is.Not.Empty);
                Assert.That(body.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.ValueTooLong));
                Assert.That(body.Errors.First().PropertyName, Is.EqualTo("Name"));
            });
        }

        [Test]
        public async Task Create_ShouldReturnBadRequestWhenDomainServiceFails()
        {
            // Arrange
            const string walletName = "Test wallet";

            _walletDomainService.Create(walletName)
                .Returns(OperationResultWithValue<int>.Fail(OperationError.Create(ErrorCodes.ValueTooLong, "Name")));

            // Act
            var result =
                await _sut.Create(new CreateWalletRequest { Name = walletName });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
                var response = result as BadRequestObjectResult;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                var body = response.Value as ValidationResult;
                Assert.That(body.Errors, Is.Not.Empty);
                Assert.That(body.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.ValueTooLong));
                Assert.That(body.Errors.First().PropertyName, Is.EqualTo("Name"));
            });
        }

        [Test]
        public async Task Create_ShouldReturnOkWithWalletId()
        {
            // Arrange
            const string walletName = "Test wallet";
            const int expectedWalletId = 1;

            _walletDomainService.Create(walletName)
                .Returns(OperationResultWithValue<int>.Success(expectedWalletId));

            // Act
            var result =
                await _sut.Create(new CreateWalletRequest { Name = walletName });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<OkObjectResult>());
                var response = result as OkObjectResult;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                var body = response.Value as Contracts.Wallets.Responses.CreationResponse;
                Assert.That(body.Id, Is.EqualTo(expectedWalletId));
            });
        }

        [Test]
        public async Task Deposit_ShouldReturnBadRequestWhenDomainServiceFails()
        {
            // Arrange
            _walletDomainService.Deposit(id: Arg.Any<int>(), currencyCode: Arg.Any<string>(), amount: Arg.Any<decimal>())
                .Returns(OperationResult.Fail(OperationError.Create(errorCode: ErrorCodes.WalletDoesNotExist, propertyName: null)));

            // Act
            var result =
                await _sut.Deposit(id: 1, new DepositOrWithdrawalRequest { Amount = 10.0m, CurrencyCode = "USD" });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
                var response = result as BadRequestObjectResult;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                var body = response.Value as ValidationResult;
                Assert.That(body.Errors, Is.Not.Empty);
                Assert.That(body.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.WalletDoesNotExist));
                Assert.That(body.Errors.First().PropertyName, Is.Null);
            });
        }

        [Test]
        public async Task Deposit_ShouldReturnOkWithWalletId()
        {
            // Arrange
            _walletDomainService.Deposit(id: Arg.Any<int>(), currencyCode: Arg.Any<string>(), amount: Arg.Any<decimal>())
                .Returns(OperationResult.Success());

            // Act
            var result =
                await _sut.Deposit(id: 1, new DepositOrWithdrawalRequest { Amount = 142.23M, CurrencyCode = "USD" });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<OkResult>());
                var response = result as OkResult;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            });
        }

        [Test]
        public async Task Withdrawal_ShouldReturnBadRequestWhenDomainServiceFails()
        {
            // Arrange
            _walletDomainService.Withdrawal(id: Arg.Any<int>(), currencyCode: Arg.Any<string>(), amount: Arg.Any<decimal>())
                .Returns(OperationResult.Fail(OperationError.Create(errorCode: ErrorCodes.WalletDoesNotExist, propertyName: null)));

            // Act
            var result =
                await _sut.Withdrawal(id: 1, new DepositOrWithdrawalRequest { Amount = 10.0m, CurrencyCode = "USD" });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
                var response = result as BadRequestObjectResult;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                var body = response.Value as ValidationResult;
                Assert.That(body.Errors, Is.Not.Empty);
                Assert.That(body.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.WalletDoesNotExist));
                Assert.That(body.Errors.First().PropertyName, Is.Null);
            });
        }

        [Test]
        public async Task Withdrawal_ShouldReturnOkWithWalletId()
        {
            // Arrange
            _walletDomainService.Withdrawal(id: Arg.Any<int>(), currencyCode: Arg.Any<string>(), amount: Arg.Any<decimal>())
                .Returns(OperationResult.Success());

            // Act
            var result =
                await _sut.Withdrawal(id: 1, new DepositOrWithdrawalRequest { Amount = 142.23M, CurrencyCode = "USD" });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<OkResult>());
                var response = result as OkResult;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            });
        }

        [Test]
        public async Task Convert_ShouldReturnBadRequestWhenDomainServiceFails()
        {
            // Arrange
            _walletDomainService.Convert(id: Arg.Any<int>(), fromCurrencyCode: Arg.Any<string>(), toCurrencyCode: Arg.Any<string>(), amount: Arg.Any<decimal>())
                .Returns(OperationResult.Fail(OperationError.Create(errorCode: ErrorCodes.WalletDoesNotExist, propertyName: null)));

            // Act
            var result =
                await _sut.Convert(id: 1, new ConvertRequest { Amount = 10.0m, FromCurrencyCode = "USD", ToCurrencyCode = "EUR" });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
                var response = result as BadRequestObjectResult;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                var body = response.Value as ValidationResult;
                Assert.That(body.Errors, Is.Not.Empty);
                Assert.That(body.Errors.First().ErrorCode, Is.EqualTo(ErrorCodes.WalletDoesNotExist));
                Assert.That(body.Errors.First().PropertyName, Is.Null);
            });
        }

        [Test]
        public async Task Convert_ShouldReturnOkWithWalletId()
        {
            // Arrange
            _walletDomainService.Convert(id: Arg.Any<int>(), fromCurrencyCode: Arg.Any<string>(), toCurrencyCode: Arg.Any<string>(), amount: Arg.Any<decimal>())
                .Returns(OperationResult.Success());

            // Act
            var result =
                await _sut.Convert(id: 1, new ConvertRequest { Amount = 142.23M, FromCurrencyCode = "USD", ToCurrencyCode = "EUR"});

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<OkResult>());
                var response = result as OkResult;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            });
        }
    }
}
