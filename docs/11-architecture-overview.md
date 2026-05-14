# Architecture Overview

## Document Information

| Field | Value |
|---|---|
| Project | OpsSphere |
| Document | Architecture Overview |
| File | `docs/11-architecture-overview.md` |
| Version | 1.0 |
| Status | Draft |
| Project Type | Enterprise Support Operations Platform |
| Architecture Style | Clean Architecture |
| Backend | .NET 10 / ASP.NET Core Web API |
| Frontend | Angular / TypeScript |
| Database | SQL Server |
| Related Issue | #5 |

---

## 1. Introduction and Goals

OpsSphere is an enterprise-grade operations platform designed for multinational BPO and contact center environments.

The platform supports ticket management, customer management, SLA tracking, internal comments, escalation workflows, dashboards, audit history, and role-based access control across regions, countries, accounts, campaigns, supervisors, agents, managers, and viewers.

This document defines the initial architecture direction for OpsSphere.

The goal is to describe how the system will be structured before implementation begins, so the project can be built in a maintainable, testable, secure, and enterprise-ready way.

This document follows a simplified architecture documentation structure inspired by arc42.

The purpose is not to create unnecessary documentation overhead. The purpose is to make the technical direction clear enough to guide implementation.

---

## 1.1 Architecture Goals

The main architecture goals are:

1. Keep business logic separated from controllers, UI, and infrastructure.
2. Use Clean Architecture to protect the domain and application layers.
3. Build the backend using .NET 10 and ASP.NET Core Web API.
4. Build the frontend using Angular and TypeScript.
5. Use SQL Server as the primary relational database.
6. Use Entity Framework Core for persistence.
7. Use CQRS with MediatR for application use cases.
8. Enforce authentication and authorization using JWT and role-based access control.
9. Maintain audit history for critical business actions.
10. Design the system to be Docker-ready and Azure-ready.

---

## 1.2 Quality Goals

| Quality Goal | Description |
|---|---|
| Maintainability | The system should be easy to understand, modify, and extend. |
| Testability | Core business rules and workflows should be testable without depending on UI or infrastructure. |
| Security | Protected resources must require authentication and authorization. |
| Auditability | Critical business actions must be traceable through audit history. |
| Scalability Readiness | The system should be structured so future modules, integrations, and deployment improvements can be added cleanly. |
| Operational Clarity | The architecture should reflect the business structure of regions, countries, accounts, campaigns, supervisors, agents, tickets, SLAs, and audit records. |

---

## 1.3 Main Stakeholders

| Stakeholder | Architecture Expectation |
|---|---|
| Admin | Secure and reliable platform configuration. |
| Operations Manager | Reliable operational visibility across assigned regions. |
| Supervisor | Clear workload, SLA, escalation, and agent visibility. |
| Agent | Simple workflows for handling tickets within scope. |
| Viewer | Read-only access to dashboards, tickets, and audit information. |
| Technical Owner | Maintainable, testable, secure, and portfolio-ready technical foundation. |

---

# 2. Architecture Constraints

## 2.1 Technical Constraints

OpsSphere will use the following technical stack:

| Area | Technology |
|---|---|
| Backend Framework | .NET 10 |
| API Layer | ASP.NET Core Web API |
| Application Pattern | CQRS with MediatR |
| Persistence | Entity Framework Core |
| Database | SQL Server |
| Frontend | Angular |
| Frontend Language | TypeScript |
| Authentication | JWT |
| Authorization | Role-based authorization and scope-based access |
| Logging | Serilog |
| Testing | xUnit, integration tests, WebApplicationFactory, Testcontainers |
| Local Environment | Docker and Docker Compose |
| CI/CD | GitHub Actions |
| Cloud Readiness | Azure App Service, Azure SQL, Azure Key Vault, Application Insights |

---

## 2.2 Business Constraints

OpsSphere must stay focused on the operational management layer of a BPO/contact center environment.

The initial version must support:

