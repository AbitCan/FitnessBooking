# FitnessBooking

A small ASP.NET Core Web API for booking fitness classes.  
It includes **dynamic pricing** for reservations, a **refund policy** for cancellations, and a full testing toolchain (unit/integration/property tests + Newman + coverage + mutation + k6 + ZAP).

---

## Project structure

- `src/FitnessBooking.Api` — Web API (controllers + hosting)
- `src/FitnessBooking.Application` — business logic (pricing, refund, reservation rules)
- `src/FitnessBooking.Domain` — domain models/enums
- `src/FitnessBooking.Infrastructure` — in-memory repositories (persistence layer)
- `tests/FitnessBooking.UnitTests` — unit tests
- `tests/FitnessBooking.IntegrationTests` — integration/API tests
- `tests/FitnessBooking.PropertyTests` — property-based tests
- `tools/postman` — Postman collection + environment for Newman
- `tools/performance` — k6 performance script
- `.github/workflows` — CI/CD + security + mutation workflows

---

## Run the project (local)

### Prerequisites
- .NET SDK (use the version pinned in `global.json` if present)

### Start the API
From the repo root:

```bash
dotnet restore
dotnet run --project src/FitnessBooking.Api --urls http://localhost:5039
If your project uses a different port locally, just replace 5039.


How to execute tests
Run all tests:
dotnet test

Run unit tests only:
dotnet test tests/FitnessBooking.UnitTests/FitnessBooking.UnitTests.csproj

Run integration tests only:
dotnet test tests/FitnessBooking.IntegrationTests/FitnessBooking.IntegrationTests.csproj

Run property-based tests only:
dotnet test tests/FitnessBooking.PropertyTests/FitnessBooking.PropertyTests.csproj

Coverage report (Coverlet + ReportGenerator)

Generate coverage (Cobertura) and build a merged HTML report:

dotnet test -c Release --no-build \
  --collect:"XPlat Code Coverage" \
  --settings coverlet.runsettings

reportgenerator \
  "-reports:**/coverage.cobertura.xml" \
  "-targetdir:TestResults/CoverageReport" \
  "-reporttypes:HtmlInline;Cobertura"

Open:
TestResults/CoverageReport/index.html
Mutation testing (Stryker.NET)

Mutation config is in stryker-config.json (repo root).
Run Stryker from the UnitTests folder so paths resolve cleanly:

cd tests/FitnessBooking.UnitTests
dotnet tool update -g dotnet-stryker
dotnet stryker --config-file ../../stryker-config.json --log-to-file


Report output is under:
tests/FitnessBooking.UnitTests/StrykerOutput/**/reports/**
Postman/Newman API tests
Install Newman
npm install -g newman

Run collection against local API

Make sure the API is running on http://localhost:5039, then:

newman run "tools/postman/FitnessBooking.Api - v1.postman_collection.json" \
  -e "tools/postman/FitnessBooking Local.postman_environment.json" \
  --env-var "baseUrl=http://localhost:5039"

Performance test (k6)

Run k6 against local API:

k6 run -e BASE_URL=http://localhost:5039 tools/performance/k6-e2e.js

Security testing (OWASP ZAP Baseline)

ZAP baseline scanning is automated via GitHub Actions workflow (see .github/workflows).
Run the workflow from GitHub Actions to generate the report artifact.

Docker
Dockerfile

src/FitnessBooking.Api/Dockerfile

Build & run locally

From repo root:

docker build -t fitnessbooking-api -f src/FitnessBooking.Api/Dockerfile .
docker run --rm -p 5039:5039 fitnessbooking-api


If your container listens on a different internal port, update the mapping accordingly.

CI/CD (GitHub Actions)

Workflows live in:

.github/workflows/ci.yml (build + tests + coverage + Newman + k6)

.github/workflows/mutation-stryker.yml (Stryker mutation tests)

.github/workflows/zap.yml (ZAP baseline)

.github/workflows/cd.yml (build & push Docker image to GHCR)

Artifacts produced typically include:

Coverage report (HTML)

Newman JUnit report

k6 summary JSON

Stryker HTML report

ZAP report

## Tools & versions used

- **.NET SDK**: 10.0.101
- **Target framework**: .NET 10.0 (`net10.0`)
- **ASP.NET Core**: .NET 10.0 (via the SDK above)
- **Test framework**: NUnit
- **Mocking**: Moq
- **Coverage**: XPlat Code Coverage (coverlet collector) + ReportGenerator
- **Mutation testing**: Stryker.NET
- **API tests**: Postman + Newman
- **Performance**: k6
- **Security**: OWASP ZAP Baseline
- **CI/CD**: GitHub Actions
- **Containerization**: Docker + GHCR
If you want it even tighter/verified, you can also add a quick “check versions” snippet:

md
Copy code
### Verify versions locally

```bash
dotnet --version
dotnet --list-sdks
sql
Copy code

Then commit + push again:

```bash
git add README.md
git commit -m "docs: update README with .NET 10.0.101"
git push
