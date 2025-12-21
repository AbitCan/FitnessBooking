# Mutation Testing (Stryker) — FitnessBooking

## What is in this folder?
- `mutation-report.html` → the latest mutation report (HTML) for grading/submission.

## The result
- **Mutation score:** 95.35%
- Mutated module: `FitnessBooking.Application`
- Tests used: `FitnessBooking.UnitTests`

## How to run it again (reproduce)
From repo root:

```powershell
cd C:\dev\FitnessBooking\tests\FitnessBooking.UnitTests
dotnet stryker --solution "..\..\FitnessBooking.sln" --project "FitnessBooking.Application.csproj" --reporter "html" --log-to-file