- Internal users only.
- Role-based access.
- Scope-based visibility.
- Ticket lifecycle management.
- SLA tracking.
- Internal comments.
- Escalations.
- Audit history.
- Basic dashboards.
- Structured operational data.

The initial version must not become:

- A full business intelligence platform.
- A workforce management system.
- A telephony platform.
- A payroll system.
- A customer portal.
- A CRM replacement.
- A full AI automation suite.

---

## 2.3 Documentation Constraints

The architecture documentation should remain practical and implementation-oriented.

Documents should be stored under the `docs/` folder and diagram assets should be stored under the appropriate diagram folders:

```text
docs/
  diagrams/
    architecture/
    uml/
    database/
```

Architecture diagrams may be created later using tools such as Mermaid, Draw.io, Lucidchart, PlantUML, or another diagramming tool.

Markdown documents should reference diagram files even if the diagrams are initially placeholders.

---

# 3. System Context

## 3.1 Business Context

OpsSphere operates inside a multinational BPO/contact center environment.

The business structure is:

```text
Multinational BPO / Contact Center
  → Region
    → Country
      → Account
        → Campaign
          → Supervisor
            → Agent
```

Tickets are created and managed within this operational context.

Each ticket may be connected to:

- Customer.
- Region.
- Country.
- Account.
- Campaign.
- Supervisor.
- Assigned agent.
- Priority.
- Status.
- SLA state.
- Comments.
- Escalations.
- Audit history.

---

## 3.2 System Users

OpsSphere supports the following internal user roles:

| Role | Main Responsibility |
|---|---|
| Admin | Manage platform structure, users, roles, permissions, and scopes. |
| Operations Manager | Monitor performance across assigned regions. |
| Supervisor | Manage agents, tickets, workload, SLA risk, and escalations within scope. |
| Agent | Create, update, comment on, escalate, resolve, and close tickets within scope. |
| Viewer | View operational information in read-only mode. |

Customers are linked to tickets but do not directly access the system in the initial version.

---

## 3.3 External Systems

The initial version of OpsSphere has limited external system interaction.

| External System | Purpose | Initial Version |
|---|---|---|
| SQL Server | Primary relational database | In scope |
| Browser | User access to Angular frontend | In scope |
| Azure App Service | Future backend/frontend hosting target | Azure-ready |
| Azure SQL | Future managed database target | Azure-ready |
| Azure Key Vault | Future secrets management target | Azure-ready |
| Application Insights | Future monitoring and telemetry target | Azure-ready |
| Power BI | External reporting and advanced analytics | Out of core scope |
| Email / Notification Services | Future notification delivery | Future phase |
| Enterprise Identity Provider | Future SSO integration | Out of MVP |

---

## 3.4 System Boundary

OpsSphere includes:

- Angular frontend.
- ASP.NET Core Web API backend.
- Application layer use cases.
- Domain model and business rules.
- Entity Framework Core persistence.
- SQL Server database.
- Authentication and authorization.
- Audit logging.
- Operational dashboards.
- Reporting-ready data views.

OpsSphere does not include:

- External customer portal.
- Full Power BI dashboard authoring.
- Telephony infrastructure.
- Omnichannel messaging.
- Workforce scheduling.
- Payroll.
- HR management.
- Production-grade SSO in the initial version.

---

# 4. Solution Strategy

## 4.1 Architecture Style

OpsSphere will follow Clean Architecture.

The system will be organized around separate layers:

```text
Presentation
  → Application
    → Domain
      ← Infrastructure
```

The dependency direction should point inward.

The domain and application layers should not depend on the API, frontend, database, or external services.

---

## 4.2 Backend Strategy

The backend will be built with .NET 10 and ASP.NET Core Web API.

The API layer will expose HTTP endpoints for:

- Authentication.
- Users.
- Roles and permissions.
- Regions.
- Countries.
- Accounts.
- Campaigns.
- Customers.
- Tickets.
- Comments.
- SLA.
- Escalations.
- Dashboards.
- Reports.
- Audit history.

