namespace RecruitmentTask.Contracts.Wallets.Responses
{
    public class GetWallets
    {
        public IEnumerable<Wallet> Wallets { get; set; }
    }

    public class Wallet
    {
        public string Name { get; set; }
        public IEnumerable<Balance> Balances {get;set;}
    }

    public class Balance
    {
        public decimal Amount { get; set; }

        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
    }
}
