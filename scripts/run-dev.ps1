param(
    [string]$Environment = "Development"
)

# Set environment and run the API project (Windows PowerShell helper)
$env:ASPNETCORE_ENVIRONMENT = $Environment
Write-Host "Starting HueBuildStatus.Api with ASPNETCORE_ENVIRONMENT=$Environment"

# Run from repository root; the script resides in ./scripts
dotnet run --project ..\HueBuildStatus.Api\HueBuildStatus.Api.csproj