Controllers should remain thin.

Controllers should delegate business workflows to application commands, queries, services, or use cases.

---

## 4.3 Application Strategy

The application layer will use CQRS with MediatR.

Commands will represent write operations.

Examples:

- `CreateTicketCommand`
- `AssignTicketCommand`
- `UpdateTicketStatusCommand`
- `AddInternalCommentCommand`
- `EscalateTicketCommand`
- `ResolveTicketCommand`
- `CloseTicketCommand`
- `CreateUserCommand`
- `AssignUserRoleCommand`
- `AssignUserScopeCommand`

Queries will represent read operations.

Examples:

- `GetTicketByIdQuery`
- `GetTicketListQuery`
- `GetSlaDashboardQuery`
- `GetUserListQuery`
- `GetAuditHistoryQuery`
- `GetSupervisorWorkloadQuery`

This separation keeps workflow logic organized and testable.

---

## 4.4 Domain Strategy

The domain layer will contain the main business concepts and invariants.

Initial domain concepts include:

- User.
- Role.
- Permission.
- Scope.
- Region.
- Country.
- Account.
- Campaign.
- Customer.
- Ticket.
- Ticket status.
- Ticket priority.
- SLA.
- Escalation.
- Comment.
- Resolution.
- Audit trail.

Domain logic should protect important rules such as:

- A ticket must be resolved before it can be closed.
- Closed tickets cannot be modified unless reopening is explicitly allowed.
- A ticket must have a customer, account, campaign, priority, and status.
- Escalated tickets must include an escalation reason.
- Internal comments cannot be empty.
- Critical actions must create audit records.
- Users cannot operate outside their authorized role and scope.

---

## 4.5 Infrastructure Strategy

The infrastructure layer will contain technical implementations.

Examples:

- Entity Framework Core DbContext.
- SQL Server persistence.
- Repository implementations if used.
- Identity and password hashing infrastructure.
- JWT token generation.
- Audit persistence.
- Logging implementation with Serilog.
- External service integrations in future phases.
- File storage in future phases.
- Email or notification services in future phases.

Infrastructure should implement interfaces defined by the application layer when appropriate.

---

## 4.6 Frontend Strategy

The frontend will be built with Angular and TypeScript.

The frontend should provide role-aware and scope-aware user experiences.

Initial frontend modules may include:

- Login.
- Dashboard.
- Ticket queue.
- Ticket detail.
- Ticket creation.
- Customer management.
- User management.
- Operational structure management.
- SLA views.
- Escalation views.
- Audit history views.
- Reports or exports.

The frontend may use Angular Material or Tailwind for UI structure.

Frontend visibility rules should improve user experience, but backend authorization must remain the source of truth.

---

## 4.7 Database Strategy

SQL Server will be the primary database.

Entity Framework Core will be used for database access and migrations.

The initial database design should support:

- Relational integrity.
- Foreign keys.
- Required fields.
- Indexes for common filters.
- Audit history.
- Historical traceability.
- Deactivation instead of physical deletion for operational structure records.

---

# 5. Building Block View

## 5.1 High-Level Building Blocks

```text
OpsSphere
├── Frontend
│   └── Angular SPA
│
├── Backend
│   ├── API Layer
│   ├── Application Layer
│   ├── Domain Layer
│   └── Infrastructure Layer
│
├── Database
│   └── SQL Server
│
└── DevOps
    ├── Docker
    ├── Docker Compose
    └── GitHub Actions
```

---

## 5.2 Backend Building Blocks

```text
src/
  OpsSphere.Api/
    Controllers/
    Middleware/
    Authentication/
    Authorization/
    DependencyInjection/

  OpsSphere.Application/
    Common/
    Interfaces/
    Behaviors/
    Features/
      Auth/
      Users/
      Customers/
      Tickets/
      Comments/
      SLA/
      Dashboards/
      Reports/
      Audit/

  OpsSphere.Domain/
    Entities/
    ValueObjects/
    Enums/
    Events/
    Exceptions/
    Rules/

  OpsSphere.Infrastructure/
    Persistence/
    Identity/
    Authentication/
    Authorization/
    Logging/
    Services/
```

