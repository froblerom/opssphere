# Deployment and DevOps

## Document Information

| Field | Value |
|---|---|
| Project | OpsSphere |
| Document | Deployment and DevOps |
| File | `docs/18-deployment-and-devops.md` |
| Version | 1.0 |
| Status | Draft |
| Project Type | Enterprise Support Operations Platform |
| Related Issue | #6 |
| Phase | Delivery Planning |

---

## 1. Purpose

This document defines the initial deployment and DevOps strategy for OpsSphere.

The purpose of this document is to describe how the project will be built, tested, configured, containerized, deployed, and prepared for cloud hosting.

OpsSphere is planned as a maintainable, testable, deployable, and supportable enterprise application. For that reason, deployment and DevOps planning must be defined before the implementation phase advances too far.

This document covers:

- Local development environment.
- Docker and Docker Compose strategy.
- GitHub workflow expectations.
- GitHub Actions CI pipeline.
- Staging environment.
- Production environment.
- Azure-ready deployment target.
- Secrets management strategy.
- Build and test pipeline.
- Deployment validation.
- Release management expectations.

---

## 2. Deployment and DevOps Goals

The main goals of the OpsSphere deployment and DevOps strategy are:

1. Make the project easy to run locally.
2. Keep local development close to real deployment conditions.
3. Automate build and test validation.
4. Use Docker for repeatable local infrastructure.
5. Use GitHub Actions for continuous integration.
6. Prepare the application for Azure deployment.
7. Keep secrets out of source control.
8. Support separate environments for local, staging, and production.
9. Validate quality before merging or deploying.
10. Create a professional delivery workflow suitable for an enterprise-grade portfolio project.

---

## 3. Environment Strategy

OpsSphere will use three main environment categories:

```text
Local Development
  → Used by developers to build and test features.

Staging
  → Used to validate integrated changes before production deployment.

Production
  → Represents the live operational environment.
```

Each environment should have its own configuration, database, secrets, and deployment rules.

---

# 4. Local Development Environment

## 4.1 Purpose

The local environment allows developers to run OpsSphere on their machine with minimal manual setup.

The local environment should support:

- Backend development.
- Frontend development.
- SQL Server database.
- API testing.
- Database migrations.
- Automated tests.
- Local debugging.
- Dockerized infrastructure.

## 4.2 Local Technology Stack

| Area | Technology |
|---|---|
| Backend | .NET 8 / ASP.NET Core Web API |
| Frontend | Angular / TypeScript |
| Database | SQL Server |
| ORM | Entity Framework Core |
| Containers | Docker |
| Local Orchestration | Docker Compose |
| API Documentation | Swagger / OpenAPI |
| Testing | xUnit, WebApplicationFactory, Testcontainers |
| Logging | Serilog |

## 4.3 Local Development Expectations

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

## 4.4 Expected Local Commands

Final commands may change during implementation, but the intended workflow is:

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

## 4.5 Local Configuration Files

Expected local configuration files may include:

```text
src/
  OpsSphere.Api/
    appsettings.json
    appsettings.Development.json

frontend/
  src/
    environments/
      environment.ts
      environment.development.ts

docker-compose.yml
.env.example
```

## 4.6 Local Database Strategy

The local database should run through Docker Compose.

Recommended local database service:

```text
SQL Server container
```

The local database should support:

- EF Core migrations.
- Seed data.
- Local manual testing.
- Integration testing when needed.
- Easy reset.

Example local database flow:

```text
Start SQL Server container
  → Apply EF Core migrations
  → Seed fictional test data
  → Run API locally
```

## 4.7 Local Sample Data

Local development should use fictional sample data only.

Example seed data:

```text
Region: Latin America
Countries: Mexico, Colombia, Costa Rica
Accounts: NovaBank, Streamly, Shopora
Campaigns: Credit Card Support, Fraud Review Support, Creator Support
Roles: Admin, OperationsManager, Supervisor, Agent, Viewer
```

No real company, client, employee, or customer data should be used.

---

# 5. Docker and Docker Compose Strategy

## 5.1 Purpose

Docker is used to make local infrastructure repeatable and consistent.

Docker Compose should allow the main development dependencies to start with a simple command.

## 5.2 Initial Docker Compose Scope

