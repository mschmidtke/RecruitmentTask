using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using RecruitmentTask.Core.Wallets;
using RecruitmentTask.Infrastructure.Configuration;

namespace RecruitmentTask.Infrastructure.Wallets.Repositories
{
    public class WalletsRepository : IWalletsRepository
    {
        public async Task<int> SaveWallet(Wallet wallet)
        {
            const string sql = @"
                    MERGE INTO [dbo].[Wallets] AS Target
                    USING(VALUES(@name, @balance))
                        AS Source (NewName, NewBalance)
                    ON Target.Name = Source.NewName
                    WHEN MATCHED THEN
                        UPDATE SET
                            Target.[Name] = Source.NewName,
                            Target.[Balance] = Source.NewBalance
                    WHEN NOT MATCHED BY TARGET THEN
                        INSERT ([Name], [Balance])
                        VALUES (Source.NewName, Source.NewBalance)
                    OUTPUT inserted.Id;
                    ";

            using var sqlConnection = new SqlConnection(DbConfig.DbConnection);

            var balanceJson = JsonConvert.SerializeObject(wallet.Balances);

            var id = await sqlConnection.QuerySingleAsync<int>(sql,
                new
                {
                    name = wallet.Name,
                    balance = balanceJson
                });
            
            return id;
        }

        public async Task<Wallet?> GetWallet(string name)
        {
            const string sql =
                "SELECT Name,Balance FROM dbo.Wallets WHERE Name = @name";

            using var sqlConnection = new SqlConnection(DbConfig.DbConnection);

            var result = await sqlConnection.QuerySingleOrDefaultAsync(sql, new { name });

            if (result == null)
            {
                return null;
            }

            var balanceDtos = JsonConvert.DeserializeObject<IEnumerable<BalanceDto>>(result.Balance) as IEnumerable<BalanceDto>;

            return Wallet.RecoverFrom(result.Name,
                balanceDtos.Select(dto => Balance.Create(dto.CurrencyCode, dto.Amount)).ToList());

        }

        public async Task<Wallet?> GetWallet(int id)
        {
            const string sql =
                "SELECT Name, Balance FROM dbo.Wallets WHERE Id = @id";

            using var sqlConnection = new SqlConnection(DbConfig.DbConnection);

            var result = await sqlConnection.QuerySingleOrDefaultAsync(sql, new { id });

            if (result == null)
            {
                return null;
            }

            var balancesDto = JsonConvert.DeserializeObject<IEnumerable<BalanceDto>>(result.Balance) as IEnumerable<BalanceDto>;

            return Wallet.RecoverFrom(result.Name,
                balancesDto.Select(dto => Balance.Create(dto.CurrencyCode, dto.Amount)).ToList());
        }

        public async Task<IEnumerable<Wallet>> GetWallets(int offset, short take)
        {
            var wallets = new List<Wallet>();

            const string sql =
                @"SELECT Id, Name, Balance FROM dbo.Wallets
                    ORDER BY Name Asc
                    OFFSET @offset ROWS
                    FETCH NEXT @take ROWS ONLY";

            using var sqlConnection = new SqlConnection(DbConfig.DbConnection);

            var results = (await sqlConnection.QueryAsync(sql, new { offset, take })).ToList();

            if (!results.Any())
            {
                return wallets;
            }

            foreach (var wallet in results)
            {

                var balancesDto =
                    JsonConvert.DeserializeObject<IEnumerable<BalanceDto>>(wallet.Balance) as IEnumerable<BalanceDto>;

                wallets.Add(Wallet.RecoverFrom(wallet.Name,
                    balancesDto.Select(dto => Balance.Create(dto.CurrencyCode, dto.Amount)).ToList()));
            }

            return wallets;
        }
    }
}