---

## 5.3 Frontend Building Blocks

```text
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

---

## 5.4 Database Building Blocks

Initial database areas:

```text
Identity and Access
  - Users
  - Roles
  - Permissions
  - UserRoles
  - UserScopes

Organization
  - Regions
  - Countries
  - Accounts
  - Campaigns
  - ManagerAssignments
  - SupervisorAssignments
  - AgentAssignments

Operations
  - Customers
  - Tickets
  - TicketComments
  - TicketAssignments
  - TicketEscalations
  - TicketStatusHistory
  - TicketResolutions

SLA
  - SlaPolicies
  - TicketSlaStates

Audit
  - AuditLogs
```

---

# 6. Runtime View

This section describes important runtime scenarios.

---

## 6.1 User Authentication Runtime Flow

```text
User
  → Angular Login Page
  → ASP.NET Core Auth Endpoint
  → Validate Credentials
  → Verify Active User
  → Load Roles and Scope
  → Generate JWT
  → Return Token to Frontend
  → Frontend Stores Token
  → User Accesses Protected Features
```

Important rules:

- Only active users may authenticate.
- Protected endpoints require a valid token.
- Expired tokens must be rejected.
- Missing roles or invalid scope should prevent access to protected modules.

---

## 6.2 Create Ticket Runtime Flow

```text
Agent or Supervisor
  → Opens Ticket Creation Page
  → Enters Required Ticket Data
  → Angular Sends Create Ticket Request
  → API Validates Authentication
  → API Applies Authorization
  → Application Handles CreateTicketCommand
  → Domain Validates Ticket Rules
  → EF Core Persists Ticket
  → SLA State Is Initialized
  → Audit Log Is Created
  → API Returns Created Ticket
```

Important rules:

- Ticket must be linked to a customer.
- Ticket must be linked to an account and campaign.
- Ticket must include priority, category, description, and creator.
- Ticket creation must respect operational scope.
- Ticket creation must be audited.

---

## 6.3 Assign Ticket Runtime Flow

```text
Supervisor
  → Opens Ticket Detail
  → Selects Eligible Agent
  → Angular Sends Assignment Request
  → API Validates Supervisor Role and Scope
  → Application Handles AssignTicketCommand
  → Domain Validates Assignment Rules
  → EF Core Updates Ticket Assignment
  → Ticket Status Updates If Needed
  → Audit Log Is Created
  → API Returns Updated Ticket
```

Important rules:

- Supervisor can only assign tickets within assigned scope.
- Ticket can only be assigned to an eligible active agent.
- Assignment and reassignment must be audited.
- Closed tickets cannot be assigned unless reopening is allowed.

---

## 6.4 Escalate Ticket Runtime Flow

```text
Agent or Supervisor
  → Opens Ticket Detail
  → Selects Escalate
  → Enters Escalation Reason
  → Angular Sends Escalation Request
  → API Validates Role and Scope
  → Application Handles EscalateTicketCommand
  → Domain Validates Escalation Rules
  → Ticket Status Becomes Escalated
  → Escalation Record Is Persisted
  → Audit Log Is Created
  → Supervisor Queue Displays Escalated Ticket
```

Important rules:

- Escalation reason is required.
- Closed tickets cannot be escalated.
- Escalations must be visible to authorized supervisors.
- Escalation events must be audited.

---

## 6.5 View SLA Dashboard Runtime Flow

```text
Manager, Supervisor, Agent, or Viewer
  → Opens SLA Dashboard
  → Angular Requests Dashboard Data
  → API Validates Authentication
  → API Applies Role and Scope Filters
  → Application Handles GetSlaDashboardQuery
  → SQL Server Returns Filtered Metrics
  → API Returns Dashboard View Model
  → Angular Displays Scoped Metrics
