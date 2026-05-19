# CI Validation Guide

This document describes the GitHub Actions MVP CI pipeline for OpsSphere. CI runs on every pull request to `main`, every push to `main`, and on manual dispatch. It is a validation gate only — it does not deploy.

## Workflow Triggers

```yaml
on:
  pull_request:
    branches: [main]
  push:
    branches: [main]
  workflow_dispatch:
```

## Job Overview

| Job | Purpose | Docker | Secrets | Required for merge |
|---|---|---|---|---|
| `backend-fast` | Build backend; run domain and application tests | No | None | Recommended |
| `integration-fast` | Run integration/API regression suite (SQLite, no Docker) | No | None | Recommended |
| `integration-heavy` | Run EF Core migration smoke test against SQL Server via Testcontainers | Yes (ubuntu-latest runner) | None | Recommended when stable |
| `frontend` | Install, build, and test Angular frontend | No | None | Recommended |
| Manual E2E smoke | Validate critical UI workflows manually | N/A | N/A | See [docs/smoke-test-22.md](../smoke-test-22.md) |

---

## backend-fast

**Purpose:** Restore the .NET solution, build it, and run domain and application unit tests. Validates that backend dependencies resolve, the solution builds cleanly, and isolated layer tests pass.

**Runner:** `ubuntu-latest`

**Commands:**
```bash
dotnet restore OpsSphere.slnx
dotnet build OpsSphere.slnx --no-restore
dotnet test tests/OpsSphere.Domain.Tests --no-build
dotnet test tests/OpsSphere.Application.Tests --no-build
```

**Docker required:** No  
**Secrets required:** None  
**Database required:** None

---

## integration-fast

**Purpose:** Run the full integration and API regression suite, excluding the heavy Testcontainers migration test. All tests in this job use the `OpsSphereSqliteFactory`, which runs SQLite in-memory with fictional seed data.

**Runner:** `ubuntu-latest`

**Commands:**
```bash
dotnet restore OpsSphere.slnx
dotnet build OpsSphere.slnx --no-restore
dotnet test tests/OpsSphere.IntegrationTests --no-build --filter "Category!=Heavy"
```

**Docker required:** No  
**Secrets required:** None  
**Database:** SQLite in-memory (managed by `OpsSphereSqliteFactory`)

The `Category!=Heavy` filter excludes only `MigrationTests`, which requires Docker. Every other test in `OpsSphere.IntegrationTests` runs here, including authentication, authorization, scope filtering, ticket lifecycle, audit logs, dashboard, health checks, and middleware.

See [mvp-regression-tests.md](mvp-regression-tests.md) for a full description of what this suite covers.

---

## integration-heavy

**Purpose:** Validate that all EF Core migrations apply cleanly to a real SQL Server instance. This is the migration smoke test and the only test tagged `[Trait("Category", "Heavy")]`.

**Runner:** `ubuntu-latest`

**Commands:**
```bash
dotnet restore OpsSphere.slnx
dotnet build OpsSphere.slnx --no-restore
dotnet test tests/OpsSphere.IntegrationTests --no-build --filter "Category=Heavy"
```

**Docker required:** Yes — Testcontainers pulls and starts `mcr.microsoft.com/mssql/server:2022-latest` automatically. GitHub-hosted `ubuntu-latest` runners include Docker Engine and support Testcontainers without additional setup.

**SQL Server service container:** Not needed. Testcontainers manages the container lifecycle internally (`WithAutoRemove(true).WithCleanUp(true)`). Do not add a `services: mssql:` block.

**Secrets required:** None. Testcontainers generates its own internal connection string.

**No production database is used at any point.**

Expected duration: 3–5 minutes (includes container pull on first run and migration application).

---

## frontend

**Purpose:** Install frontend dependencies using the lock file, build the Angular application with production configuration, and run unit tests using ChromeHeadless.

**Runner:** `ubuntu-latest`  
**Working directory:** `frontend/`

**Commands:**
```bash
npm ci
npm run build
npm run test:ci
```

`npm run test:ci` maps to `ng test --watch=false --browsers=ChromeHeadless`. ChromeHeadless is available on `ubuntu-latest` without additional setup.

`npm run build` uses the Angular production configuration (default in `angular.json`).

`npm ci` requires `frontend/package-lock.json`, which is present in the repository. The Node.js setup step caches npm using `frontend/package-lock.json` as the cache key.

**Docker required:** No  
**Secrets required:** None  
**npm audit:** Not a blocking step in CI.

