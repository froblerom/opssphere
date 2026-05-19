# OpsSphere Agent Progress - Current State

## Purpose

This file tracks the current state of the OpsSphere repository for AI-assisted work.

It should be updated when major documentation, architecture, implementation, testing, or delivery milestones are completed.

The goal is to give future agents a short operational memory without forcing them to read the entire repository.

---

## Current Project Phase

```text
MVP complete. Release readiness phase.
```

OpsSphere has completed full-stack MVP implementation across backend, frontend, testing, and CI. The current focus is release validation, documentation finalization, and preparing for MVP sign-off.

---

## Current Product Identity

```text
OpsSphere is an enterprise support operations platform for multinational BPO/contact center environments.
```

It is not:

```text
A generic ticketing system
A CRM replacement
A Power BI replacement
A telephony platform
A workforce management system
An AI automation suite
```

---

## Completed Milestones

```text
Milestone 1 — Technical Foundation
  .NET 10 / ASP.NET Core Web API solution structure
  Clean Architecture (Domain → Application ← Infrastructure → API)
  EF Core migrations and SQL Server integration
  JWT authentication with role + permission + scope-based authorization
  Docker Compose local infrastructure
  Serilog structured logging

Milestone 2 — Core Domain Implementation
  User and role management (CRUD, deactivation, scope assignment)
  Organization hierarchy management (Region → Country → Account → Campaign)
  Customer management with ticket history
  Ticket lifecycle: create, assign, status workflow, priority, comment, escalate, resolve, close
  SLA evaluation (wall-clock, four states: WithinSla, AtRisk, Breached, Completed)
  Audit log (immutable, scoped, lifecycle-complete)

Milestone 3 — Operational Dashboard and Filters
  Operational dashboard with totals, groupings (status, priority, SLA state, account, campaign, agent, supervisor)
  Dashboard filters (account, campaign, status, date range) with scope intersection
  SLA summary endpoint
  Dashboard drill-ins to filtered ticket list

Milestone 4 — Angular Frontend MVP
  Standalone Angular 21.x components with Angular Material
  Lazy-loaded routes with authGuard and permissionGuard
  Full ticket lifecycle UI workflows
  Role-aware navigation (admin, supervisor, agent, viewer)
  Dashboard and SLA badge views
  Audit log and entity history panels
  Customer management UI

Milestone 5 — Integration Testing and CI
  OpsSphereSqliteFactory (SQLite in-memory, fictional JWT, full seed data)
  Full integration test suite: auth, authorization, user mgmt, org mgmt, customer mgmt,
    ticket management, ticket lifecycle gaps, audit, SLA, dashboard, health, middleware,
    persistence/seed verification
  MigrationTests (Heavy category, Testcontainers SQL Server)
  GitHub Actions CI: backend-fast, integration-fast, integration-heavy, frontend jobs
  CI documentation: docs/testing/ci-validation.md, docs/testing/mvp-regression-tests.md

Milestone 6 — Release Readiness (current)
  docs/release/mvp-release-checklist.md
  docs/release/mvp-demo-script.md
  docs/release/mvp-uat-checklist.md
  docs/release/mvp-known-limitations.md
  docs/release/phase-2-3-parking-lot.md
  Angular root route (/) redirects to /dashboard on init
```

---

## Current Canonical Technical Direction

```text
Backend:
  .NET 10
  ASP.NET Core Web API
  Clean Architecture
  CQRS without MediatR (direct handler injection)
  EF Core 9.0.5 (Infrastructure only)
  SQL Server (production/local), SQLite in-memory (tests)
  JWT authentication
  PermissionAuthorizationHandler (per-permission policy)
  Role + Permission + Scope-based access control (4-level hierarchy)
  ScopeAuthorizationService (full hierarchy resolution, DB-backed)
  ActiveUserMiddleware (blocks deactivated users, 401)
  GlobalExceptionHandlingMiddleware (safe error envelope, no stack traces)
  CorrelationIdMiddleware (X-Correlation-Id on all responses)
  Serilog (structured, enriched with CorrelationId and UserId)
  Central package management: Directory.Packages.props

Frontend:
  Angular 21.x
  TypeScript
  Angular Material
  Standalone components
  Lazy-loaded routes
  authGuard + permissionGuard on all protected routes
  HTTP interceptors
  RxJS signals

Testing:
  xUnit
  WebApplicationFactory (OpsSphereSqliteFactory)
  Testcontainers.MsSql (Heavy category only)
  Integration tests (SQLite in-memory, fictional seed data)
  [assembly: CollectionBehavior(DisableTestParallelization = true)]
  Frontend unit tests (Karma + ChromeHeadless)

DevOps:
  Docker Compose (local SQL Server)
  GitHub Actions (backend-fast, integration-fast, integration-heavy, frontend)
  OpsSphere.slnx (.slnx format, .NET 10 SDK required)
```

---

## Current MVP Scope

The MVP includes:

```text
Authentication and JWT API access
Role-based and permission-based authorization
Scope-based visibility (Region → Country → Account → Campaign)
User and role management
Operational structure management
Customer management
Ticket lifecycle (create, assign, status, priority, comment, escalate, resolve, close)
SLA tracking (wall-clock, four states)
Internal comments
Escalation queue
Audit log (immutable, lifecycle-complete)
Operational dashboards with scope-filtered metrics
Filtered operational views
GitHub Actions CI pipeline
```

---

## Current MVP Non-Goals

The MVP excludes:

```text
Swagger / OpenAPI endpoint
Automated browser E2E tests (Playwright, Cypress)
Azure deployment
Application Insights / centralized log aggregation
Business-hours SLA calendars
Notification system (email, in-app, webhooks)
Reports module, report builder, export UI
Attachments
Enterprise SSO
External customer portal
Power BI integration
Telephony or omnichannel
Workforce management
Mobile app
```

See `docs/release/mvp-known-limitations.md` and `docs/release/phase-2-3-parking-lot.md` for full details.

---

## Current Documentation Baseline

Core product docs:

```text
docs/00-executive-summary.md through docs/22-implementation-guardrails.md
docs/decisions/
```

Testing and CI docs:

```text
docs/testing/mvp-regression-tests.md
docs/testing/ci-validation.md
docs/smoke-test-22.md
```

Release docs:

```text
docs/release/mvp-release-checklist.md
docs/release/mvp-demo-script.md
docs/release/mvp-uat-checklist.md
docs/release/mvp-known-limitations.md
docs/release/phase-2-3-parking-lot.md
```

Agent harness docs:

```text
docs/agents/
docs/agents/progress/current-state.md
```

---

## Current Critical Rules for Agents

```text
Repository is source of truth.
Do not rely on chat memory when docs exist.
Load minimal context.
Do not expand MVP scope.
Respect Clean Architecture.
Keep controllers thin.
Backend authorization is source of truth.
Frontend visibility is not security.
Enforce role + permission + scope.
Audit critical actions.
Test behavior changes.
Do not modify unrelated files.
Do not commit secrets.
Use fictional sample data only.
No real company, customer, employee, or credential data.
```

---

## Next Recommended Work

```text
MVP sign-off: complete mvp-release-checklist.md and mvp-uat-checklist.md manually.
Run integration-heavy CI job at least once on the release branch.
Complete manual smoke test (docs/smoke-test-22.md) for all personas.
Dry-run the demo script (docs/release/mvp-demo-script.md) end-to-end.
After sign-off: begin Phase 2 planning (Swagger, Azure deployment, E2E automation, SLA enhancements).
```

---

## Last Updated

```text
2026-05-19
```
