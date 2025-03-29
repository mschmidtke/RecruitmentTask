using FluentMigrator;

namespace RecruitmentTask.Database.Migrations
{
    [Migration(2025_03_26_10_57_15)]
    public class Migration0020: Migration
    {
        public override void Up()
        {
            Create.Table("Wallets").InSchema(DbSchemas.Default)
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(150).NotNullable().Unique()
                .WithColumn("Balance").AsString(int.MaxValue);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