The initial Docker Compose setup should include:

```text
SQL Server database
Optional backend API container
Optional frontend container
```

The first implementation may start with only SQL Server in Docker Compose and run backend/frontend directly from the developer machine.

A later version may containerize the full application stack.

## 5.3 Recommended Docker Compose Services

```text
services:
  sqlserver:
    image: SQL Server
    purpose: Local database

  api:
    image: OpsSphere API
    purpose: Backend API container
    status: Optional for initial development

  frontend:
    image: OpsSphere Angular app
    purpose: Frontend container
    status: Optional for initial development
```

## 5.4 Docker Compose Goals

Docker Compose should support:

- Repeatable local database startup.
- Stable connection strings.
- Easy local reset.
- Consistent developer onboarding.
- Future full-stack container execution.

## 5.5 Docker Compose Non-Goals

The initial Docker Compose setup does not need to provide:

- Production-grade orchestration.
- Kubernetes.
- Horizontal scaling.
- Advanced networking.
- Production monitoring.
- Production secret storage.

Those capabilities are outside the initial delivery planning scope.

## 5.6 Dockerfile Strategy

The project may include Dockerfiles for:

```text
Backend API
Frontend Angular application
```

Expected files:

```text
src/OpsSphere.Api/Dockerfile
frontend/Dockerfile
docker-compose.yml
```

The backend Dockerfile should:

- Restore dependencies.
- Build the API.
- Publish the API.
- Run the published output.

The frontend Dockerfile may:

- Install dependencies.
- Build Angular.
- Serve static files through a web server if containerized.

---

# 6. Git Workflow Strategy

## 6.1 Purpose

OpsSphere will use a simple GitHub workflow based on issues, branches, pull requests, and review checklists.

The goal is to keep development organized and traceable without adding unnecessary process overhead.

## 6.2 Branching Strategy

Recommended branch naming:

```text
docs/issue-6-delivery-planning
feature/issue-<number>-short-description
fix/issue-<number>-short-description
chore/issue-<number>-short-description
```

Examples:

```text
docs/issue-6-delivery-planning
feature/issue-10-ticket-creation
fix/issue-15-auth-validation
chore/issue-20-ci-pipeline
```

## 6.3 Pull Request Strategy

Each pull request should:

- Reference the related issue.
- Summarize what changed.
- List added or changed files.
- Include validation performed.
- Close the issue when complete.

Recommended PR template:

```text
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

## 6.4 Commit Strategy

Commit messages should be clear and scoped.

Examples:

```text
docs: add deployment and devops strategy
docs: add testing strategy
feat: add ticket creation command
test: add ticket workflow unit tests
ci: add backend build pipeline
fix: enforce ticket closure rule
```

## 6.5 Main Branch Protection

When configured, the main branch should require:

```text
Pull request review
Passing CI checks
No direct commits to main
Up-to-date branch before merge
```

For early portfolio development, this may be applied gradually.

---

# 7. GitHub Actions Strategy

## 7.1 Purpose

GitHub Actions will provide continuous integration for OpsSphere.

The CI pipeline should automatically validate changes before they are merged.

## 7.2 CI Pipeline Triggers

The pipeline should run on:

```text
Pull requests to main
Pushes to main
Manual workflow dispatch when needed
```

Recommended triggers:

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

## 7.3 CI Pipeline Responsibilities

The CI pipeline should validate:

- Backend dependency restore.
- Backend build.
- Backend unit tests.
- Backend integration tests when available.
- Frontend dependency install.
- Frontend build.
- Frontend tests.
- Optional linting.
- Optional API contract checks.
- Optional Docker image build.

## 7.4 Initial CI Pipeline Stages

```text
Checkout repository
  → Setup .NET
  → Restore backend dependencies
  → Build backend
  → Run backend tests
  → Setup Node.js
  → Install frontend dependencies
  → Build frontend
  → Run frontend tests
```

## 7.5 Future CI Pipeline Stages

Future CI improvements may include:

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

## 7.6 Example CI Pipeline Structure

```text
.github/
  workflows/
    ci.yml
    deploy-staging.yml
    deploy-production.yml