---

## E2E / Manual Smoke Strategy

No automated browser E2E tooling (Playwright, Cypress) is configured in this pipeline. Automated browser-level E2E tests are deferred to a future sprint.

Manual MVP smoke validation is documented in [docs/smoke-test-22.md](../smoke-test-22.md). That checklist covers authentication for all personas, admin/supervisor/agent/viewer workflows, dashboard/SLA/audit, customer flows, and error/access states.

---

## Recommended Required Checks Before Merge

Branch protection is configured through the GitHub repository UI, not through code.

**Recommended as required before merge:**
- `backend-fast`
- `integration-fast`
- `frontend`

**Recommended when stable:**
- `integration-heavy` — can be required or used as a release-gate signal depending on runner Docker availability and observed stability.

---

## Failure Triage

### Restore failures (`dotnet restore`)

- Check that `global.json` SDK version (`10.0.204`) is available on the runner via `actions/setup-dotnet@v4` with `dotnet-version: '10.x'`.
- Check that `Directory.Packages.props` package versions are resolvable from NuGet.
- Check for network connectivity issues in the CI log.

### Build failures (`dotnet build`)

- Read the full build error output in the Actions log.
- Look for missing project references, compilation errors, or package version conflicts.
- Reproduce locally: `dotnet build OpsSphere.slnx --no-restore` after a successful `dotnet restore`.

### Domain/Application test failures (`backend-fast`)

- These tests have no external dependencies. A failure here is a compilation or logic error.
- Run locally: `dotnet test tests/OpsSphere.Domain.Tests` and `dotnet test tests/OpsSphere.Application.Tests`.

### Integration-fast failures

- Tests use SQLite in-memory. A failure here typically indicates a handler, controller, authorization, or persistence logic regression.
- Run locally: `dotnet test tests/OpsSphere.IntegrationTests --filter "Category!=Heavy"`
- Note: `[assembly: CollectionBehavior(DisableTestParallelization = true)]` is set in `AssemblyInfo.cs` — tests run sequentially to avoid shared SQLite state conflicts.

### Integration-heavy Docker/Testcontainers failures

- Confirm Docker is available on the runner. GitHub-hosted `ubuntu-latest` includes Docker Engine.
- If the container fails to start, check for Docker daemon availability in the CI log.
- If migration application fails, check for pending or conflicting EF Core migrations.
- Run locally (requires Docker Desktop running): `dotnet test tests/OpsSphere.IntegrationTests --filter "Category=Heavy"`

### Frontend `npm ci` failures

- Check that `frontend/package-lock.json` is present and matches `package.json`.
- If `package-lock.json` is out of sync, run `npm install` locally, commit the updated lock file, and re-push.

### Frontend build failures

- Run locally: `cd frontend && npm run build`
- Check for Angular template compilation errors, TypeScript errors, or budget violations defined in `angular.json`.

### ChromeHeadless test failures

- Run locally: `cd frontend && npm run test:ci`
- Check the Karma test output for specific component, service, guard, or interceptor failures.
- ChromeHeadless is available on `ubuntu-latest` via the pre-installed Google Chrome binary.

---

## Docker and Testcontainers Notes

- Docker is required **only** for the `integration-heavy` job.
- GitHub-hosted `ubuntu-latest` runners include Docker Engine — no additional setup is needed.
- Testcontainers manages the SQL Server container lifecycle. It starts, provides the connection string, runs the test, and disposes the container.
- No SQL Server service container (`services: mssql:`) should be added to the workflow.
- No production database connection strings are used at any point in CI.

---

## Secrets and Log Safety

- No GitHub Secrets are required by any job in this workflow.
- All integration tests use fictional seed data and an in-memory or Testcontainers-managed database.
- The JWT signing key used in tests (`"integration-testing-only-fictional-jwt-signing-key"`) is a fictional string in source code, not a secret.
- Do not print environment variables, connection strings, tokens, or passwords in workflow steps.
- Test assertion output does not include sensitive values — this is validated by `AssertAuditAsync` and `AssertAuditDoesNotContainPiiAsync` in the test factory.

---

## Out of Scope

This workflow does not and should not include:

- Production deployment automation
- Azure App Service or Azure SQL deployment
- Staging deployment
- Docker image building or publishing
- Dependency vulnerability scanning
- Performance or load testing
- Security penetration testing
- Phase 2 or Phase 3 validation workflows
- Automated browser E2E tooling (Playwright, Cypress)
- Branch protection configuration (use GitHub UI)
