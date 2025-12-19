# Mutation Testing (Stryker.NET)

This folder contains the mutation testing deliverables for the **FitnessBooking** project.

## Tool
- **Stryker.NET** (`dotnet-stryker`)

## How to reproduce
Run mutation testing from the UnitTests project directory:

```powershell
cd C:\dev\FitnessBooking\tests\FitnessBooking.UnitTests
dotnet stryker --solution "..\..\FitnessBooking.sln" --project "FitnessBooking.Application.csproj" --reporter "html" --log-to-file
