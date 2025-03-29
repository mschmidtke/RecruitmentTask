namespace RecruitmentTask.Contracts.Wallets.Requests
{
    public class DepositOrWithdrawalRequest
    {
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
    }
}
