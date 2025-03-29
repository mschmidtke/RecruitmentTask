using RecruitmentTask.Contracts.Wallets.Responses;
using RecruitmentTask.Core.ExchangeRate.Model;
using Balance = RecruitmentTask.Core.Wallets.Balance;

namespace RecruitmentTask.Api.Wallet
{
    public static class WalletMapper
    {
        public static GetWallets ToContract(this IEnumerable<Core.Wallets.Wallet> wallets, ExchangeRates rates)
        {
            return new GetWallets
            {
                Wallets = wallets.Select(wallet => ToContract(wallet, rates))
            };
        }

        public static Contracts.Wallets.Responses.Wallet ToContract(
            this Core.Wallets.Wallet wallet, ExchangeRates rates)
        {
            return new Contracts.Wallets.Responses.Wallet
            {
                Name = wallet.Name,
                Balances = wallet.Balances.Select(balance => ToContract(balance, rates))
            };
        }

        private static Contracts.Wallets.Responses.Balance ToContract(
            this Balance balance, ExchangeRates rates)
        {
            var name = rates.Currencies.First(rate => rate.Code == balance.CurrencyCode).Name;

            return new Contracts.Wallets.Responses.Balance
            {
                Amount = balance.Amount,
                CurrencyCode = balance.CurrencyCode,
                CurrencyName = name
            };
        }
    }
}
