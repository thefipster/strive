Param(
    [Parameter(Position=0)]
    [string]$MigrationName
)

# fallback to positional args if invoked without a named parameter
if (-not $MigrationName -and $args.Count -gt 0) {
    $MigrationName = $args[0]
}

if (-not $MigrationName) {
    Write-Host "Error: Migration name not supplied." -ForegroundColor Red
    Write-Host ""
    Write-Host "Usage: .\add-queue-migration.ps1 <MigrationName>"
    Write-Host "Example: .\add-queue-migration.ps1 InitialCreate"
    exit 1
}

dotnet ef migrations add "$MigrationName" -p .\src\queue\Fip.Strive.Queue.Storage.Postgres -s .\src\harvest\Fip.Strive.Harvester.Web -c PostgresQueueContext