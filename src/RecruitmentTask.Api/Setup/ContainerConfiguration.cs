using RecruitmentTask.Api.ExchangeRateRefresher;
using RecruitmentTask.Api.ExchangeRateRefresher.Services;
using RecruitmentTask.Core.ExchangeRate;
using RecruitmentTask.Core.Wallets;
using RecruitmentTask.Infrastructure.ExchangeRate.Adapters;
using RecruitmentTask.Infrastructure.ExchangeRate.Repositories;
using RecruitmentTask.Infrastructure.Wallets.Repositories;

namespace RecruitmentTask.Api.Setup
{
    public static class ContainerConfiguration
    {
        public static void AddBackgroundExchangeRateUpdater(this IServiceCollection services)
        {
            services.AddHostedService<ExchangeRateUpdater>();
            services.AddSingleton<IExchangeRateAdapter, ExchangeRateAdapter>();
            services.AddSingleton<IExchangeRateService, ExchangeRateService>();
            services.AddSingleton<IExchangeRatesRepository, ExchangeRatesRepository>();
        }

        public static void AddWalletServices(this IServiceCollection services)
        {
            services.AddSingleton<IWalletsRepository, WalletsRepository>();
            services.AddSingleton<IWalletDomainService, WalletDomainService>();
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }
    }
}
