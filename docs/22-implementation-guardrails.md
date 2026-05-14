# Implementation Guardrails

## Purpose

This document is the canonical implementation guardrails document for OpsSphere.
It keeps future implementation issues aligned with the project architecture, MVP boundaries, validation expectations, and Definition of Done.

Use this document when creating implementation issues, planning branches, opening pull requests, and reviewing changes.

## Repository Implementation Workflow

OpsSphere implementation work is issue-driven.

Each implementation issue should define:

- Scope.
- Out of scope.
- Acceptance criteria.
- Validation expectations.
- Related documentation or context.

Branches should include the issue number.

Recommended branch names:

```text
docs/issue-<number>-short-description
feature/issue-<number>-short-description
fix/issue-<number>-short-description
chore/issue-<number>-short-description
```

Pull requests should reference and close the related issue.

Example PR footer:

```text
Closes #24
```

Commit examples:

```text
docs: add implementation guardrails
chore: update PR checklist
feat: add ticket creation command
test: add ticket workflow tests
ci: add backend validation pipeline
fix: enforce ticket closure rule
```

## Backend Guardrails

Future backend structure:

```text
src/
  OpsSphere.Api/
  OpsSphere.Application/
  OpsSphere.Domain/
  OpsSphere.Infrastructure/

tests/
  OpsSphere.Domain.Tests/
  OpsSphere.Application.Tests/
  OpsSphere.Api.Tests/
  OpsSphere.IntegrationTests/
```

Backend implementation uses:

- .NET 10.
- ASP.NET Core Web API.
- Clean Architecture.
- Inward dependency direction.

Dependency rules:

- Domain must not depend on API.
- Domain must not depend on Infrastructure.
- Domain must not depend on EF Core.
- Domain must not depend on SQL Server.
- Domain must not depend on JWT.
- Domain must not depend on logging infrastructure.
- Domain must not depend on Angular.
- Application should coordinate use cases through commands, queries, handlers, validators, authorization coordination, and audit coordination.
- Infrastructure implements persistence, external infrastructure, and technical services behind Application abstractions.
- API exposes transport endpoints and delegates behavior inward.

Controller rules:

- Controllers should stay thin.
- Controllers should delegate to commands and queries, preferably through MediatR.
- Business rules should not live in controllers.
- Business rules should not live in Angular.
- Backend authorization is the source of truth.

## Frontend Guardrails

Future frontend structure:

```text
frontend/
  src/
    app/
      core/
      shared/
      features/
```

Frontend implementation uses:

- Angular.
- TypeScript.
- RxJS.
- Angular Router.
- Reactive Forms.
- Guards where appropriate.
- Interceptors where appropriate.

UI foundation:

- Angular Material is the primary UI component foundation for the MVP.
- Tailwind may be considered later only by future explicit decision.
- Bootstrap is not approved for the initial MVP unless a future ADR introduces it.

Security boundary:

- Frontend role and scope checks are UX-only.
- Frontend visibility never replaces backend authorization.
- Backend authorization remains the security boundary.

Frontend foundation boundary:

- Foundation issues may add only shell and placeholder Angular components.
- Business screens such as login, dashboard, tickets, customers, users, SLA, audit, and reports should wait for explicitly scoped feature issues.

## Database Guardrails

Database implementation uses:

- SQL Server as the primary relational database.
- Entity Framework Core as the primary data access technology in Infrastructure.

Dependency rules:

- Application must not depend directly on EF Core.
- Domain must not depend directly on EF Core.
- EF Core DbContext, configurations, migrations, and database-specific implementation details belong in Infrastructure.

Migration rules:

- Migrations are not part of Sprint 0.
- Future migrations should be reviewed before merge.
- Future migrations should be clearly named.
- Future migrations should be tested locally.
- Avoid destructive changes unless explicitly justified in the issue and PR.

Historical data rules:

- Use deactivation or soft deletion when historical records depend on existing data.
- Preserve auditability and historical traceability.
- Avoid physical deletion for operational records tied to tickets, audit logs, assignments, SLA state, or historical reporting.

## API Guardrails

Business API routes use:

```text
/api
```

Examples:

```http
GET /api/tickets
POST /api/tickets
POST /api/tickets/{id}/assign
POST /api/tickets/{id}/escalate
POST /api/tickets/{id}/resolve
POST /api/tickets/{id}/close
```

API rules:

- Controllers stay thin.
- Business workflow actions such as assign, escalate, resolve, and close may use action-style subroutes.
- Success response shapes should be consistent.
- Error response shapes should be consistent.
- Protected business endpoints use JWT authentication.
- Authorization combines role, permission, and operational scope.
- Frontend visibility never replaces backend authorization.

## Health Endpoint Decision

Health endpoints are operational endpoints, not business resources.

Canonical health routes:

```http
GET /health
GET /health/details
```

Guardrails:

