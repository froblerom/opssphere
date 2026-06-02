# OpsSphere

> Enterprise Support Operations Platform — .NET 10 · Angular · SQL Server · Clean Architecture · CQRS · JWT · RBAC+Scope · Audit Logging · Docker · GitHub Actions CI

OpsSphere simulates a multinational BPO/contact center operation where admins, managers, supervisors, agents, and viewers manage tickets, SLA state, escalations, dashboards, and audit history across a structured regional hierarchy.

**This is a senior-level portfolio project** built from business discovery through production-ready architecture — not a tutorial app. It demonstrates the kind of judgment, design, and implementation expected in enterprise .NET roles.

[![CI](https://github.com/froblerom/opssphere/actions/workflows/ci.yml/badge.svg)](https://github.com/froblerom/opssphere/actions/workflows/ci.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular&logoColor=white)](https://angular.dev)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/sql-server)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](docker-compose.yml)

---

## Demo Preview

<!-- PLACEHOLDER: demo GIF or screenshot carousel -->
> _Screenshots and demo recording coming soon._

### Demo Users (fictional seed data)

| Role | Email | Scope |
|---|---|---|
| Admin | `admin@opssphere.local` | All data |
| Operations Manager | `manager.latam@opssphere.local` | LATAM region |
| Supervisor | `supervisor.novabank@opssphere.local` | NOVABANK account |
| Agent | `agent.novabank@opssphere.local` | NOVABANK-CC campaign |
| Viewer | `viewer.latam@opssphere.local` | LATAM region (read-only) |

Password for all demo users: `OpsSphere123!` — hashed before persistence, for local demo only.

---

## What This Project Demonstrates

This project covers the full development lifecycle: from business requirements to a working, testable, documented application.

| Area | Skills Demonstrated |
|---|---|
| **Backend** | .NET 10, ASP.NET Core Web API, Clean Architecture, CQRS/MediatR |
| **Database** | SQL Server, EF Core 9, Fluent API configuration, migrations |
| **Security** | JWT authentication, role-based + scope-based authorization |
| **Domain Design** | 20 domain entities, business rule enforcement, ticket lifecycle |
| **Audit & Traceability** | Structured audit logging as a first-class architecture concern |
| **Frontend** | Angular, TypeScript, RxJS, Angular Material, role-aware route guards |
| **Testing** | xUnit, integration tests, WebApplicationFactory, automated layer boundary enforcement |
| **DevOps** | Docker Compose, GitHub Actions CI, environment-based configuration |
| **Architecture Docs** | ADRs, C4 diagrams, domain model, ERD, process flows |
| **Business Analysis** | Requirements spec, stakeholder mapping, business rules, use cases |

---

## Implemented Features

### Identity & Access
- JWT authentication with configurable signing key and expiry
- 5 roles: Admin, OperationsManager, Supervisor, Agent, Viewer
- Scope-based access: users only see data within their assigned region, account, or campaign
- Granular permissions: `tickets.create`, `tickets.assign`, `audit.view`, etc.

### Operational Structure
- Multi-level org hierarchy: Region → Country → Account → Campaign
- Supervisor and agent assignment management
- Soft-delete (deactivation) for org entities — no physical deletes

### Ticket Management
- Full lifecycle: Open → InProgress → OnHold → Escalated → Resolved → Closed
- Ticket assignment and reassignment with scope enforcement
- Internal comments (agents and supervisors only)
- Priority levels linked to SLA policies
- Escalation workflow with required reason field

### SLA Tracking
- 4 SLA states: WithinSla · AtRisk · Breached · Completed
- Per-priority SLA time windows
- SLA state calculated from ticket creation timestamp

### Audit Logging
- Every critical action writes an audit record: ticket created, assigned, escalated, resolved, status changed, user modified, scope changed
- Records include actor, timestamp, entity type, entity ID, action, previous value, new value

### Dashboards
- Role- and scope-filtered operational metrics
- Open tickets, overdue tickets, by priority, by campaign

### Infrastructure
- GitHub Actions CI: restore → build → test on every push and PR
- Docker Compose for local SQL Server (no local SQL Server install required)
- EF Core migrations with design-time factory
- Fictional deterministic seed data for repeatable demos and tests
- Serilog structured logging with CorrelationId and UserId enrichment

---

## Architecture

![C4 Container Diagram](docs/diagrams/architecture/c4-container-diagram.png)

**Layer boundaries are enforced by automated tests.** The build fails if Domain or Application accidentally imports EF Core or Infrastructure. This is not a convention — it is a compile-time guarantee.

### Key Architecture Decisions

| ADR | Decision |
|---|---|
| [ADR-001](docs/decisions/ADR-001-clean-architecture.md) | Clean Architecture — domain-centered, dependency inversion |
| [ADR-002](docs/decisions/ADR-002-sql-server.md) | SQL Server as primary relational database |
| [ADR-003](docs/decisions/ADR-003-jwt-authentication.md) | JWT authentication — stateless, standard, testable |
| [ADR-004](docs/decisions/ADR-004-angular-frontend.md) | Angular for frontend — structured, enterprise-ready |
| [ADR-005](docs/decisions/ADR-005-entity-framework-core.md) | Entity Framework Core for persistence |

Full ADRs in [docs/decisions/](docs/decisions/).

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API |
| Architecture | Clean Architecture, CQRS, MediatR, FluentValidation |
| Database | SQL Server 2022, Entity Framework Core 9 |
| Frontend | Angular 21, TypeScript, RxJS, Angular Material |
| Auth | JWT Bearer tokens, RBAC + scope-based authorization |
| Logging | Serilog — structured, enriched, console and file sinks |
| Testing | xUnit, WebApplicationFactory, integration tests |
| Local Infra | Docker Compose (SQL Server container) |
| CI/CD | GitHub Actions |
| Cloud Target | Azure App Service, Azure SQL, Azure Key Vault (designed for, not yet deployed) |

---

## Local Setup

**Prerequisites:** .NET 10 SDK · Node.js 20+ · Docker Desktop

### 1. Start the database

~~~~bash
cp .env.example .env
# Edit .env and set MSSQL_SA_PASSWORD

docker compose up -d
~~~~

### 2. Run the backend

Add your connection string and a local JWT signing key to `appsettings.Development.json` (gitignored) or .NET user secrets. See `appsettings.Development.json` for the expected shape.

~~~~bash
dotnet restore OpsSphere.slnx
dotnet build OpsSphere.slnx
dotnet ef database update --project src/OpsSphere.Infrastructure --startup-project src/OpsSphere.Api
dotnet run --project src/OpsSphere.Api
~~~~

### 3. Run the frontend

~~~~bash
cd frontend
npm install
npm start
~~~~

Frontend: `http://localhost:4200` — API: `https://localhost:7024` (or configured port in `launchSettings.json`).

### Reset local environment

~~~~bash
docker compose down -v
docker compose up -d
dotnet ef database update --project src/OpsSphere.Infrastructure --startup-project src/OpsSphere.Api
dotnet run --project src/OpsSphere.Api
~~~~

---

## Demo Script

A full guided walkthrough covering all five personas is in [docs/release/mvp-demo-script.md](docs/release/mvp-demo-script.md).

**Quick smoke test after setup:**

~~~~http
POST /api/auth/login
{ "email": "agent.novabank@opssphere.local", "password": "OpsSphere123!" }
→ Returns bearer token

GET /api/auth/me
Authorization: Bearer <token>
→ Returns current user, role, and scope

GET /api/auth/protected-smoke
→ 401 without token · 200 with valid token
~~~~

---

## Test & Validation

~~~~bash
dotnet test OpsSphere.slnx
~~~~

| Project | What It Covers |
|---|---|
| `OpsSphere.Domain.Tests` | Enforces Domain has zero EF Core / Infrastructure dependencies |
| `OpsSphere.Application.Tests` | Enforces Application has zero EF Core / Infrastructure dependencies |
| `OpsSphere.IntegrationTests` | API smoke tests, EF Core model metadata, SLA and ticket workflow coverage |

CI documentation: [docs/testing/ci-validation.md](docs/testing/ci-validation.md)  
Integration test coverage: [docs/testing/mvp-regression-tests.md](docs/testing/mvp-regression-tests.md)

---

## Roadmap / Not Implemented Yet

Intentional scope deferrals documented in [docs/release/mvp-known-limitations.md](docs/release/mvp-known-limitations.md):

| Feature | Phase |
|---|---|
| Swagger / OpenAPI | Phase 2 |
| Reports module and CSV export | Phase 2 |
| Notifications (email, in-app) | Phase 2 |
| Business-hours SLA calendars | Phase 2 |
| Azure deployment configuration | Phase 2 |
| Application Insights integration | Phase 2 |
| Browser E2E tests (Playwright / Cypress) | Phase 2 |
| Customer portal | Out of scope |
| Workforce management / scheduling | Out of scope |
| Full BI / analytics platform | Out of scope |

See [docs/release/phase-2-3-parking-lot.md](docs/release/phase-2-3-parking-lot.md).

---

## Repository Structure

~~~~
opssphere/
├── src/
│   ├── OpsSphere.Api/              # Thin controllers, middleware, composition root
│   ├── OpsSphere.Application/      # CQRS commands, queries, handlers, validators
│   ├── OpsSphere.Domain/           # Entities, business rules — no framework deps
│   └── OpsSphere.Infrastructure/   # EF Core, SQL Server, JWT, Serilog, Identity
│
├── frontend/
│   └── src/app/
│       ├── core/                   # Auth service, guards, HTTP interceptors
│       ├── features/               # Login, dashboard, tickets, customers, org, users
│       └── shared/                 # Components, pipes, models
│
├── tests/
│   ├── OpsSphere.Domain.Tests/
│   ├── OpsSphere.Application.Tests/
│   └── OpsSphere.IntegrationTests/
│
├── docs/
│   ├── decisions/                  # Architecture Decision Records (ADRs)
│   ├── release/                    # Demo script, release checklist, known limitations
│   └── testing/                    # CI validation, regression coverage
│
├── docker-compose.yml              # Local SQL Server
└── .github/workflows/              # GitHub Actions CI pipeline
~~~~

---

## Documentation

OpsSphere includes a full enterprise documentation suite built alongside the code:

| Document | Purpose |
|---|---|
| [Executive Summary](docs/00-executive-summary.md) | Business problem, target users, platform value |
| [Architecture Overview](docs/11-architecture-overview.md) | Clean Architecture design, runtime flows, quality goals |
| [C4 Architecture](docs/12-c4-architecture.md) | System context, container, and component diagrams |
| [Domain Model](docs/10-domain-model.md) | Core business entities and relationships |
| [Security & Permissions](docs/16-security-and-permissions.md) | Role-based + scope-based access design |
| [API Design](docs/15-api-design.md) | Endpoint structure and contracts |
| [Database Design](docs/14-database-design.md) | ERD and schema decisions |
| [ADRs](docs/decisions/) | Architecture Decision Records |
| [Demo Script](docs/release/mvp-demo-script.md) | Guided walkthrough for all five personas |
| [Known Limitations](docs/release/mvp-known-limitations.md) | Intentional MVP scope boundaries |

---

*All organization names, customer names, and operational data in this project are fictional.*

*Built by Fred Roblero · [LinkedIn](https://www.linkedin.com/in/fred-roblerom/) · [GitHub](https://github.com/froblerom/opssphere)*