```

Initial workflow:

```text
ci.yml
```

Future deployment workflows:

```text
deploy-staging.yml
deploy-production.yml
```

## 7.7 CI Quality Gate

A pull request should not be merged if:

- Backend build fails.
- Backend tests fail.
- Frontend build fails.
- Frontend tests fail.
- Required integration tests fail.
- The change does not match the related issue scope.

---

# 8. Build and Test Pipeline

## 8.1 Purpose

The build and test pipeline ensures that OpsSphere remains stable as changes are added.

## 8.2 Backend Build Pipeline

Backend pipeline steps:

```text
Restore .NET dependencies
  → Build solution
  → Run unit tests
  → Run integration tests
  → Publish API artifact when needed
```

Expected commands:

```text
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
dotnet publish --configuration Release
```

## 8.3 Frontend Build Pipeline

Frontend pipeline steps:

```text
Install Node dependencies
  → Run frontend tests
  → Build Angular app
  → Publish frontend artifact when needed
```

Expected commands:

```text
npm ci
npm run test
npm run build
```

## 8.4 Integration Test Pipeline

Integration tests may require SQL Server.

Recommended approach:

```text
GitHub Actions SQL Server service container
or
Testcontainers-managed SQL Server container
```

Integration tests should:

- Apply migrations.
- Seed fictional test data.
- Run workflow validations.
- Dispose test database.

## 8.5 E2E Pipeline

E2E tests may be added after the frontend and backend are stable.

Recommended E2E approach:

```text
Start backend
Start frontend
Start test database
Seed data
Run Playwright tests
Upload traces or screenshots only when tests fail
```

E2E tests should be limited to critical journeys to avoid slowing down the pipeline.

---

# 9. Staging Environment

## 9.1 Purpose

The staging environment is used to validate release candidates before production deployment.

Staging should be as close to production as practical while remaining safe for testing.

## 9.2 Staging Environment Scope

Staging should include:

- Deployed backend API.
- Deployed frontend application.
- Staging database.
- Staging configuration.
- Staging secrets.
- Application logs.
- Health checks.
- Test users and fictional data.

## 9.3 Staging Deployment Goals

Staging should support:

- Manual validation.
- UAT scenarios.
- API verification.
- Smoke testing.
- Deployment rehearsal.
- Observability validation.
- Configuration validation.

## 9.4 Staging Data Strategy

Staging must not use real confidential data.

Allowed data:

```text
Fictional users
Fictional accounts
Fictional customers
Fictional tickets
Fictional SLA states
Fictional audit records
```

## 9.5 Staging Validation Checklist

Before promoting to production, staging should validate:

```text
Application starts successfully
Frontend loads successfully
API health check passes
Database connection works
Login works
Ticket creation works
Ticket assignment works
Ticket escalation works
SLA dashboard loads
Audit records are created
Logs are being produced
No secrets are exposed
```

---

# 10. Production Environment

## 10.1 Purpose

The production environment represents the live environment where OpsSphere would be used by real internal users.

For the portfolio version, production may be simulated or Azure-ready rather than fully operated with real users.

## 10.2 Production Expectations

Production should be prepared for:

- Secure configuration.
- HTTPS access.
- Managed database.
- Centralized logging.
- Health checks.
- Secrets management.
- Deployment rollback planning.
- Monitoring and alerting.
- Backup strategy.
- Least privilege access.

## 10.3 Production Non-Goals for Initial Version

The initial production-ready plan does not require:

- Kubernetes.
- Multi-region deployment.
- High availability architecture.
- Enterprise SSO.
- Advanced autoscaling.
- Blue-green deployment.
- Full disaster recovery automation.

These may be considered in future phases.

## 10.4 Production Readiness Checklist

A production deployment should not be considered ready until:

```text
CI pipeline passes
Staging validation passes
Secrets are stored outside source control
Database migrations are reviewed
Health checks are enabled
Structured logging is enabled
Application Insights is configured or planned
HTTPS is enforced
Authentication and authorization are validated
Audit logging is validated
Rollback process is documented
```

---

# 11. Azure Deployment Target

## 11.1 Purpose

OpsSphere is designed to be Azure-ready.

The initial deployment strategy should align with Azure services commonly used for enterprise .NET applications.

## 11.2 Target Azure Services

| Area | Azure Service |
|---|---|
| Backend Hosting | Azure App Service |
| Frontend Hosting | Azure Static Web Apps or Azure App Service |
| Database | Azure SQL Database |
| Secrets | Azure Key Vault |
| Monitoring | Application Insights |
| Logs and Metrics | Azure Monitor |
| File Storage, future | Azure Blob Storage |
| CI/CD | GitHub Actions |

## 11.3 Initial Azure Architecture

```text
User Browser
  → Angular Frontend
  → ASP.NET Core Web API on Azure App Service
  → Azure SQL Database

