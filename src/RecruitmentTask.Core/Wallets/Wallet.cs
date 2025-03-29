using RecruitmentTask.Core.ExchangeRate;
using RecruitmentTask.Core.ExchangeRate.Model;
using RecruitmentTask.Core.Validations;

namespace RecruitmentTask.Core.Wallets
{
    public class Wallet
    {
        public string Name { get; }
        public IList<Balance> Balances { get; }

        public static Wallet Create(string name)
        {
            return new Wallet(name);
        }

        public static Wallet RecoverFrom(string name, IList<Balance> balance)
        {
            return new Wallet(name, balance);
        }

        public async Task<OperationResult> Deposit(string currencyCode, decimal amount,
            IExchangeRatesRepository exchangeRatesRepository)
        {
            var errors = await ValidateDepositOrWithdrawal(currencyCode, amount, exchangeRatesRepository);

            if (errors.Any())
            {
                return OperationResult.Fail(errors);
            }

            var currencyBalance = Balances.SingleOrDefault(currency =>
                currency.CurrencyCode.Equals(currencyCode, StringComparison.CurrentCultureIgnoreCase));

            if (currencyBalance == null)
            {
                Balances.Add(Wallets.Balance.Create(currencyCode, amount));
                return OperationResult.Success();
            }

            currencyBalance.AddAmount(amount);

            return OperationResult.Success();
        }

        public async Task<OperationResult> Withdrawal(string currencyCode, decimal amount,
            IExchangeRatesRepository exchangeRatesRepository)
        {
            var errors = await ValidateDepositOrWithdrawal(currencyCode, amount, exchangeRatesRepository);

            if (errors.Any())
            {
                return OperationResult.Fail(errors);
            }

            var currencyBalance = Balances.SingleOrDefault(currency =>
                currency.CurrencyCode.Equals(currencyCode, StringComparison.CurrentCultureIgnoreCase));

            if (currencyBalance == null)
            {
                Balances.Add(Wallets.Balance.Create(currencyCode, amount));
                return OperationResult.Fail(OperationError.Create(ErrorCodes.AccountInCurrencyDoesNotExist, null));
            }

            var operationResult = currencyBalance.WithdrawalAmount(amount);

            return operationResult;
        }

        public async Task<OperationResult> Convert(string fromCurrencyCode, string toCurrencyCode, decimal amount,
            IExchangeRatesRepository exchangeRatesRepository)
        {
            var rates = await exchangeRatesRepository.GetMostRecentExchangeRates();

            var errors = ValidateConversion(fromCurrencyCode, toCurrencyCode, amount, rates);

            if (errors.Any())
            {
                return OperationResult.Fail(errors);
            }

            var fromBalance = Balances.SingleOrDefault(currency =>
                currency.CurrencyCode.Equals(fromCurrencyCode, StringComparison.CurrentCultureIgnoreCase));

            var toBalance = Balances.SingleOrDefault(currency =>
                currency.CurrencyCode.Equals(toCurrencyCode, StringComparison.CurrentCultureIgnoreCase));

            if (fromBalance == null)
            {
                return OperationResult.Fail(OperationError.Create(ErrorCodes.AccountInCurrencyDoesNotExist, null));
            }

            var operationResult = fromBalance.WithdrawalAmount(amount);

            if (!operationResult.IsSuccess)
            {
                return operationResult;
            }

            var fromRates = rates.Currencies.Single(rate =>
                rate.Code.Equals(fromCurrencyCode, StringComparison.InvariantCultureIgnoreCase));

            var toRates = rates.Currencies.Single(rate =>
                rate.Code.Equals(toCurrencyCode, StringComparison.InvariantCultureIgnoreCase));

            var afterConversion =
                Math.Round(
                    Math.Round(amount * Math.Round(fromRates.BuyCourse / fromRates.Rate, 2), 2) /
                    Math.Round(toRates.SellCourse / toRates.Rate, 2), 2);

            if (toBalance == null)
            {
                Balances.Add(Wallets.Balance.Create(toCurrencyCode, afterConversion));
                return OperationResult.Success();
            }

            toBalance.AddAmount(afterConversion);

            return OperationResult.Success();
        }

        private IEnumerable<OperationError> ValidateConversion(string fromCurrencyCode, string toCurrencyCode,
            decimal amount, ExchangeRates latestRates)
        {
            var errors = new List<OperationError>();

            var fromRate = latestRates.Currencies.SingleOrDefault(currency =>
                currency.Code.Equals(fromCurrencyCode, StringComparison.InvariantCultureIgnoreCase));

            var toRate = latestRates.Currencies.SingleOrDefault(currency =>
                currency.Code.Equals(toCurrencyCode, StringComparison.InvariantCultureIgnoreCase));

            if (fromRate == null)
            {
                errors.Add(OperationError.Create(ErrorCodes.NotSupportedCurrency, nameof(fromCurrencyCode)));
            }

            if (toRate == null)
            {
                errors.Add(OperationError.Create(ErrorCodes.NotSupportedCurrency, nameof(toCurrencyCode)));
            }

            if (amount <= 0)
            {
                errors.Add(OperationError.Create(ErrorCodes.InvalidAmount, nameof(amount)));
            }

            return errors;
        }

        private async Task<IEnumerable<OperationError>> ValidateDepositOrWithdrawal(string currencyCode, decimal amount,
            IExchangeRatesRepository exchangeRatesRepository)
        {
            var errors = new List<OperationError>();

            var latestRates = await exchangeRatesRepository.GetMostRecentExchangeRates();

            var rate = latestRates.Currencies.SingleOrDefault(currency =>
                currency.Code.Equals(currencyCode, StringComparison.InvariantCultureIgnoreCase));

            if (rate == null)
            {
                errors.Add(OperationError.Create(ErrorCodes.NotSupportedCurrency, nameof(currencyCode)));
            }

            if (amount <= 0)
            {
                errors.Add(OperationError.Create(ErrorCodes.InvalidAmount, nameof(amount)));
            }

            return errors;
        }

        private Wallet(string name)
        {
            Name = name;
            Balances = new List<Balance>();
        }

        private Wallet(string name, IList<Balance> balance)
        {
            Name = name;
            Balances = balance;
        }
    }
}
