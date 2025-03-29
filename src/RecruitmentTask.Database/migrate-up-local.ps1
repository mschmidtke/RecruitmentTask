Param(
    [Parameter(Mandatory=$false)]
    $dbDataSource = "localhost,14330",
    [Parameter(Mandatory=$false)]
    $dbName = "RecruitmentTask",
    [Parameter(Mandatory=$false)]
    $dbUser = "sa",
    [Parameter(Mandatory=$false)]
    $dbPassword = "To-jest-haslo-do-sqla-w-kontenerze-123",	
    [Parameter(Mandatory=$false)]
    $createDbIfNotExists = $true
)

if ($createDbIfNotExists) {
	sqlcmd -S $dbDataSource -U $dbUser -P $dbPassword `
		-Q "If(db_id(N'$dbName') IS NULL) 
		BEGIN 
			PRINT 'Database not exists - creating...' 
			CREATE DATABASE [$dbName] 
			PRINT 'Database created' 
		END 
		ELSE 
			PRINT 'Database exists' "
}

dotnet run `
    --db:connectionstring "Data Source=$dbDataSource;Initial Catalog=$dbName;User ID=$dbUser;Password=$dbPassword;MultipleActiveResultSets=true;TrustServerCertificate=false;Encrypt=false"