# OpsSphere DevOps Context

## Purpose

This document provides compressed DevOps context for agents working inside OpsSphere.

Use this file when the task touches:

- Local development setup
- Docker
- Docker Compose
- GitHub workflow
- GitHub Actions
- CI/CD
- Environment configuration
- Secrets
- Deployment validation
- Azure readiness

---

## DevOps Goals

OpsSphere DevOps should support:

```text
Easy local setup
Repeatable infrastructure
Automated build validation
Automated test validation
Dockerized local dependencies
GitHub Actions CI
Azure-ready deployment
Safe secrets management
Separate environments
Professional delivery workflow
```

---

## Environment Strategy

OpsSphere uses three environment categories:

```text
Local Development
  → Used by developers to build and test features.

Staging
  → Used to validate integrated changes before production deployment.

Production
  → Represents the live operational environment.
```

Each environment should have separate:

```text
Configuration
Database
Secrets
Deployment rules
Monitoring
Validation checklist
```

---

## Local Development Expectations

A developer should be able to:

```text
Clone repository
  → Restore dependencies
  → Start local infrastructure
  → Apply database migrations
  → Run backend API
  → Run frontend application
  → Execute tests
  → Validate API through Swagger
```

Expected command family:

```text
git clone <repository-url>
cd OpsSphere

docker compose up -d

dotnet restore
dotnet build
dotnet test

cd frontend
npm install
npm run start
npm run test
```

Actual commands may evolve with implementation.

---

## Docker Strategy

Initial Docker Compose scope:

```text
SQL Server database
Optional backend API container
Optional frontend container
```

The first implementation may start with only SQL Server in Docker Compose and run backend/frontend directly from the developer machine.

Docker Compose should support:

```text
Repeatable local database startup
Stable connection strings
Easy local reset
Consistent developer onboarding
Future full-stack container execution
```

Docker Compose does not need to provide:

```text
Production-grade orchestration
Kubernetes
Horizontal scaling
Advanced networking
Production monitoring
Production secret storage
```

---

## Expected Files

Possible files:

```text
docker-compose.yml
.env.example

src/OpsSphere.Api/Dockerfile
frontend/Dockerfile

.github/workflows/ci.yml
.github/workflows/deploy-staging.yml
.github/workflows/deploy-production.yml
```

Deployment workflows may be introduced later.

---

## Git Workflow

Use issues, branches, pull requests, and validation.

Recommended branch names:

```text
docs/issue-6-delivery-planning
feature/issue-10-ticket-creation
fix/issue-15-auth-validation
chore/issue-20-ci-pipeline
```

Commit examples:

```text
docs: add deployment and devops strategy
feat: add ticket creation command
test: add ticket workflow unit tests
ci: add backend build pipeline
fix: enforce ticket closure rule
```

---

## Pull Request Expectations

Each pull request should:

```text
Reference the related issue.
Summarize what changed.
List added or changed files.
Include validation performed.
Close the issue when complete.
```

Recommended PR template:

```markdown
## Summary

Briefly describe what this PR adds or changes.

## Added / Changed

- 

## Validation

- [ ] Reviewed changed files.
- [ ] Confirmed scope matches the related issue.
- [ ] Ran relevant tests.

## Related Issue

Closes #
```

---

## GitHub Actions CI

CI should run on:

```text
Pull requests to main
Pushes to main
Manual workflow dispatch when needed
```

Recommended trigger:

```yaml
on:
  pull_request:
    branches:
      - main
  push:
    branches:
      - main
  workflow_dispatch:
```

Initial CI responsibilities:

```text
Checkout repository
Setup .NET
Restore backend dependencies
Build backend
Run backend tests
Setup Node.js
Install frontend dependencies
Build frontend
Run frontend tests
```

Future CI improvements:

```text
Run integration tests with SQL Server service container
Run API tests
Run E2E smoke tests
Build Docker images
Scan dependencies
Publish test results
Upload build artifacts
Deploy to staging
```

---

## CI Quality Gate

A pull request should not be merged if:

```text
Backend build fails.
Backend tests fail.
Frontend build fails.
Frontend tests fail.
Required integration tests fail.
The change does not match the related issue scope.
```

---

## Secrets Management

Sensitive configuration includes:

```text
Connection strings
JWT signing keys
Admin credentials
Cloud credentials
API keys
Storage keys
Email provider keys
```

Rules:

```text
Do not commit secrets to source control.
Use .env.example for placeholders.
Use .NET User Secrets for local sensitive development values when needed.
Use GitHub Secrets for CI/CD.
Use Azure Key Vault for hosted environments.
Use environment variables for deployment configuration.
Do not store production secrets in appsettings files.
```

---

## Deployment Security Principles

```text
Use HTTPS in hosted environments.
Do not expose secrets in source control.
Do not expose internal stack traces to users.
Use least privilege for cloud resources.
Restrict production deployment permissions.
Use separate staging and production configuration.
Validate authentication and authorization before release.
```

---

## Azure Readiness

OpsSphere should be prepared for:

```text
Azure App Service
Azure SQL Database
Azure Key Vault
Application Insights
GitHub Actions deployment pipelines
```

MVP does not need full production deployment immediately, but architecture should remain compatible with Azure deployment.

---

## Deployment Validation

Deployment validation should include:

```text
Deploy application
  → Call /health
  → Confirm database connectivity
  → Run smoke tests
  → Review logs
  → Validate authentication
  → Validate authorization
  → Validate critical workflow
```

Smoke tests may include:

```text
API is reachable.
Health endpoint is healthy.
Database is reachable.
Login works.
Protected endpoint rejects unauthenticated request.
Ticket list endpoint responds for authorized user.
Frontend loads.
```

---

## DevOps Risks to Watch

```text
Secrets committed to repository.
Local setup becomes too complex.
CI pipeline is missing.
Tests are not run before merge.
Staging differs too much from production.
Database migrations break deployment.
Frontend and backend versions mismatch.
Logs are insufficient after deployment.
Production secrets stored in appsettings.
Manual deployment steps are undocumented.
```

---

## DevOps Definition of Done

DevOps-related work is done when:

```text
Local setup is documented.
Commands are repeatable.
Secrets are not committed.
Docker Compose works or limitations are documented.
CI workflow validates build and tests.
Deployment assumptions are documented.
Health checks or smoke tests are considered.
```