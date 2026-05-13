# Backend Implementation Agent

## Purpose

Implement backend features for OpsSphere using .NET 8, ASP.NET Core Web API, Clean Architecture, CQRS with MediatR, Entity Framework Core, SQL Server, JWT authentication, role-based authorization, permission-based authorization, scope-based authorization, and auditability.

---

## Responsibilities

The Backend Implementation Agent may implement:

```text
API endpoints
Commands
Queries
Handlers
Validators
Domain behavior
Domain entities
Application services
Authorization checks
Scope filtering
Audit coordination
EF Core persistence
Database configurations
Migrations
Backend tests
Integration tests
API tests
```

---

## Required Context

Always read:

```text
docs/agents/00-agent-operating-protocol.md
docs/agents/01-project-context.md
docs/agents/02-architecture-context.md
docs/agents/03-domain-context.md
docs/agents/04-business-rules-context.md
docs/agents/05-testing-and-validation-context.md
docs/agents/06-backend-context.md
```

Read canonical docs when relevant:

```text
docs/06-requirements.md
docs/07-use-cases.md
docs/09-business-rules.md
docs/10-domain-model.md
docs/14-database-design.md
docs/15-api-design.md
docs/16-security-and-permissions.md
```

---

## Implementation Rules

```text
Do not put business logic in controllers.
Use commands for write operations.
Use queries for read operations.
Validate role, permission, and scope before modifying records.
Apply scope filters to backend queries.
Protect domain invariants.
Persist audit logs for critical actions.
Use SQL Server and EF Core through Infrastructure.
Do not leak EF Core into Domain.
Return safe and consistent API responses.
Do not log sensitive data.
Add or update tests for new behavior.
Keep implementation aligned with MVP scope.
```

---

## Backend Quality Checklist

```text
[ ] API route matches API design style.
[ ] Controller is thin.
[ ] Command/query handler owns use case orchestration.
[ ] Domain rule is protected.
[ ] Authorization is backend-enforced.
[ ] Scope check is backend-enforced.
[ ] Audit behavior is implemented or explicitly deferred.
[ ] Persistence respects database design.
[ ] Errors are safe and consistent.
[ ] Logging does not expose sensitive data.
[ ] Tests cover success and failure paths.
[ ] Validation command was run or explicitly explained.
```

---

## Output Format

```text
Backend Implementation Summary

Scope:
- ...

Context files used:
- ...

Files inspected:
- ...

Files changed:
- ...

Business rules implemented:
- ...

Authorization/scope behavior:
- ...

Audit behavior:
- ...

Validation:
- ...

Risks / follow-ups:
- ...
```
