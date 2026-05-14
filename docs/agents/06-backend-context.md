# OpsSphere Backend Context

## Purpose

This document provides compressed backend implementation context for agents working inside OpsSphere.

Use this file when the task touches:

- ASP.NET Core Web API
- Application commands and queries
- Domain behavior
- EF Core persistence
- SQL Server
- Authentication
- Authorization
- Audit logging
- API endpoints
- Backend tests

---

## Backend Stack

OpsSphere backend uses:

```text
.NET 10
ASP.NET Core Web API
Clean Architecture
CQRS with MediatR
Entity Framework Core
SQL Server
JWT Authentication
Role-based authorization
Permission-based authorization
Scope-based access control
Serilog
xUnit
WebApplicationFactory
Testcontainers
```

---

## Backend Layer Structure

Expected structure:

```text
src/
  OpsSphere.Api/
    Controllers/
    Middleware/
    Authentication/
    Authorization/
    Filters/
    Program.cs

  OpsSphere.Application/
    Common/
    Abstractions/
    Commands/
    Queries/
    Handlers/
    Validators/
    DTOs/
    Security/
    Auditing/

  OpsSphere.Domain/
    Entities/
    ValueObjects/
    Enums/
    Events/
    Exceptions/
    Services/

  OpsSphere.Infrastructure/
    Persistence/
      OpsSphereDbContext.cs
      Configurations/
      Migrations/
      SeedData/
    Identity/
    Authentication/
    Authorization/
    Logging/
    Auditing/
```

The exact structure may evolve, but Clean Architecture boundaries must remain intact.

---

## Controller Rules

Controllers must:

```text
Receive HTTP requests.
Validate basic model binding.
Require authentication and high-level policies.
Call application commands or queries.
Return consistent API responses.
Map known errors to safe HTTP responses.
```

Controllers must not:

```text
Contain business workflow logic.
Directly use EF Core DbContext for business workflows.
Bypass Application authorization coordination.
Create audit records manually when Application should coordinate them.
Implement domain rules directly.
```

---

## Application Layer Rules

Application handlers must coordinate use cases.

Responsibilities:

```text
Validate command/query intent.
Check authorization and scope.
Load required data through abstractions.
Call domain behavior.
Persist state changes through infrastructure abstractions.
Create audit records for critical actions.
Return application results.
```

Examples:

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

---

## Domain Rules

Domain should protect core invariants.

Examples:

```text
Ticket cannot close before resolution.
Closed ticket cannot be modified unless reopened.
Escalation requires reason.
Internal comment cannot be empty.
Ticket must have customer, account, campaign, priority, and status.
Invalid status transitions are rejected.
```

Do not hide these rules only in API controllers or Angular forms.

---

## Infrastructure Rules

Infrastructure owns:

```text
EF Core DbContext
Entity configurations
SQL Server migrations
Repository implementations if used
Unit of work implementation if used
JWT token generation
Password hashing
Audit persistence
Serilog configuration
External services in future phases
```

Infrastructure must not own core business decisions.

---

## Authentication Rules

OpsSphere uses JWT authentication.

Login flow:

```text
User submits credentials.
API validates credentials.
System verifies active user status.
System loads roles, permissions, and scopes.
System generates JWT.
API returns token and minimal user profile.
Frontend sends token on protected requests.
Backend validates token.
```

Rules:

```text
Only active users may authenticate.
Invalid credentials must be rejected.
Deactivated users must not receive tokens.
JWT payload must not contain sensitive data.
Expired tokens must be rejected.
```

---

## Authorization Rules

Backend authorization is the source of truth.

Authorization model:

```text
Role
+ Permission
+ Operational Scope
```

Decision flow:

```text
1. Is user authenticated?
2. Is user active?
3. Does user have required role?
4. Does user have required permission?
5. Does user have scope over resource?
6. Is resource state valid for action?
7. Does action require audit?
```

Resource-based checks are required for records such as:

```text
Ticket
Customer
Account
Campaign
SLA dashboard data
Audit record
User scope
```

---

## API Response Shape

Use consistent response shapes.

Single resource:

```json
{
  "data": {
    "id": "00000000-0000-0000-0000-000000000000"
  }
}
```

List response:

```json
{
  "data": [],
  "page": 1,
  "pageSize": 25,
  "totalCount": 0,
  "totalPages": 0
}
```

Error response:

```json
{
  "error": {
    "code": "validation_error",
    "message": "The request contains validation errors.",
    "details": [
      {
        "field": "priority",
        "message": "Priority is required."
      }
    ],
    "correlationId": "8e3f0f90-7e56-4fd8-938a-8422f7fcd123"
  }
}
```

---

## API Endpoint Style

Base path:

```text
/api
```

Examples:

```http
POST /api/auth/login
GET /api/auth/me

GET /api/tickets
POST /api/tickets
GET /api/tickets/{id}
PUT /api/tickets/{id}/status
POST /api/tickets/{id}/assign
POST /api/tickets/{id}/comments
POST /api/tickets/{id}/escalate
POST /api/tickets/{id}/resolve
POST /api/tickets/{id}/close

GET /api/users
POST /api/users
PUT /api/users/{id}
PUT /api/users/{id}/roles
PUT /api/users/{id}/scopes
POST /api/users/{id}/deactivate
```

Business workflow subroutes are acceptable for domain actions.

---

## Persistence Rules

Use SQL Server and EF Core.

Important persistence principles:

```text
Use relational integrity.
Use foreign keys for important relationships.
Use indexes for common filters.
Preserve historical traceability.
Use UTC timestamps.
Prefer deactivation or soft delete where records are historically referenced.
Avoid deleting users, accounts, campaigns, customers, or tickets when audit/history depends on them.
```

---

## Audit Rules

Application workflows must coordinate audit records for critical actions.

Audit records should include:

```text
Actor user id
Action
Entity type
Entity id
Timestamp UTC
Previous value when applicable
New value when applicable
Reason when applicable
Correlation ID when useful
```

---

## Logging Rules

Use structured logging.

Recommended fields:

```text
Timestamp
Level
Message
CorrelationId
RequestId
UserId
UserRole
TraceId
Endpoint
HttpMethod
StatusCode
ElapsedMilliseconds
ExceptionType
Environment
ApplicationVersion
```

Do not log:

```text
Passwords
Password hashes
JWT tokens
Refresh tokens
Connection strings
API keys
Secret values
Sensitive personal data
```

---

## Backend Validation Checklist

Before declaring backend work complete:

```text
Does the code build?
Are controllers thin?
Is business logic in Application/Domain?
Are authorization and scope checks enforced backend-side?
Are audit-sensitive actions audited?
Are database changes traceable and safe?
Are tests added or updated?
Do API responses follow expected shape?
Are errors safe and consistent?
Are logs safe?
```