```

Important rules:

- Dashboard data must respect role and scope.
- Viewers must only receive read-only data.
- SLA state must be available for filtering.
- Unauthorized data must not be exposed.

---

# 7. Deployment View

## 7.1 Local Development Deployment

The local development environment should use Docker Compose where practical.

```text
Developer Machine
  ├── Angular Frontend
  ├── ASP.NET Core Web API
  ├── SQL Server Container
  └── Optional Supporting Tools
```

Possible local components:

- Angular development server.
- ASP.NET Core API.
- SQL Server container.
- Docker Compose network.
- Seed data.
- Integration test containers.

---

## 7.2 Azure-Ready Deployment Target

OpsSphere should be designed to support future deployment to Azure.

```text
Browser
  → Azure App Service / Static Web App
    → ASP.NET Core Web API
      → Azure SQL
      → Azure Key Vault
      → Application Insights
```

Target Azure services:

| Azure Service | Purpose |
|---|---|
| Azure App Service | Host backend API and possibly frontend. |
| Azure Static Web Apps | Optional frontend hosting target. |
| Azure SQL | Managed SQL Server database. |
| Azure Key Vault | Secure secrets and connection strings. |
| Application Insights | Monitoring, telemetry, and diagnostics. |
| Azure Storage | Future attachment storage if needed. |

---

## 7.3 Deployment Principles

Deployment should follow these principles:

- Configuration should be environment-based.
- Secrets should not be committed to source control.
- Database migrations should be controlled.
- Logging should be structured.
- Health checks should be available.
- CI/CD should be automated through GitHub Actions.
- Production-specific settings should be separated from local development settings.

---

# 8. Cross-Cutting Concepts

## 8.1 Authentication

OpsSphere will use JWT authentication for API access.

Authentication responsibilities:

- Validate credentials.
- Verify active user state.
- Generate JWT tokens.
- Protect API endpoints.
- Reject expired or invalid tokens.

---

## 8.2 Authorization

OpsSphere will use role-based authorization and scope-based access control.

Authorization dimensions:

```text
Role
  + Operational Scope
  + Resource Ownership
  + Action Permission
