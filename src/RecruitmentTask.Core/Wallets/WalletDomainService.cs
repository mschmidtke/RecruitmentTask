using RecruitmentTask.Core.ExchangeRate;
using RecruitmentTask.Core.Validations;

namespace RecruitmentTask.Core.Wallets
{
    public interface IWalletDomainService
    {
        Task<OperationResultWithValue<int>> Create(string name);
        Task<OperationResult> Deposit(int id, string currencyCode, decimal amount);
        Task<OperationResult> Withdrawal(int id, string currencyCode, decimal amount);
        Task<OperationResult> Convert(int id, string fromCurrencyCode, string toCurrencyCode,
            decimal amount);
        Task<IEnumerable<Wallet>> GetWallets(int pageNumber, short count);
    }

    public class WalletDomainService : IWalletDomainService
    {
        private readonly IWalletsRepository _walletsRepository;
        private readonly IExchangeRatesRepository _exchangeRatesRepository;

        public WalletDomainService(IWalletsRepository walletsRepository,
            IExchangeRatesRepository exchangeRatesRepository)
        {
            _walletsRepository = walletsRepository;
            _exchangeRatesRepository = exchangeRatesRepository;
        }

        public async Task<OperationResultWithValue<int>> Create(string name)
        {
            var wallet = await _walletsRepository.GetWallet(name);

            if (wallet == null)
            {
                wallet = Wallet.Create(name);
                var id = await _walletsRepository.SaveWallet(wallet);

                return OperationResultWithValue<int>.Success(id);
            }

            return OperationResultWithValue<int>.Fail(OperationError.Create(ErrorCodes.WalletAlreadyExist, null));
        }

        public async Task<OperationResult> Deposit(int id, string currencyCode, decimal amount)
        {
            var wallet = await _walletsRepository.GetWallet(id);

            if (wallet == null)
            {
                return OperationResult.Fail(OperationError.Create(ErrorCodes.WalletDoesNotExist, null));
            }

            var operationResult = await wallet.Deposit(currencyCode, amount, _exchangeRatesRepository);

            if (operationResult.IsSuccess)
            {
                await _walletsRepository.SaveWallet(wallet);
            }

            return operationResult;
        }

        public async Task<OperationResult> Withdrawal(int id, string currencyCode, decimal amount)
        {
            var wallet = await _walletsRepository.GetWallet(id);

            if (wallet == null)
            {
                return OperationResult.Fail(OperationError.Create(ErrorCodes.WalletDoesNotExist, null));
            }

            var operationResult = await wallet.Withdrawal(currencyCode, amount, _exchangeRatesRepository);

            if (operationResult.IsSuccess)
            {
                await _walletsRepository.SaveWallet(wallet);
            }

            return operationResult;
        }

        public async Task<OperationResult> Convert(int id, string fromCurrencyCode, string toCurrencyCode,
            decimal amount)
        {
            var wallet = await _walletsRepository.GetWallet(id);

            if (wallet == null)
            {
                return OperationResult.Fail(OperationError.Create(ErrorCodes.WalletDoesNotExist, null));
            }

            var operationResult = await wallet.Convert(fromCurrencyCode, toCurrencyCode, amount, _exchangeRatesRepository);

            if (operationResult.IsSuccess)
            {
                await _walletsRepository.SaveWallet(wallet);
            }

            return operationResult;
        }

        public Task<IEnumerable<Wallet>> GetWallets(int pageNumber, short count)
        {
            var pffset = count * (pageNumber - 1);
            var take = count;

            return _walletsRepository.GetWallets(pffset, take);
        }
    }
}
