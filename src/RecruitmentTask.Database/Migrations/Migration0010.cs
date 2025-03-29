using FluentMigrator;

namespace RecruitmentTask.Database.Migrations
{
    [Migration(2025_03_24_21_36_40)]
    public class Migration0010: Migration
    {
        public override void Up()
        {
            Create.Table("ExchangesRates").InSchema(DbSchemas.Default)
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("QuotationDate").AsDate().NotNullable().Unique()
                .WithColumn("PublicationDate").AsDate().NotNullable()
                .WithColumn("Rates").AsString(int.MaxValue).NotNullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
