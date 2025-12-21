# tools/postman/run-newman.ps1
param(
  [string]$BaseUrl = "http://localhost:5039"
)

$ErrorActionPreference = "Stop"

$collection = Join-Path $PSScriptRoot "FitnessBooking.Api - v1.postman_collection.json"
$envFile    = Join-Path $PSScriptRoot "FitnessBooking Local.postman_environment.json"
$outEnv     = Join-Path $PSScriptRoot "after_run_environment.json"

if (!(Test-Path $collection)) { throw "Collection not found: $collection" }
if (!(Test-Path $envFile))    { throw "Environment not found: $envFile" }

Write-Host "Running Newman..."
Write-Host "BaseUrl: $BaseUrl"
Write-Host "Collection: $collection"
Write-Host "Environment: $envFile"

# Use newman.cmd to avoid PowerShell script execution policy issues (.ps1 shims)
newman.cmd run $collection `
  -e $envFile `
  --env-var "baseUrl=$BaseUrl" `
  --delay-request 200 `
  --export-environment $outEnv

# Pass through Newman's exit code (important for CI)
exit $LASTEXITCODE
