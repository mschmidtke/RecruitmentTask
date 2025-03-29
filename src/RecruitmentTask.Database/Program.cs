using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

using (var serviceProvider = CreateServices())
using (var scope = serviceProvider.CreateScope())
{
    UpdateDatabase(scope.ServiceProvider);
}

static ServiceProvider CreateServices()
{
    return new ServiceCollection()
        .AddFluentMigratorCore()
        .ConfigureRunner(rb => rb
            .AddSqlServer2014()
            .WithGlobalConnectionString("Data Source=localhost,14330;Initial Catalog=RecruitmentTask;User ID=sa;Password=To-jest-haslo-do-sqla-w-kontenerze-123;MultipleActiveResultSets=true;TrustServerCertificate=true;")
            .ScanIn(typeof(Program).Assembly).For.Migrations())
        .AddLogging(lb => lb.AddFluentMigratorConsole())
        .BuildServiceProvider(false);
}

static void UpdateDatabase(IServiceProvider serviceProvider)
{
    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

    runner.MigrateUp();
}