- The canonical health route is not `/api/health` unless a future ADR changes it.
- `/health` should expose only readiness-safe information.
- `/health/details` should be protected or restricted if it exposes sensitive operational details.

## Testing Guardrails By Issue Type

Every issue should state its validation expectations before work begins.

Documentation issues:

- Review changed Markdown for clarity, links, headings, and scope alignment.
- Confirm no source code, package, migration, Docker, or GitHub Actions changes were included unless explicitly scoped.
- Confirm related templates or README references are updated only when useful.

Backend issues:

- Run relevant .NET build and test commands when projects exist.
- Add or update unit tests for Domain and Application rules.
- Add integration or API tests for persistence, authorization, scope filtering, audit side effects, and important workflow behavior.
- Include negative authorization and cross-scope tests where applicable.
- Confirm controllers remain thin and business rules remain outside API transport code.

Frontend issues:

- Run relevant Angular build, lint, and test commands when the frontend exists.
- Test route guards, interceptors, forms, validation, loading states, error states, and role-aware UI where applicable.
- Confirm UI role and scope checks are treated as UX only.
- Confirm backend authorization remains the security boundary.

Database issues:

- Test migrations locally against SQL Server when migrations are part of the issue.
- Verify migration names are clear and changes are reviewable.
- Confirm Application and Domain do not gain direct EF Core dependencies.
- Confirm destructive database changes are avoided or explicitly justified.
- Validate deactivation, soft deletion, auditability, and historical traceability where applicable.

DevOps issues:

- Validate local setup commands and CI commands affected by the issue.
- Confirm no secrets, tokens, passwords, or real connection strings are committed.
- Confirm pipeline changes are minimal, documented, and aligned with the issue.
- Confirm build and test outputs are suitable for pull request review.

Observability issues:

- Validate health, logging, correlation ID, and error behavior affected by the issue.
- Confirm logs avoid sensitive data.
- Confirm `/health/details` is protected or restricted if detailed checks expose sensitive data.
- Confirm operational signals support troubleshooting without leaking business or credential data.

## Definition of Done

An implementation issue is done when:

- The work matches the related issue scope.
- Out-of-scope items were not added.
- Acceptance criteria are satisfied.
- Validation requested by the issue was completed or an explicit reason is documented.
- PR description references and closes the related issue.
- Branch name includes the issue number.
- Code, documentation, and templates follow the established project conventions.
- Backend authorization remains the source of truth for protected behavior.
- Business rules are implemented in Domain or Application, not controllers or Angular.
- Controllers remain thin when backend endpoints are involved.
- Domain and Application dependency boundaries are preserved.
- EF Core remains isolated to Infrastructure.
- Role, permission, and operational scope behavior is validated where applicable.
- Auditability and historical traceability are preserved where applicable.
- Database changes avoid destructive behavior unless explicitly justified.
- Tests are added or updated according to the issue type and risk.
- Local validation commands are run when the relevant projects exist.
- Documentation is updated when behavior, architecture, setup, or validation expectations change.
- No real data, secrets, tokens, passwords, or production connection strings are committed.
- MVP boundaries are respected.

## MVP Boundaries

The MVP includes:

- Realistic operational hierarchy.
- Internal user authentication.
- JWT authentication.
- Role-based access control.
- Permission and operational scope enforcement.
- Basic administration of users, roles, scopes, and operational structure.
- Customer records only as internal ticket context.
- Ticket lifecycle management.
- Ticket ownership visibility.
- Internal comments.
- Simple deterministic SLA tracking.
- Audit history for important changes.
- Basic scoped operational dashboards.
- Filters and structured data that can support external reporting tools.
- Clean backend architecture.
- Testable workflows.
- Fictional sample data only.

The MVP excludes:

- Full business intelligence platform behavior.
- Advanced Power BI dashboard design.
- Replacement for Power BI.
- AI-assisted ticket classification.
- Predictive SLA risk.
- Advanced SLA calendars, pause rules, business-hour calculations, or predictive SLA modeling.
- Telephony integration.
- Call recording.
- Omnichannel integrations.
- Workforce scheduling.
- Payroll.
- HR management.
- External customer portal.
- Real-time chat.
- Mobile app.
- Complex notification engine.
- Enterprise SSO.
- Multi-tenant billing.
- Complex workflow automation.
- Replacement for CRM systems.
- Replacement for workforce management tools.

Practical boundary rule:

- Build OpsSphere as an internal BPO/contact-center operations platform, not a generic ticketing system and not a broad enterprise suite.

## Related Documents

- `docs/05-scope-and-roadmap.md`
- `docs/11-architecture-overview.md`
- `docs/14-database-design.md`
- `docs/15-api-design.md`
- `docs/16-security-and-permissions.md`
- `docs/17-testing-strategy.md`
- `docs/18-deployment-and-devops.md`
- `docs/19-observability-and-support.md`
- `docs/21-implementation-roadmap.md`
