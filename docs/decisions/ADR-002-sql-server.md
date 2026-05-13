# ADR-002: Use SQL Server as the Primary Database

## Status

Accepted

## Context

OpsSphere needs to store structured operational data for a multinational BPO and contact center environment.

The system must manage users, roles, permissions, regions, countries, accounts, campaigns, customers, tickets, ticket assignments, internal comments, SLA state, escalations, resolutions, audit history, dashboards, and reporting-ready operational data.

The data model contains many relational concepts:

```text
Region
  → Country
    → Account
      → Campaign
        → Ticket
```

Tickets must also be linked to:

```text
- Customer
- Created by user
- Assigned agent
- Supervisor
- Priority
- Status
- SLA state
- Comments
- Escalations
- Resolution
- Audit records
```

OpsSphere also needs strong consistency for important business operations such as ticket creation, assignment, SLA tracking, status changes, user role changes, scope changes, and audit logging.

Because the platform is designed as an enterprise-grade .NET application, the database choice should align with the backend stack, relational data requirements, reporting needs, and Azure-ready deployment strategy.

## Decision

OpsSphere will use SQL Server as the primary relational database.

SQL Server will store the core operational data for:

```text
- Identity and access
- Users
- Roles
- Permissions
- User scopes
- Regions
- Countries
- Accounts
- Campaigns
- Customers
- Tickets
- Ticket comments
- Ticket assignments
- Ticket escalations
- Ticket status history
- Ticket resolutions
- SLA policies
- Ticket SLA state
- Audit logs
- Reporting-ready operational views
```

Entity Framework Core will be used as the primary data access technology for the application.

## Rationale

SQL Server is a strong fit for OpsSphere because the system is centered around structured business records, relational integrity, operational history, and reporting-ready data.

OpsSphere requires relationships between users, roles, scopes, organizational structure, customers, tickets, SLAs, comments, escalations, and audit records. A relational database makes these relationships explicit and enforceable through primary keys, foreign keys, indexes, constraints, and transactions.

SQL Server also fits the selected technology stack:

```text
Backend:
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core

Database:
- SQL Server

Cloud readiness:
- Azure SQL
```

This makes the project easier to position as an enterprise .NET platform and keeps the architecture aligned with common senior .NET development environments.

SQL Server also supports future reporting needs. OpsSphere is not intended to replace Power BI, but it should produce clean, structured operational data that can later support external reporting and business intelligence tools.

## Consequences

### Positive Consequences

- Relational integrity can be enforced through foreign keys and constraints.
- Ticket, customer, account, campaign, user, SLA, and audit data can be modeled clearly.
- Transactions can protect important workflows such as ticket creation and audit logging.
- SQL Server aligns well with .NET, Entity Framework Core, and Azure SQL.
- The database can support indexed filtering for dashboards, queues, SLA views, and reports.
- Structured data can later be exported or consumed by reporting tools such as Power BI.
- SQL Server is familiar in enterprise environments and common in .NET job requirements.

### Negative Consequences

- SQL Server adds more setup complexity than an embedded or file-based database.
- Local development may require Docker, SQL Server Express, LocalDB, or another SQL Server-compatible setup.
- Schema design must be handled carefully to avoid unnecessary complexity.
- Migrations must be managed consistently through Entity Framework Core.
- Poor indexing or excessive joins could affect dashboard and reporting performance if the data grows.
- SQL Server may be heavier than needed for very small prototypes.

## Alternatives Considered

### PostgreSQL

PostgreSQL is a strong relational database and could also support the needs of OpsSphere.

Rejected for this project because the selected portfolio stack is intentionally focused on enterprise .NET, SQL Server, Entity Framework Core, and Azure-ready architecture.

PostgreSQL remains a valid alternative for other contexts, but SQL Server better supports the positioning of OpsSphere as a senior-level .NET enterprise project.

### SQLite

SQLite would be simple for local development and lightweight prototypes.

Rejected because OpsSphere is intended to simulate an enterprise operations platform, not a small local application. SQLite would not represent the expected production-style database environment for this project.

### MongoDB or Other NoSQL Database

A document database could store flexible ticket data or event-like structures.

Rejected because OpsSphere has a strongly relational domain model. Users, roles, permissions, scopes, accounts, campaigns, tickets, SLAs, assignments, comments, and audit logs require clear relationships and consistency.

NoSQL may be considered in the future for specialized use cases such as event streams, search indexes, or analytics, but not as the primary database.

### In-Memory Database

An in-memory database could be useful for tests or temporary development scenarios.

Rejected as the primary database because OpsSphere requires persistent operational data, relational integrity, audit history, and realistic enterprise database behavior.

## Implementation Notes

The database design should follow these principles:

```text
- Use relational integrity for core business relationships.
- Use foreign keys for important entity relationships.
- Use indexes for common filters and dashboard queries.
- Preserve historical traceability.
- Prefer deactivation or soft deletion when historical records depend on existing data.
- Store timestamps in UTC.
- Keep audit logs immutable where possible.
- Use Entity Framework Core migrations to evolve the schema.
```

Initial database areas:

```text
Identity and Access
  - Users
  - Roles
  - Permissions
  - UserRoles
  - RolePermissions
  - UserScopes

Organization Structure
  - Regions
  - Countries
  - Accounts
  - Campaigns
  - ManagerAssignments
  - SupervisorAssignments
  - AgentAssignments

Customer Management
  - Customers

Ticket Management
  - Tickets
  - TicketComments
  - TicketAssignments
  - TicketEscalations
  - TicketStatusHistory
  - TicketResolutions

SLA Management
  - SlaPolicies
  - TicketSlaStates

Audit
  - AuditLogs

Reporting Support
  - Indexed operational fields
  - Reporting-ready views or exports
```

## Decision Scope

This ADR defines SQL Server as the primary database engine for OpsSphere.

It does not define the complete physical schema, table structure, indexing strategy, or migration implementation. Those details belong in the database design documentation and implementation work.