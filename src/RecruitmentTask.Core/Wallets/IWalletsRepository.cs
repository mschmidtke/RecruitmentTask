namespace RecruitmentTask.Core.Wallets;

public interface IWalletsRepository
{
    Task<int> SaveWallet(Wallet wallet);
    Task<Wallet?> GetWallet(string name);
    Task<Wallet?> GetWallet(int id);
    Task<IEnumerable<Wallet>> GetWallets(int offset, short take);
}