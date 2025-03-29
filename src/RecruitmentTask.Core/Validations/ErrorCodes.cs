namespace RecruitmentTask.Core.Validations
{
    public static class ErrorCodes
    {
        public const string ValueTooLong = nameof(ValueTooLong);
        public const string NotSupportedCurrency = nameof(NotSupportedCurrency);
        public const string InvalidAmount = nameof(InvalidAmount);
        public const string AccountInCurrencyDoesNotExist = nameof(AccountInCurrencyDoesNotExist);
        public const string WalletDoesNotExist = nameof(WalletDoesNotExist);
        public const string WalletAlreadyExist = nameof(WalletAlreadyExist);
        public const string NotEnoughBalance = nameof(NotEnoughBalance);
        public const string ValueCannotBeLessOrEqualZero = nameof(ValueCannotBeLessOrEqualZero);
    }
}
