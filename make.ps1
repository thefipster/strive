param (
    [Parameter(Mandatory = $true)]
    [ValidateSet("build", "run", "test", "clean")]
    [string]$Command
)

# Stop execution on all errors
$ErrorActionPreference = 'Stop'

# Don't display progress UI (some commands show progress bars that clutter logs)
$ProgressPreference = 'SilentlyContinue'

function Build-Strive {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Args
    )

    Write-Host "Building"
    dotnet build strive.sln --configuration Release
}

function Run-Strive {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Args
    )
    Write-Host "Starting"
    # Via the Aspire AppHost, so Postgres comes up with the app. To run the web app on its own,
    # set ConnectionStrings__strive and start src/Fip.Strive.Web directly.
    dotnet run --project src/Fip.Strive.AppHost --configuration Release
}

function Clean-Strive {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Args
    )
    Write-Host "Cleaning"
    dotnet clean strive.sln
}

function Test-Strive {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Args
    )
    Write-Host "Testing"
    dotnet test strive.sln --collect:"XPlat Code Coverage"
    reportgenerator -reports:**/coverage*.xml -targetdir:logs/coverage -reporttypes:Html
    Start-Process (Join-Path $PSScriptRoot 'logs\coverage\index.html')
}

switch ($Command) {
    "build" {
        Build-Strive -Args "."
    }
    "run" {
        Run-Strive -Args "."
    }
    "test" {
        Test-Strive -Args "."
    }
    "clean" {
        Clean-Strive -Args "."
    }
}