ASP.NET Core Web API
  → Azure Key Vault for secrets
  → Application Insights for telemetry
```

## 11.4 Backend Deployment Target

The backend API should be deployable to:

```text
Azure App Service
```

The API deployment should include:

- Published .NET application.
- Environment-specific configuration.
- Connection string from secure configuration.
- JWT settings from secure configuration.
- Application Insights instrumentation key or connection string.
- Health check endpoint.

## 11.5 Frontend Deployment Target

The Angular frontend may be deployed to:

```text
Azure Static Web Apps
```

or:

```text
Azure App Service
```

Azure Static Web Apps may be preferred for a simple frontend deployment.

The frontend deployment should include:

- Production Angular build.
- Environment-specific API base URL.
- HTTPS.
- Static asset hosting.

## 11.6 Database Deployment Target

The database should be deployable to:

```text
Azure SQL Database
```

Database deployment should include:

- EF Core migrations.
- Schema version control.
- Controlled migration execution.
- No real data in repository.
- Backup and restore considerations for production.

## 11.7 Azure Deployment Phases

Recommended deployment phases:

```text
Phase 1:
  - Local Docker Compose
  - GitHub Actions CI

Phase 2:
  - Manual Azure deployment
  - Azure App Service
  - Azure SQL

Phase 3:
  - Automated staging deployment
  - Application Insights
  - Azure Key Vault

Phase 4:
  - Automated production deployment
  - Release approvals
  - Rollback process
```

---

# 12. Secrets Management Strategy

## 12.1 Purpose

Secrets management prevents sensitive configuration from being committed to source control.

Secrets include:

- Database passwords.
- Connection strings.
- JWT signing keys.
- API keys.
- Azure credentials.
- Application Insights connection strings.
- Storage account keys.
- Email provider keys in future phases.

## 12.2 Secrets Rules

OpsSphere should follow these rules:

```text
Never commit secrets to Git.
Never store production secrets in appsettings.json.
Use local secret storage for development.
Use GitHub Actions secrets for CI/CD.
Use Azure Key Vault for Azure-hosted environments.
Use .env.example to document required variables without values.
Rotate secrets if exposure is suspected.
```

## 12.3 Local Secrets Strategy

For local development, secrets may be stored using:

```text
.NET User Secrets
.env file for Docker Compose
Local environment variables
```

Example files:

```text
.env
.env.example
```

`.env` should be ignored by Git.

`.env.example` may be committed to document expected values:

```text
SQLSERVER_SA_PASSWORD=<local-password>
OPSSPHERE_DB_CONNECTION=<connection-string>
JWT_SIGNING_KEY=<local-dev-key>
```

## 12.4 GitHub Actions Secrets

GitHub Actions should use repository or environment secrets.

Examples:

```text
AZURE_CREDENTIALS
AZURE_WEBAPP_NAME
AZURE_RESOURCE_GROUP
AZURE_SQL_CONNECTION_STRING
JWT_SIGNING_KEY
APPLICATIONINSIGHTS_CONNECTION_STRING
```

Secrets should not be printed in logs.

## 12.5 Azure Key Vault Strategy

For Azure environments, secrets should be stored in:

```text
Azure Key Vault
```

Azure Key Vault may store:

```text
Database connection string
JWT signing key
Application Insights connection string
Email provider API key
Storage account connection string
```

Azure App Service should access Key Vault through managed identity when possible.

## 12.6 Configuration Separation

Configuration should be separated by environment:

```text
Development
Staging
Production
```

Recommended configuration files:

```text
appsettings.json
appsettings.Development.json
appsettings.Staging.json
appsettings.Production.json
```

Sensitive values should not be stored directly in these files.

---

# 13. Database Migration Strategy

## 13.1 Purpose

Database migrations allow schema changes to be tracked and applied consistently.

OpsSphere will use:

```text
Entity Framework Core migrations
```

## 13.2 Migration Rules

Migration rules:

```text
Migrations should be reviewed before deployment.
Migrations should be tested locally.
Migrations should be tested in staging before production.
Production migration execution should be controlled.
Data-destructive migrations require extra review.
Seed data should be fictional and environment-aware.
```

## 13.3 Migration Flow

Recommended migration flow:

```text
Developer creates migration
  → Runs migration locally
  → Runs tests
  → Opens PR
  → CI validates build and tests
  → Migration is applied to staging
  → Staging validation passes
  → Migration is approved for production