```

Examples:

- Admin can manage users, roles, permissions, and organizational structure.
- Manager can view data within assigned regions.
- Supervisor can manage tickets within assigned accounts or campaigns.
- Agent can work only on tickets within assigned scope.
- Viewer can only view read-only operational data.

Backend authorization must be enforced even if the frontend hides unavailable actions.

---

## 8.3 Audit Logging

Audit logging is a core architecture concept.

Critical actions must be recorded in audit history.

Examples:

- Ticket created.
- Ticket assigned.
- Ticket reassigned.
- Ticket status changed.
- Ticket priority changed.
- Ticket escalated.
- Ticket resolved.
- Ticket closed.
- Internal comment added.
- User created.
- User updated.
- User deactivated.
- Role changed.
- Scope changed.
- Organizational structure changed.

Audit records should include:

- Actor.
- Timestamp.
- Entity type.
- Entity identifier.
- Action.
- Previous value when applicable.
- New value when applicable.

---

## 8.4 Error Handling

The API should return consistent error responses.

Error categories:

- Validation errors.
- Authentication errors.
- Authorization errors.
- Not found errors.
- Business rule violations.
- Unexpected server errors.

Business rule violations should be explicit enough for developers and users to understand the issue without exposing sensitive information.

---

## 8.5 Validation

Validation should happen at multiple levels:

| Level | Responsibility |
|---|---|
| Frontend | User-friendly form validation. |
| API | Request shape and basic validation. |
| Application | Use case validation and authorization coordination. |
| Domain | Business invariants. |
| Database | Structural integrity, required fields, foreign keys, uniqueness. |

---

## 8.6 Logging

Serilog will be used for structured backend logging.

Logs should support:

- Request tracing.
- Error diagnosis.
- Important operational events.
- Future Application Insights integration.

Logs must not expose sensitive data such as passwords, tokens, or confidential customer information.

---

## 8.7 Data Access

Entity Framework Core will be used for data access.

Data access principles:

- Use SQL Server as the primary relational database.
- Use migrations to manage schema changes.
- Use indexes for common filters.
- Avoid leaking EF Core concerns into the domain layer.
- Keep query logic organized and testable.

---

## 8.8 Testing

Testing should cover:

- Domain rules.
- Application commands and queries.
- Authorization behavior.
- API endpoints.
- Database integration.
- Critical workflows.

Recommended tools:

- xUnit.
- WebApplicationFactory.
- Testcontainers.
- EF Core integration tests.
- Angular unit and component tests where useful.

---

# 9. Architecture Decisions

This section records initial architecture decisions.

---

## ADR-001: Use Clean Architecture

| Field | Value |
|---|---|
| Decision | Use Clean Architecture for backend structure. |
| Status | Accepted |
| Rationale | Keeps business logic independent from controllers, database, and infrastructure. Improves maintainability and testability. |
| Consequence | Requires more initial structure but improves long-term organization. |

---

## ADR-002: Use .NET 10 and ASP.NET Core Web API

| Field | Value |
|---|---|
| Decision | Use .NET 10 with ASP.NET Core Web API for backend services. |
| Status | Accepted |
| Rationale | Aligns with enterprise .NET roles and supports modern API development. |
| Consequence | Backend implementation will be centered around controllers, middleware, dependency injection, and application services. |

---

## ADR-003: Use Angular for Frontend

| Field | Value |
|---|---|
| Decision | Use Angular and TypeScript for the frontend. |
| Status | Accepted |
| Rationale | Angular is commonly used in enterprise environments and fits structured business applications. |
| Consequence | Frontend modules should be organized by features and protected by route guards. |

---

## ADR-004: Use SQL Server as Primary Database

| Field | Value |
|---|---|
| Decision | Use SQL Server as the primary relational database. |
| Status | Accepted |
| Rationale | Fits enterprise .NET environments and supports relational operational data. |
| Consequence | Database design should include normalized tables, foreign keys, indexes, and audit history. |

---

## ADR-005: Use Entity Framework Core

| Field | Value |
|---|---|
| Decision | Use Entity Framework Core for persistence. |
| Status | Accepted |
| Rationale | Provides standard .NET data access, migrations, and SQL Server integration. |
| Consequence | Infrastructure layer will contain DbContext, configurations, migrations, and persistence implementations. |

---

## ADR-006: Use CQRS with MediatR

| Field | Value |
|---|---|
| Decision | Use CQRS with MediatR for application use cases. |
| Status | Accepted |
| Rationale | Separates commands and queries, keeps use cases explicit, and improves testability. |
| Consequence | Application features will be implemented as commands, queries, handlers, validators, and DTOs. |

---

## ADR-007: Use JWT Authentication and RBAC

| Field | Value |
|---|---|
| Decision | Use JWT authentication with role-based authorization. |
| Status | Accepted |
| Rationale | Supports stateless API authentication and protected endpoints. |
| Consequence | API endpoints must enforce authentication and authorization policies. |

---

## ADR-008: Treat Audit Logging as a Core Requirement

| Field | Value |
|---|---|
| Decision | Audit logging must be part of core workflows. |
| Status | Accepted |
| Rationale | OpsSphere depends on traceability and accountability. |
| Consequence | Commands that change important business state must create audit records. |

---

## ADR-009: Design for Azure-Ready Deployment

| Field | Value |
|---|---|
| Decision | Design the application to be Azure-ready. |
| Status | Accepted |
| Rationale | Demonstrates enterprise deployment awareness and supports future cloud deployment. |
| Consequence | Configuration, secrets, logging, health checks, and deployment scripts should be cloud-friendly. |

---

# 10. Risks and Technical Debt

## 10.1 Initial Architecture Risks

| Risk | Description | Mitigation |
|---|---|---|
| Overengineering | Clean Architecture and CQRS can become too heavy if applied without discipline. | Keep handlers focused and avoid unnecessary abstractions. |
| Scope Creep | The system could expand into BI, telephony, AI, or CRM too early. | Keep MVP scope focused on operations, tickets, SLA, audit, and RBAC. |
| Authorization Complexity | Role and scope rules can become difficult to maintain. | Define permissions clearly and test authorization behavior directly. |
| SLA Complexity | SLA rules can become complex if business-hour calendars and pause rules are added too early. | Start with a simple SLA model and expand later. |
| Audit Volume | Audit logging can grow quickly. | Use structured audit records and plan indexes for common audit queries. |
| Database Complexity | Region, country, account, campaign, user, and ticket relationships can become complex. | Keep database design normalized and document relationships clearly. |
| Frontend Permission Drift | UI visibility rules may drift from backend authorization. | Treat backend authorization as the source of truth. |
| Reporting Expectations | Users may expect full Power BI-like analytics. | Keep dashboards operational and export reporting-ready data. |

---

## 10.2 Known Technical Debt Accepted for MVP

| Technical Debt | Reason |
|---|---|
| Simple SLA model | Advanced SLA calendars are intentionally deferred. |
| Basic dashboard queries | Advanced analytics are outside MVP scope. |
| No production SSO | JWT-based internal authentication is enough for MVP. |
| No external customer portal | Customers are linked to tickets but do not log in. |
| Limited notification system | Notifications may be added in a future phase. |
| Basic deployment readiness | Full production hardening may be added after MVP. |

---

# 11. Related Documents

| Document | File |
|---|---|
| Executive Summary | `docs/00-executive-summary.md` |
| Business Context | `docs/01-business-context.md` |
| Business Case | `docs/02-business-case.md` |
| Project Charter | `docs/03-project-charter.md` |
| Stakeholders | `docs/04-stakeholders.md` |
| Scope and Roadmap | `docs/05-scope-and-roadmap.md` |
| Software Requirements Specification | `docs/06-requirements.md` |
| Use Cases | `docs/07-use-cases.md` |
| Business Process Flows | `docs/08-business-process-flows.md` |
| Business Rules | `docs/09-business-rules.md` |
| Domain Model | `docs/10-domain-model.md` |
| C4 Architecture | `docs/12-c4-architecture.md` |
| UML Diagrams | `docs/13-uml-diagrams.md` |
| Database Design | `docs/14-database-design.md` |
| API Design | `docs/15-api-design.md` |
| Security and Permissions | `docs/16-security-and-permissions.md` |

---

# 12. Diagram References

The following diagrams should be created or referenced in the related technical design documents.

## 12.1 Architecture Diagrams

```text
docs/diagrams/architecture/
  c4-system-context.png
  c4-container.png
  c4-component-backend.png
  deployment-overview.png
```

## 12.2 UML Diagrams

```text
docs/diagrams/uml/
  use-case-diagram.png
  domain-class-diagram.png
  ticket-lifecycle-sequence.png
  ticket-assignment-sequence.png
  escalation-sequence.png
```

## 12.3 Database Diagrams

```text
docs/diagrams/database/
  initial-erd.png
  ticket-model-erd.png
  security-model-erd.png
```

---

# 13. Document Summary

OpsSphere will be built as a maintainable enterprise application using Clean Architecture, .NET 10, ASP.NET Core Web API, Angular, SQL Server, Entity Framework Core, CQRS with MediatR, JWT authentication, role-based authorization, audit logging, Docker-ready local development, and Azure-ready deployment design.

The architecture separates business logic from controllers, UI, persistence, and infrastructure.

The system is designed around the operational needs of a multinational BPO/contact center environment, including regions, countries, accounts, campaigns, supervisors, agents, tickets, customers, SLAs, comments, escalations, dashboards, and audit history.

This document provides the technical foundation for the remaining architecture and design documents in issue #5.
