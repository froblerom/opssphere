# ADR-005: Use Entity Framework Core for Data Access

## Status

Accepted

## Context

OpsSphere uses SQL Server as the primary relational database.

The system must persist and query operational data related to:

```text
- Users
- Roles
- Permissions
- Operational scopes
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
- Ticket SLA states
- Audit logs
```

The backend is built with .NET 8 and ASP.NET Core Web API using Clean Architecture.

Because the system has a relational domain model, strong business workflows, audit requirements, and reporting-ready data needs, the data access strategy must support:

```text
- Entity mapping
- Relationship configuration
- Query execution
- Transactions
- Migrations
- Indexes
- Change tracking when useful
- Testable persistence boundaries
```

The data access approach must also fit the .NET ecosystem and work well with SQL Server.

## Decision

OpsSphere will use Entity Framework Core as the primary data access technology.

Entity Framework Core will be used for:

```text
- Mapping domain and persistence models to SQL Server tables
- Configuring relationships
- Configuring indexes and constraints
- Running database migrations
- Querying operational data
- Persisting changes
- Managing transactions
- Supporting integration tests with realistic database behavior
```

The EF Core implementation will live in the Infrastructure layer.

Application and Domain layers must not depend directly on EF Core.

The Application layer may depend on abstractions such as repository interfaces, unit-of-work interfaces, or application-specific data access contracts when needed.

## Rationale

Entity Framework Core is a strong fit for OpsSphere because it integrates naturally with .NET, ASP.NET Core, and SQL Server.

OpsSphere needs a data access strategy that can handle relational entities and relationships such as:

```text
Region
  → Country
    → Account
      → Campaign
        → Ticket
```

It also needs to support relationships between tickets, customers, assigned agents, supervisors, comments, escalations, SLA state, and audit records.

EF Core provides a productive way to model these relationships while keeping the database schema aligned with the application through migrations.

EF Core also supports the portfolio goal of demonstrating enterprise .NET development with modern backend practices.

Used correctly, EF Core can support Clean Architecture by remaining inside Infrastructure while the core business logic stays in Domain and Application layers.

## Consequences

### Positive Consequences

- EF Core works naturally with .NET 8 and ASP.NET Core.
- EF Core integrates well with SQL Server.
- Migrations provide a controlled way to evolve the database schema.
- Relationships, constraints, and indexes can be configured in code.
- LINQ queries improve developer productivity.
- Change tracking can simplify create, update, and delete workflows.
- EF Core supports transaction handling for workflows such as ticket creation plus audit logging.
- EF Core can support integration testing with Testcontainers and SQL Server.
- The project demonstrates a common enterprise .NET skillset.

### Negative Consequences

- EF Core can hide inefficient queries if not used carefully.
- Poorly designed LINQ queries may generate slow SQL.
- Lazy loading, if enabled, can cause unexpected query behavior.
- Developers must understand tracking vs. no-tracking queries.
- Migrations must be managed carefully to avoid schema drift.
- Complex reporting queries may require explicit SQL, views, projections, or optimized read models.
- There is a risk of leaking EF Core types into Application or Domain layers if boundaries are not respected.

## Alternatives Considered

### Dapper

Dapper is a lightweight micro-ORM that gives more direct control over SQL.

Rejected as the primary data access strategy because OpsSphere has many relational entities, relationships, migrations, and CRUD-heavy operational workflows where EF Core provides more productivity.

Dapper may still be considered later for performance-critical read queries, reporting views, exports, or dashboards where hand-optimized SQL is justified.

### Raw ADO.NET

Raw ADO.NET provides full control over database access.

Rejected because it would require too much manual mapping and boilerplate for the initial version of OpsSphere.

ADO.NET may be useful for very specific low-level optimizations, but it should not be the default approach.

### Stored Procedure-Centric Access

A stored procedure-centric approach would place much of the data access and workflow behavior inside SQL Server.

Rejected because OpsSphere needs business rules and use cases to remain testable in the application code. SQL Server should store and protect data, but it should not become the main owner of application behavior.

Stored procedures may be considered later for specialized reporting, data maintenance, or performance-sensitive operations.

### No ORM

Avoiding an ORM would maximize control but increase implementation effort.

Rejected because OpsSphere is intended to demonstrate maintainable enterprise .NET development. EF Core provides the right balance between productivity, structure, and integration with SQL Server.

## Implementation Notes

EF Core should be implemented inside:

```text
src/
  OpsSphere.Infrastructure/
    Persistence/
      OpsSphereDbContext.cs
      Configurations/
      Migrations/
      SeedData/
```

Recommended configuration style:

```text
src/
  OpsSphere.Infrastructure/
    Persistence/
      Configurations/
        UserConfiguration.cs
        RoleConfiguration.cs
        PermissionConfiguration.cs
        RegionConfiguration.cs
        CountryConfiguration.cs
        AccountConfiguration.cs
        CampaignConfiguration.cs
        CustomerConfiguration.cs
        TicketConfiguration.cs
        TicketCommentConfiguration.cs
        TicketEscalationConfiguration.cs
        AuditLogConfiguration.cs
```

Recommended practices:

```text
- Keep EF Core in Infrastructure.
- Do not reference DbContext from Domain entities.
- Do not place business rules inside EF Core configuration.
- Use migrations for schema evolution.
- Use explicit relationship configuration for important entities.
- Use indexes for common filters and dashboard queries.
- Use AsNoTracking for read-only queries when appropriate.
- Use projections for list, dashboard, and reporting views.
- Avoid lazy loading by default.
- Keep audit-sensitive operations inside explicit application workflows.
- Use transactions when a workflow must persist multiple related changes.
```

Common indexed query areas may include:

```text
- Tickets by status
- Tickets by priority
- Tickets by account
- Tickets by campaign
- Tickets by assigned agent
- Tickets by supervisor
- Tickets by SLA state
- Tickets by created date
- Audit logs by entity
- Audit logs by user
- Audit logs by date
```

The application should avoid returning EF Core entities directly from API endpoints.

Preferred API flow:

```text
Controller
  → Application Command or Query
  → Handler
  → Data access abstraction or DbContext through Infrastructure boundary
  → DTO / Response model
  → Controller response
```

## Decision Scope

This ADR defines Entity Framework Core as the primary data access technology for OpsSphere.

It does not define the final database schema, table list, migration naming convention, repository pattern usage, or query optimization strategy.

Those details belong in database design, implementation tasks, and performance tuning work.