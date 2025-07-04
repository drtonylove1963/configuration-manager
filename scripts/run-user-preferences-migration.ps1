# PowerShell script to run the user preferences migration
# This script applies the EF Core migration and then runs the data migration

param(
    [string]$ConnectionString = "Server=localhost,1434;Database=ConfigurationManagerTest;User Id=sa;Password=ConfigManager123!;TrustServerCertificate=true;Encrypt=false;Connection Timeout=60;Command Timeout=60;Pooling=false;"
)

Write-Host "Starting User Preferences Migration..." -ForegroundColor Green

try {
    # Step 1: Apply EF Core migration to create tables
    Write-Host "Step 1: Applying EF Core migration..." -ForegroundColor Yellow

    Set-Location $PSScriptRoot\..

    $migrationResult = dotnet ef database update --project src/Infrastructure --startup-project src/Api 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ EF Core migration applied successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ EF Core migration failed:" -ForegroundColor Red
        Write-Host $migrationResult -ForegroundColor Red
        exit 1
    }

    # Step 2: Run data migration script
    Write-Host "Step 2: Running data migration script..." -ForegroundColor Yellow

    $scriptPath = Join-Path $PSScriptRoot "migrate-user-preferences.sql"

    if (Test-Path $scriptPath) {
        # Use sqlcmd to run the migration script
        $sqlcmdResult = sqlcmd -S "localhost,1434" -d "ConfigurationManagerTest" -U "sa" -P "ConfigManager123!" -i $scriptPath 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Data migration completed successfully" -ForegroundColor Green
            Write-Host $sqlcmdResult -ForegroundColor Cyan
        } else {
            Write-Host "✗ Data migration failed:" -ForegroundColor Red
            Write-Host $sqlcmdResult -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "✗ Migration script not found: $scriptPath" -ForegroundColor Red
        exit 1
    }

    Write-Host "🎉 User Preferences Migration completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Update your application to use the new UserSettings table" -ForegroundColor White
    Write-Host "2. Test the theme preferences functionality" -ForegroundColor White
    Write-Host "3. Implement proper user authentication" -ForegroundColor White

} catch {
    Write-Host "✗ Migration failed with error:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}