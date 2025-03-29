using RecruitmentTask.Core.Validations;

namespace RecruitmentTask.Core.Wallets
{
    public class Balance
    {
        public decimal Amount { get; private set; }

        public string CurrencyCode { get; }

        public static Balance Create(string currencyCode, decimal amount)
        {
            return new Balance(currencyCode, amount);
        }

        private Balance(string currencyCode, decimal amount)
        {
            Amount = amount;
            CurrencyCode = currencyCode;
        }

        public OperationResult AddAmount(decimal amount)
        {
            Amount += amount;

            return OperationResult.Success();
        }

        public OperationResult WithdrawalAmount(decimal amount)
        {
            if (Amount < amount)
            {
                return OperationResult.Fail(OperationError.Create(ErrorCodes.NotEnoughBalance, nameof(amount)));
            }

            Amount -= amount;

            return OperationResult.Success();
        }
    }
}
