$ErrorActionPreference = "Stop"
Set-Location (Split-Path $PSScriptRoot -Parent)  # repo root

Remove-Item -Recurse -Force .\artifacts -ErrorAction SilentlyContinue
Get-ChildItem -Path .\tests -Recurse -Directory -Filter "TestResults" |
  Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Force .\artifacts\testresults | Out-Null

dotnet test .\FitnessBooking.sln `
  --settings .\coverlet.runsettings `
  --collect:"XPlat Code Coverage" `
  --results-directory .\artifacts\testresults

reportgenerator `
  -reports:".\artifacts\testresults\**\coverage.cobertura.xml" `
  -targetdir:".\artifacts\coverage" `
  -reporttypes:Html

Write-Host "Coverage report: artifacts\coverage\index.html"