```

## 13.4 Production Migration Caution

Production migrations should avoid:

- Dropping columns without backup.
- Renaming critical columns without migration plan.
- Deleting operational history.
- Breaking audit records.
- Breaking reporting views.
- Running unreviewed migration scripts.

---

# 14. Deployment Pipeline Strategy

## 14.1 Initial Deployment Strategy

The initial project may start with manual deployment steps documented clearly.

As the project matures, deployment should become automated through GitHub Actions.

## 14.2 Staging Deployment Pipeline

A future staging deployment pipeline may run when changes are merged to main.

```text
Push to main
  → Run CI
  → Build backend artifact
  → Build frontend artifact
  → Deploy backend to Azure App Service staging
  → Deploy frontend to Azure Static Web Apps or App Service
  → Apply staging migrations
  → Run smoke tests
```

## 14.3 Production Deployment Pipeline

Production deployment should require manual approval.

```text
Create release
  → Run CI
  → Deploy to staging
  → Validate staging
  → Manual approval
  → Deploy to production
  → Run production smoke checks
  → Monitor logs and health checks
```

## 14.4 Deployment Artifacts

Deployment artifacts may include:

```text
Published backend API
Built Angular frontend
Database migration bundle or migration scripts
Docker image, if containerized
Release notes
```

---

# 15. Release Management

## 15.1 Purpose

Release management ensures that changes are delivered in a controlled and traceable way.

## 15.2 Release Types

OpsSphere may use the following release categories:

| Release Type | Description |
|---|---|
| Documentation Release | Adds or updates planning, architecture, or design documentation |
| Feature Release | Adds new application behavior |
| Bug Fix Release | Fixes incorrect behavior |
| Technical Release | Updates infrastructure, CI/CD, dependencies, or configuration |
| Security Release | Fixes security or access-control issues |

## 15.3 Versioning Strategy

A simple semantic versioning strategy may be used:

```text
MAJOR.MINOR.PATCH
```

Examples:

```text
0.1.0 - Initial MVP planning docs
0.2.0 - Initial backend foundation
0.3.0 - Ticket lifecycle implementation
1.0.0 - MVP release
```

## 15.4 Release Notes

Each release should summarize:

```text
What changed
Why it changed
Related issues
Validation performed
Known limitations
Deployment notes
```

---

# 16. Rollback Strategy

## 16.1 Purpose

Rollback planning reduces deployment risk.

Even in a portfolio project, documenting rollback strategy demonstrates enterprise delivery thinking.

## 16.2 Application Rollback

Application rollback may involve:

```text
Redeploy previous backend artifact
Redeploy previous frontend artifact
Disable problematic feature flag if available
Restore previous configuration
```

## 16.3 Database Rollback

Database rollback requires caution.

Preferred approach:

```text
Take backup before production migration
Avoid destructive schema changes
Use forward-fix when safer
Prepare rollback scripts for risky migrations
Validate migrations in staging first
```

## 16.4 Rollback Decision Criteria

Rollback should be considered when:

```text
Application fails to start
Authentication is broken
Users cannot access core workflows
Ticket creation fails
Database migration causes critical errors
Security-sensitive behavior is broken
Critical API endpoints return unexpected errors
```

---

# 17. Deployment Validation

## 17.1 Smoke Tests

After deployment, run smoke tests to confirm the application is operational.

Minimum smoke tests:

```text
Frontend loads
API health endpoint responds
Database connection works
Login works
Ticket list loads
Ticket creation works
SLA dashboard loads
Audit log records critical action
```

## 17.2 Health Check Endpoint

The backend should expose a health check endpoint.

Example:

```text
GET /health
```

Expected checks:

```text
API process is running
Database connection is available
Required configuration is present
```

Future checks may include:

```text
Background worker status
External service availability
Application Insights connectivity
```

## 17.3 Deployment Checklist

```text
CI pipeline passed
Staging validation completed
Secrets configured
Database migration reviewed
Database backup completed if production
Application deployed
Health check passed
Smoke tests passed
Logs reviewed
No critical errors detected
```

---

# 18. Observability Readiness

## 18.1 Purpose

Deployment is not complete unless the application can be monitored.

OpsSphere should be prepared for observability through:

- Structured logs.
- Health checks.
- Error handling.
- Application metrics.
- Audit logs.
- Application Insights.

Detailed observability practices are defined in:

```text
docs/19-observability-and-support.md
```

## 18.2 Minimum Observability for Deployment

Minimum deployment observability:

```text
API startup logs
Request logs
Error logs
Health endpoint
Database connection failure logging
Authentication failure logging
Audit logs for critical business actions
```

---

# 19. Security and Access Considerations

## 19.1 Deployment Security Principles

Deployment should follow these principles:

```text
Use HTTPS in hosted environments.
Do not expose secrets in source control.
Do not expose internal stack traces to users.
Use least privilege for cloud resources.
Restrict production deployment permissions.
Use separate staging and production configuration.
Validate authentication and authorization before release.
```

## 19.2 Sensitive Configuration

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

These values must be stored securely and never hardcoded.

---

# 20. DevOps Risks and Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Secrets committed to repository | Security exposure | Use `.gitignore`, `.env.example`, GitHub Secrets, and Azure Key Vault |
| Local setup is too complex | Slower development | Use Docker Compose and clear setup instructions |
| CI pipeline is missing | Regressions reach main branch | Add GitHub Actions early |
| Tests are not run before merge | Broken builds | Require CI checks on pull requests |
| Staging differs too much from production | Deployment surprises | Keep staging configuration close to production |
| Database migrations break deployment | Data or schema issues | Test migrations locally and in staging |
| Frontend and backend versions mismatch | Runtime errors | Deploy compatible artifacts together |
| Logs are insufficient after deployment | Hard troubleshooting | Use structured logging and health checks |
| Production secrets stored in appsettings | Security risk | Use Azure Key Vault or environment variables |
| Manual deployment steps are undocumented | Inconsistent releases | Maintain deployment checklist |

---

# 21. Initial DevOps Backlog

The following backlog should guide future implementation.

## Local Environment

- Add Docker Compose for SQL Server.
- Add `.env.example`.
- Add local setup instructions.
- Add seed data strategy.
- Add database reset instructions.

## Backend CI

- Add GitHub Actions workflow for .NET restore, build, and test.
- Add backend test result reporting.
- Add integration test support.
- Add optional code coverage reporting.

## Frontend CI

- Add Node.js setup.
- Add `npm ci`.
- Add Angular build.
- Add frontend test execution.
- Add optional linting.

## Deployment

- Create Azure App Service deployment plan.
- Create Azure SQL deployment plan.
- Add staging deployment workflow.
- Add production deployment workflow with manual approval.
- Add smoke test script.

## Secrets

- Add `.gitignore` rules for local secret files.
- Configure GitHub repository secrets.
- Plan Azure Key Vault integration.
- Document required environment variables.

## Observability

- Add Serilog configuration.
- Add health checks.
- Add Application Insights readiness.
- Add deployment log review checklist.

---

# 22. Summary

The OpsSphere deployment and DevOps strategy is designed to support professional software delivery.

The strategy starts with a practical local development environment, Docker-based infrastructure, GitHub workflow discipline, and automated CI validation.

As the project matures, OpsSphere should become Azure-ready through:

- Azure App Service.
- Azure SQL Database.
- Azure Key Vault.
- Application Insights.
- GitHub Actions deployment pipelines.

This delivery strategy demonstrates that OpsSphere is not only designed as an enterprise application, but also planned as a system that can be built, tested, deployed, monitored, and maintained through repeatable engineering practices.