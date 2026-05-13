# OpsSphere Architecture Context

## Purpose

This document provides compressed architecture context for agents working inside OpsSphere.

Use this file when the task touches:

- Backend architecture
- Frontend architecture
- Clean Architecture boundaries
- API design
- CQRS/MediatR
- Persistence
- Security architecture
- DevOps readiness
- Observability readiness
- Architecture review

---

## Architecture Style

OpsSphere uses Clean Architecture.

The dependency direction is:

```text
Presentation
  → Application
    → Domain
      ← Infrastructure
```

The dependency direction must point inward.

Domain and Application must not depend on:

```text
ASP.NET Core controllers
Angular frontend
Entity Framework Core
SQL Server
JWT infrastructure
Serilog infrastructure
Azure services
Email providers
External integrations
```

---

## Technical Stack

| Area | Technology |
|---|---|
| Backend Framework | .NET 8 |
| API Layer | ASP.NET Core Web API |
| Application Pattern | CQRS with MediatR |
| Persistence | Entity Framework Core |
| Database | SQL Server |
| Frontend | Angular |
| Frontend Language | TypeScript |
| Authentication | JWT |
| Authorization | Role-based, permission-based, and scope-based access |
| Logging | Serilog |
| Testing | xUnit, WebApplicationFactory, Testcontainers |
| Local Environment | Docker and Docker Compose |
| CI/CD | GitHub Actions |
| Cloud Readiness | Azure App Service, Azure SQL, Azure Key Vault, Application Insights |

---

## Layer Responsibilities

## Presentation / API Layer

Responsible for:

- HTTP controllers
- Request and response models
- Authentication middleware
- Authorization policies
- Routing
- API error mapping
- OpenAPI/Swagger exposure
- Correlation ID propagation

Rules:

```text
Controllers must remain thin.
Controllers must not contain business workflow logic.
Controllers must delegate to Application commands and queries.
Controllers must not directly enforce complex domain rules.
Controllers must not bypass Application authorization coordination.
```

---

## Application Layer

Responsible for:

- Commands
- Queries
- Handlers
- Validators
- Use case orchestration
- Authorization coordination
- Scope checks
- Transaction boundaries
- Audit coordination
- Application-specific interfaces
- DTOs and result models when appropriate

Examples of commands:

```text
CreateTicketCommand
AssignTicketCommand
UpdateTicketStatusCommand
AddInternalCommentCommand
EscalateTicketCommand
ResolveTicketCommand
CloseTicketCommand
CreateUserCommand
AssignUserRoleCommand
AssignUserScopeCommand
```

Examples of queries:

```text
GetTicketByIdQuery
GetTicketListQuery
GetSlaDashboardQuery
GetUserListQuery
GetAuditHistoryQuery
GetSupervisorWorkloadQuery
```

Rules:

```text
Application handlers coordinate workflows.
Application handlers must enforce use case-level authorization.
Application handlers must coordinate audit records for critical actions.
Application layer may depend on Domain.
Application layer must not depend directly on EF Core implementation.
```

---

## Domain Layer

Responsible for:

- Entities
- Value objects
- Enums
- Domain services
- Domain rules
- Business invariants
- Domain exceptions

Initial domain concepts include:

```text
User
Role
Permission
Scope
Region
Country
Account
Campaign
Customer
Ticket
TicketStatus
TicketPriority
SLA
SLAState
Escalation
Comment
Resolution
AuditTrail
Queue
Dashboard
ReportingView
```

Important rules protected by Domain or Application:

```text
A ticket must be linked to a customer.
A ticket must be linked to an account and campaign.
A ticket must have a priority.
A ticket must have a status.
A ticket must follow valid status transitions.
A ticket must be resolved before it can be closed.
Closed tickets cannot be modified unless reopening is explicitly allowed.
Escalated tickets must include an escalation reason.
Internal comments cannot be empty.
Users cannot operate outside their authorized role and scope.
Critical business actions must create audit records.
```

---

## Infrastructure Layer

Responsible for:

- EF Core DbContext
- SQL Server persistence
- Entity configurations
- Migrations
- Seed data
- Repository implementations if used
- Unit of work implementation if used
- Password hashing
- JWT token generation
- Audit persistence
- Serilog configuration
- External service integrations in future phases
- File storage in future phases
- Email or notification services in future phases

Rules:

```text
Infrastructure implements technical details.
Infrastructure may depend on Application and Domain abstractions.
Infrastructure must not own business workflow decisions.
Infrastructure must not leak EF Core types into Domain.
```

---

## Frontend Architecture

The frontend uses Angular and TypeScript.

Recommended structure:

```text
frontend/
  src/
    app/
      core/
        auth/
        guards/
        interceptors/
        services/

      shared/
        components/
        pipes/
        models/

      features/
        login/
        dashboard/
        tickets/
        customers/
        users/
        organization/
        sla/
        audit/
        reports/
```

Frontend responsibilities:

- Login UI
- Role-aware navigation
- Route guards
- HTTP interceptors for JWT
- Ticket forms
- Ticket queues
- Customer management screens
- User management screens
- Operational structure screens
- SLA views
- Escalation views
- Audit history views
- Dashboards
- Reports or exports

Frontend rules:

```text
Frontend visibility improves UX.
Frontend visibility is not security.
Backend authorization is the source of truth.
Do not rely on hidden buttons as access control.
```

---

## API Design Principles

OpsSphere uses REST-style HTTP endpoints.

Base path:

```text
/api
```

Examples:

```http
GET /api/tickets
POST /api/tickets
GET /api/tickets/{id}
PUT /api/tickets/{id}/status
POST /api/tickets/{id}/comments
POST /api/tickets/{id}/escalate
POST /api/tickets/{id}/resolve
POST /api/tickets/{id}/close
```

Business workflow routes such as `assign`, `escalate`, `resolve`, and `close` are acceptable because they represent domain workflows, not simple field updates.

---

## Database Architecture

OpsSphere uses SQL Server as the primary database.

EF Core is the primary data access technology.

Database areas:

```text
Identity and Access
Organization Structure
Customer Management
Ticket Management
SLA Management
Audit
Reporting Support
```

Important persistence principles:

```text
Use relational integrity.
Use foreign keys for important relationships.
Use indexes for common filters and dashboards.
Use soft delete or deactivation where historical records depend on data.
Preserve historical traceability.
Use UTC timestamps.
Avoid physically deleting records needed by tickets or audit logs.
```

---

## Architectural Invariants

Agents must preserve these invariants:

```text
Business logic does not live in controllers.
Domain does not depend on infrastructure.
Application use cases are explicit.
Authorization is enforced backend-side.
Scope filters are applied in backend queries.
Audit-sensitive actions produce audit records.
Database design preserves historical context.
Frontend does not become the security boundary.
Tests protect critical workflows.
DevOps and observability readiness are not afterthoughts.
```
