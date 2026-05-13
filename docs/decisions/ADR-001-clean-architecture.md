# ADR-001: Use Clean Architecture

## Status

Accepted

## Context

OpsSphere is an enterprise support operations platform designed for BPO and contact center environments.

The system must support ticket management, customer management, SLA tracking, escalation workflows, internal comments, dashboards, audit history, authentication, authorization, and role-based access control across regions, countries, accounts, campaigns, supervisors, agents, managers, and viewers.

Because the system contains business workflows, authorization rules, audit requirements, and operational constraints, the project needs an architecture that keeps business logic separated from technical details such as controllers, database access, frontend implementation, authentication infrastructure, logging, and external services.

The project also needs to remain maintainable and testable as the platform grows through MVP, Phase 2, and future enterprise-ready capabilities.

## Decision

OpsSphere will use Clean Architecture as the primary backend architecture style.

The backend will be organized around the following layers:

```text
Presentation
  → Application
    → Domain
      ← Infrastructure
```

The dependency direction must point inward.

The Domain layer must not depend on:

```text
- ASP.NET Core controllers
- Angular frontend
- Entity Framework Core
- SQL Server
- JWT infrastructure
- Logging infrastructure
- Email providers
- Azure services
- External integrations
```

The Application layer will coordinate use cases through commands, queries, handlers, validators, authorization coordination, transaction boundaries, and audit coordination.

The Infrastructure layer will implement technical concerns such as persistence, database access, authentication services, token generation, logging, and future external integrations.

## Rationale

Clean Architecture is used because OpsSphere has business rules that should remain independent from frameworks and infrastructure.

Examples of business rules include:

```text
- A ticket must be linked to a customer.
- A ticket must be linked to an account and campaign.
- A ticket must follow valid status transitions.
- A ticket must be resolved before it can be closed.
- Closed tickets cannot be modified unless reopened by an authorized role.
- Escalated tickets must include an escalation reason.
- Internal comments cannot be empty.
- Users must operate within their assigned role and operational scope.
- Critical business actions must be recorded in audit history.
```

These rules should not be hidden inside controllers, database queries, UI logic, or framework-specific code.

Clean Architecture supports the main quality goals of OpsSphere:

```text
Maintainability:
The system can evolve without mixing business logic with infrastructure details.

Testability:
Domain rules and application use cases can be tested without depending on the UI, database, or external services.

Security:
Authorization rules can be coordinated in the application layer and enforced consistently by the backend.

Auditability:
Critical workflows can consistently produce audit records as part of application use cases.

Scalability readiness:
Future modules, integrations, workers, reporting exports, and Azure services can be added without rewriting the core domain.

Operational clarity:
The architecture reflects the business model of tickets, customers, SLAs, escalations, roles, scopes, and audit history.
```

## Consequences

### Positive Consequences

- Business logic remains independent from ASP.NET Core controllers.
- The domain model can focus on operational rules instead of persistence concerns.
- Application use cases become explicit and easier to test.
- Infrastructure details can be replaced or extended with less impact on the core system.
- The project demonstrates enterprise-level architecture practices.
- The backend structure supports future growth, including background jobs, notifications, reporting exports, and Azure deployment.
- The system is better prepared for automated testing.

### Negative Consequences

- The initial project structure is more complex than a simple CRUD application.
- More folders, projects, interfaces, commands, queries, and handlers may be required.
- Developers must understand dependency direction and layer responsibilities.
- There is a risk of overengineering if the pattern is applied too rigidly.
- Simple features may require more files than in a traditional layered or controller-based approach.

## Alternatives Considered

### Simple CRUD Architecture

A simple CRUD architecture would place most logic directly in controllers or services close to the API layer.

Rejected because OpsSphere is not only a basic ticketing system. It includes business workflows, role-based access, scope-based visibility, SLA behavior, audit history, and operational rules that would become hard to maintain if mixed into controllers.

### Traditional Layered Architecture

A traditional layered architecture could separate UI, business logic, and data access.

Rejected as the primary style because traditional layering often allows business logic to become dependent on infrastructure or database models. OpsSphere needs stronger protection around the Domain and Application layers.

### Database-Centric Architecture

A database-centric approach would rely heavily on stored procedures, database rules, or direct data access patterns.

Rejected because the project needs testable business workflows in the application code, not only database-level behavior. SQL Server remains important, but it should not become the main owner of application behavior.

### Microservices Architecture

A microservices approach would split the platform into multiple independently deployable services.

Rejected for the initial version because it would add unnecessary operational complexity. OpsSphere should start as a modular monolith using Clean Architecture and only consider service extraction if future scale or team structure justifies it.

## Implementation Notes

The initial backend solution should follow this structure:

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

Controllers should remain thin.

Application handlers should coordinate use cases.

Domain entities and domain services should protect business invariants.

Infrastructure should implement persistence, authentication, logging, and external service details.

## Decision Scope

This ADR applies to the backend architecture of OpsSphere.

It does not define the frontend architecture, database engine, authentication mechanism, or ORM choice. Those decisions are documented in separate ADRs.