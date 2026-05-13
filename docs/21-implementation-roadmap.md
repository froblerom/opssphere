# Implementation Roadmap

## Purpose

This document is the canonical implementation roadmap for OpsSphere MVP development.
It translates the planning, architecture, domain, database, API, security, testing, DevOps, observability, and risk documentation into a sequence of small implementation issues.

OpsSphere is an internal BPO/contact-center operations platform. The MVP must prove operational structure, ticket execution, simple SLA tracking, auditability, RBAC, scope-based visibility, and scoped dashboards. It must not become a generic ticketing system.

## Roadmap Principles

- Build technical foundations before business workflows.
- Favor small issue-based pull requests with narrow acceptance criteria.
- Keep backend authorization as the source of truth.
- Treat frontend role visibility as UX only, never as a security boundary.
- Keep business rules out of controllers.
- Keep EF Core inside Infrastructure.
- Keep Domain and Application independent from ASP.NET Core, EF Core, SQL Server, Angular, JWT infrastructure, and logging infrastructure.
- Introduce authentication, RBAC, permissions, and scope checks before protected business APIs.
- Introduce audit and minimal SLA foundations before ticket workflows depend on them.
- Preserve auditability and historical context through append-only audit records, lifecycle history, and deactivation instead of destructive removal.
- Use fictional seed data only.
- Use deterministic MVP SLA evaluation; defer background workers, advanced calendars, and predictive SLA.
- Keep AI classification, external customer portal, enterprise SSO, advanced Power BI dashboards, telephony, omnichannel, workforce management, and advanced automation out of MVP.

## Canonical Inputs

Agent operating and context inputs:

- `docs/agents/00-agent-operating-protocol.md`
- `docs/agents/01-project-context.md`
- `docs/agents/02-architecture-context.md`
- `docs/agents/03-domain-context.md`
- `docs/agents/04-business-rules-context.md`
- `docs/agents/05-testing-and-validation-context.md`
- `docs/agents/06-backend-context.md`
- `docs/agents/07-frontend-context.md`
- `docs/agents/08-devops-context.md`
- `docs/agents/09-observability-context.md`
- `docs/agents/11-prompt-levels.md`
- `docs/agents/12-prompt-classifier.md`

Canonical product and architecture inputs:

- `docs/03-project-charter.md`
- `docs/04-stakeholders.md`
- `docs/05-scope-and-roadmap.md`
- `docs/06-requirements.md`
- `docs/07-use-cases.md`
- `docs/08-business-process-flows.md`
- `docs/09-business-rules.md`
- `docs/10-domain-model.md`
- `docs/11-architecture-overview.md`
- `docs/12-c4-architecture.md`
- `docs/13-uml-diagrams.md`
- `docs/14-database-design.md`
- `docs/15-api-design.md`
- `docs/16-security-and-permissions.md`
- `docs/17-testing-strategy.md`
- `docs/18-deployment-and-devops.md`
- `docs/19-observability-and-support.md`
- `docs/20-risk-register.md`

Architecture decision records:

- `docs/decisions/ADR-001-clean-architecture.md`
- `docs/decisions/ADR-002-sql-server.md`
- `docs/decisions/ADR-003-jwt-authentication.md`
- `docs/decisions/ADR-004-angular-frontend.md`
- `docs/decisions/ADR-005-entity-framework-core.md`

Note: ADRs are stored in `docs/decisions/` in this repository.

## MVP Implementation Strategy

The MVP should be implemented as a modular monolith with .NET, ASP.NET Core Web API, Angular, SQL Server, EF Core, JWT authentication, RBAC, scope-based access, structured logging, health checks, and automated tests.

Implementation should proceed in this order:

1. Repository, solution, frontend, Docker, database, CI, error handling, health, logging, and test harness foundations.
2. Domain, persistence, seed data, authentication, authorization, permissions, scope filtering, minimal audit, and minimal SLA foundations.
3. Administrative capabilities for users, roles, scopes, and operational hierarchy.
4. Customer and ticket workflows, each with backend authorization, scope checks, audit records, and SLA behavior where applicable.
5. Dashboards, frontend integration, integration tests, E2E smoke tests, CI hardening, and MVP stabilization.

Sequence refinement: Sprints 19 and 20 complete SLA tracking and audit views, but minimal SLA and audit primitives must be introduced earlier in Sprints 4, 7, 8, and 12 so ticket creation and lifecycle work remain auditable from the start.

## Sprint / Issue Sequence

### Sprint 0: Repository and Solution Foundation

- Goal: Establish repository guardrails and the first implementation backlog shape.
- Why this comes now: Every later issue depends on consistent conventions, validation expectations, and MVP boundaries.
- Deliverables: Repository README updates, issue template, PR checklist, Definition of Done, validation checklist, initial solution/frontend folder plan.
- Backend scope: Define backend layer naming, dependency direction, controller thinness, and command/query expectations.
- Frontend scope: Define Angular folder conventions and UX-only authorization rule.
- Database scope: Confirm SQL Server and EF Core migration expectations.
- Testing scope: Define required unit, integration, API, and E2E test expectations by issue type.
- DevOps / Observability scope: Define expected local, CI, logging, and health validation commands.
- Out of scope: Application features, migrations, endpoints, and Angular components.
- Exit criteria: Future GitHub issues can be written with clear scope, acceptance criteria, validation, and MVP/non-MVP boundaries.
- Suggested GitHub issue title: `chore: establish repository implementation guardrails`

### Sprint 1: Backend Clean Architecture Solution Structure

- Goal: Create the backend solution structure that enforces Clean Architecture.
- Why this comes now: Domain, Application, API, and Infrastructure boundaries must exist before backend features.
- Deliverables: API, Application, Domain, Infrastructure, and test projects with correct references.
- Backend scope: Dependency direction, DI extension placeholders, thin controller convention, CQRS/MediatR and validation pipeline placeholders aligned with the architecture documentation.
- Frontend scope: None.
- Database scope: Infrastructure persistence folders only.
- Testing scope: Build smoke test and optional architecture dependency tests.
- DevOps / Observability scope: Document local backend build command.
- Out of scope: Business endpoints, EF mappings, migrations, and authentication.
- Exit criteria: Backend builds and Domain/Application do not depend on API, Infrastructure, EF Core, SQL Server, JWT, or logging infrastructure.
- Suggested GitHub issue title: `feat: scaffold backend Clean Architecture solution`

### Sprint 2: Angular Frontend Foundation

- Goal: Create the Angular application foundation.
- Why this comes now: Frontend feature work needs routing, shared services, guards, API client patterns, and layout conventions.
- Deliverables: Angular app, core/shared/features folders, routing shell, API client convention, auth interceptor placeholder, guard placeholder.
- Backend scope: None.
- Frontend scope: App shell, navigation shell, environment configuration, route organization, shared model conventions.
- Database scope: None.
- Testing scope: Frontend build smoke test and first component/service test.
- DevOps / Observability scope: Document frontend validation commands.
- Out of scope: Completed business screens or security enforcement.
- Exit criteria: Angular app builds and can host future authenticated feature routes.
- Suggested GitHub issue title: `feat: scaffold Angular frontend foundation`

### Sprint 3: Docker Compose and Local SQL Server Setup

- Goal: Add repeatable local infrastructure for SQL Server.
- Why this comes now: Persistence, migrations, integration tests, and local development need a stable database path.
- Deliverables: `docker-compose.yml`, `.env.example`, local setup notes, appsettings structure for local connection strings.
- Backend scope: Configuration binding placeholders for database and future JWT settings.
- Frontend scope: API base URL environment convention.
- Database scope: Local SQL Server container with named volume and documented reset path.
- Testing scope: Manual container startup validation.
- DevOps / Observability scope: No committed secrets; local-only configuration documented.
- Out of scope: Azure deployment, production secrets, and application schema.
- Exit criteria: A developer can start local SQL Server without real credentials committed to the repository.
- Suggested GitHub issue title: `chore: add Docker Compose SQL Server setup`

### Sprint 4: EF Core Persistence Foundation and Initial Migration

- Goal: Establish Infrastructure persistence with EF Core and SQL Server.
- Why this comes now: Authentication, authorization, scope, ticket, SLA, and audit behavior all require a relational model.
- Deliverables: DbContext, entity configurations, initial migration, UTC timestamp convention, indexes, base persistence abstractions.
- Backend scope: Infrastructure persistence only; Application repository/unit-of-work abstractions where needed.
- Frontend scope: None.
- Database scope: Users, roles, permissions, user roles, role permissions, user scopes, regions, countries, accounts, campaigns, customers, tickets, comments, assignments, status history, escalations, resolutions, SLA policies/states, and audit logs.
- Testing scope: DbContext and migration smoke tests against SQL Server or Testcontainers where practical.
- DevOps / Observability scope: Database health check can be wired once API health exists.
- Out of scope: CRUD APIs, seed data, and business workflows.
- Exit criteria: Migration applies cleanly and EF Core remains isolated to Infrastructure.
- Suggested GitHub issue title: `feat: add EF Core persistence foundation and initial migration`

### Sprint 5: Seed Data Foundation for Roles, Permissions, and Fictional Organization Data

- Goal: Add deterministic fictional seed data for local development, demos, and tests.
- Why this comes now: Auth, permissions, scopes, hierarchy, and tests need known data.
- Deliverables: Role catalog, permission catalog, role-permission mappings, fictional users, fictional regions/countries/accounts/campaigns, initial scopes.
- Backend scope: Seed service or migration-based seed strategy.
- Frontend scope: None.
- Database scope: Fictional data only; no production-like secrets or real customer records.
- Testing scope: Seed validation tests for required roles, permissions, and scope assignments.
- DevOps / Observability scope: Document local seed/reset behavior.
- Out of scope: Real data import and production identity integration.
- Exit criteria: Local database can be seeded repeatably with fictional personas and organization structure.
- Suggested GitHub issue title: `feat: add fictional seed data and permission catalog`

### Sprint 6: Authentication Foundation with JWT

- Goal: Implement internal user authentication with JWT.
- Why this comes now: Protected APIs and frontend authenticated routes require reliable identity.
- Deliverables: Login endpoint, current-user endpoint, password hashing, JWT issuance, minimal claims.
- Backend scope: Auth commands/queries, active-user checks, token service in Infrastructure, safe login failure behavior.
- Frontend scope: Login screen, auth service, token storage strategy, authenticated route guard.
- Database scope: Credential fields, last-login tracking if documented, active/deactivated user state.
- Testing scope: Successful login, invalid login, deactivated-user rejection, protected endpoint without token.
- DevOps / Observability scope: No passwords or tokens in logs.
- Out of scope: Refresh tokens, MFA, enterprise SSO, external customer login.
- Exit criteria: Active seeded internal users can authenticate and invalid/deactivated users cannot.
- Suggested GitHub issue title: `feat: implement JWT authentication foundation`

### Sprint 7: Authorization Foundation with RBAC and Scope Model

- Goal: Enforce backend permissions and operational scope access.
- Why this comes now: Organization, customer, ticket, dashboard, report, audit, and lookup APIs must not exist before authorization foundations.
- Deliverables: Permission policies, role checks, scope service, resource access checks, query filter helpers.
- Backend scope: Backend-only source of truth for role, permission, scope, resource state, and active-user access.
- Frontend scope: Role-aware navigation driven by authenticated user data.
- Database scope: Query support and indexes for role/permission/scope access.
- Testing scope: Viewer write denial, Agent admin denial, cross-scope denial, scoped list filtering.
- DevOps / Observability scope: Safe structured logs for authorization and scope denials.
- Out of scope: Business CRUD and frontend security as a source of truth.
- Exit criteria: Backend can authorize actions and filter scoped resources independently of the frontend.
- Suggested GitHub issue title: `feat: implement RBAC permissions and scope authorization`

### Sprint 8: Global Error Handling, Correlation ID, Logging, and Health Checks

- Goal: Add supportability foundations before business APIs multiply.
- Why this comes now: Errors, logs, correlation IDs, and health endpoints should be consistent from the first feature endpoint.
- Deliverables: Global exception handling, canonical error envelope, correlation ID middleware, structured logging, `/health`, `/health/details`.
- Backend scope: Safe error responses, request logging, database/configuration health checks where available.
- Frontend scope: Shared API error handling model.
- Database scope: Database health check integration.
- Testing scope: Health endpoint tests, error envelope tests, correlation ID propagation tests.
- DevOps / Observability scope: `/health` is public/readiness-safe; `/health/details` is protected or disabled when sensitive in production.
- Out of scope: Full Application Insights production integration and alerting.
- Exit criteria: API responses, logs, and health endpoints are consistent and safe. The error response shape from `docs/15-api-design.md` is the canonical implementation contract, with `docs/19-observability-and-support.md` reconciled if examples differ.
- Suggested GitHub issue title: `feat: add error handling correlation logging and health checks`

### Sprint 9: User and Role Management Foundation

- Goal: Implement administrative user, role, and permission management foundations.
- Why this comes now: Operational access must be manageable before business data expands.
- Deliverables: User list/detail/create/update/deactivate, role assignment, role/permission read views.
- Backend scope: Admin governance commands/queries with validation, authorization, and audit hooks.
- Frontend scope: Admin user management screens and forms.
- Database scope: User, role, and user-role updates with historical preservation.
- Testing scope: Admin success tests, non-admin denial tests, deactivated login checks, audit side-effect checks.
- DevOps / Observability scope: Safe logs for access changes without sensitive credentials.
- Out of scope: Password reset, MFA, SSO, and self-service account management.
- Exit criteria: Admin can govern internal users and roles while changes are authorized and auditable.
- Suggested GitHub issue title: `feat: implement user and role management foundation`

### Sprint 10: Organization Structure Management

- Goal: Implement operational hierarchy management.
- Why this comes now: Customers, tickets, scopes, assignments, and dashboards depend on region/country/account/campaign structure.
- Deliverables: Region, country, account, campaign, manager/supervisor/agent assignment APIs and UI.
- Backend scope: Admin write operations, scoped reads, deactivation rules, assignment validation.
- Frontend scope: Organization admin screens and scoped list views.
- Database scope: Organization hierarchy tables and assignment records.
- Testing scope: Admin write tests, scoped read tests, invalid assignment tests, deactivation history tests.
- DevOps / Observability scope: Audit and structured logs for structure/assignment changes.
- Out of scope: Workforce scheduling, HR, payroll, and capacity planning.
- Exit criteria: A fictional BPO hierarchy can be configured and scoped users can only see allowed structure.
- Suggested GitHub issue title: `feat: implement organization structure management`

### Sprint 11: Customer Management Foundation

- Goal: Implement customers as internal ticket context.
- Why this comes now: Ticket creation requires scoped customers linked to operational accounts.
- Deliverables: Customer create/list/detail/update, deactivation, and a customer ticket history route/read-model placeholder to be completed after ticket workflows exist.
- Backend scope: Scoped customer commands/queries with validation, authorization, and audit hooks.
- Frontend scope: Customer list, detail, create/edit screens.
- Database scope: Customer records linked to account and preserving historical context.
- Testing scope: Scoped customer access, create/update validation, Viewer read-only behavior, audit side effects.
- DevOps / Observability scope: Avoid logging sensitive customer details.
- Out of scope: Full CRM, external customer portal, customer login, marketing profile management.
- Exit criteria: Authorized internal users can manage customers only within scope.
- Suggested GitHub issue title: `feat: implement scoped customer management foundation`

### Sprint 12: Ticket Domain and Workflow Rules

- Goal: Implement ticket domain rules before ticket endpoints.
- Why this comes now: Ticket APIs must rely on Domain/Application rules, not controller logic.
- Deliverables: Ticket status model, priority model, transition rules, assignment eligibility rules, comment rules, escalation rules, resolution/closure rules, ticket number convention.
- Backend scope: Domain entities/value objects/enums and Application validators/services for workflow orchestration.
- Frontend scope: Shared TypeScript model alignment where useful.
- Database scope: Confirm existing ticket-related schema supports lifecycle, SLA state, and audit history.
- Testing scope: Unit tests for valid/invalid transitions, closure restrictions, escalation reason requirement, comment validation.
- DevOps / Observability scope: None beyond existing error/log conventions.
- Out of scope: Ticket endpoints and UI workflows.
- Exit criteria: Ticket workflow rules are proven by tests without ASP.NET Core or EF Core dependencies.
- Suggested GitHub issue title: `feat: implement ticket domain workflow rules`

### Sprint 13: Create Ticket Use Case

- Goal: Implement ticket creation with operational context, SLA initialization, and audit.
- Why this comes now: Required auth, scope, customer, hierarchy, minimal SLA, and audit foundations exist.
- Deliverables: Create ticket command/API, ticket list/detail read models needed for confirmation, Angular create form and initial queue.
- Backend scope: `CreateTicket` use case, required field validation, customer/account/campaign validation, scope checks, initial status, ticket number, SLA state creation, audit record.
- Frontend scope: Ticket create form, basic queue/detail confirmation, validation messages.
- Database scope: Ticket insert, initial status/history, initial SLA state, audit record.
- Testing scope: Successful create, missing fields, invalid customer/context, cross-scope denial, SLA/audit side effects.
- DevOps / Observability scope: Structured logs for ticket creation without sensitive descriptions in logs.
- Out of scope: Assignment, comments, escalation, resolution, closure, advanced SLA.
- Exit criteria: Authorized scoped users can create tickets and every ticket has initial SLA and audit context.
- Suggested GitHub issue title: `feat: implement create ticket use case`

### Sprint 14: Assign Ticket Use Case

- Goal: Implement supervisor assignment to eligible agents.
- Why this comes now: Assignment depends on ticket visibility, user scopes, organization assignments, and ticket workflow rules.
- Deliverables: Assign ticket API, eligible-agent lookup, assignment UI control.
- Backend scope: Assignment command, active eligible agent validation, supervisor scope validation, assignment history, audit record.
- Frontend scope: Eligible-agent dropdown and assignment action for authorized users.
- Database scope: Ticket assignment fields/history and audit record.
- Testing scope: Supervisor in-scope assignment, Agent assignment denial, cross-scope denial, inactive/ineligible agent rejection, closed-ticket rejection.
- DevOps / Observability scope: Safe denial reason logging.
- Out of scope: Auto-assignment, workforce scheduling, notification engine.
- Exit criteria: Supervisors can assign scoped tickets only to active eligible agents.
- Suggested GitHub issue title: `feat: implement assign ticket use case`

### Sprint 15: Ticket Status Updates and Workflow Enforcement

- Goal: Implement status and priority changes with enforced workflow rules.
- Why this comes now: Core execution depends on valid state transitions after creation and assignment exist.
- Deliverables: Status update API, priority update API if permitted, status history, frontend controls.
- Backend scope: Transition validation, permission checks, scope checks, resource-state checks, history and audit records.
- Frontend scope: Status and priority controls that reflect role and state.
- Database scope: Ticket status/priority updates, status history, audit record.
- Testing scope: Valid transitions, invalid transitions, Viewer denial, cross-scope denial, closed-ticket restrictions.
- DevOps / Observability scope: Business rule failures return canonical safe errors with correlation IDs.
- Out of scope: Reopen workflow unless explicitly approved for MVP.
- Exit criteria: Ticket status and priority updates are controlled, scoped, audited, and historically traceable.
- Suggested GitHub issue title: `feat: implement ticket status workflow enforcement`

### Sprint 16: Internal Comments

- Goal: Add internal collaboration on tickets.
- Why this comes now: Comments require scoped ticket access and established audit/error patterns.
- Deliverables: Add/list internal comments APIs, comments timeline UI.
- Backend scope: Comment command/query, non-empty content validation, internal-only visibility, audit record.
- Frontend scope: Comment timeline and add-comment form for authorized users.
- Database scope: Comment records with author, timestamp, ticket link, and audit record.
- Testing scope: Empty comment rejected, Viewer write denial, cross-scope denial, unauthorized comments hidden.
- DevOps / Observability scope: Avoid logging full comment bodies.
- Out of scope: External/customer-visible comments, chat, attachments.
- Exit criteria: Authorized internal users can add and view comments for scoped tickets.
- Suggested GitHub issue title: `feat: implement internal ticket comments`

### Sprint 17: Escalation Workflow

- Goal: Implement ticket escalation with reason, visibility, history, and audit.
- Why this comes now: Escalation is a core operational workflow once status and comments exist.
- Deliverables: Escalate ticket API, escalation queue/read model, escalation UI.
- Backend scope: Escalation command, required reason, allowed status/state checks, supervisor/manager visibility, audit record.
- Frontend scope: Escalation action, reason form, escalation list for authorized roles.
- Database scope: Escalation record, ticket escalation flags/state, audit record.
- Testing scope: Missing reason rejected, cross-scope denial, closed-ticket rejection, scoped escalation query tests.
- DevOps / Observability scope: Structured escalation event logs.
- Out of scope: Complex notification engine, automatic escalation, predictive escalation.
- Exit criteria: Escalations are visible to authorized scoped users and preserve reason/history.
- Suggested GitHub issue title: `feat: implement ticket escalation workflow`

### Sprint 18: Resolve and Close Ticket Workflow

- Goal: Complete the MVP ticket lifecycle.
- Why this comes now: Resolution and closure depend on ticket creation, assignment, status rules, comments, escalation, SLA, and audit.
- Deliverables: Resolve API, close API, ticket history API, scoped customer ticket history completion, resolution/closure UI.
- Backend scope: Resolution command, closure command, closure preconditions, final SLA outcome preservation, audit/history records, customer ticket history read model.
- Frontend scope: Resolution form, close action, lifecycle history timeline.
- Database scope: Resolution fields/records, resolved/closed timestamps, final SLA state, audit records.
- Testing scope: Cannot close unresolved ticket, closed tickets locked from normal changes, final SLA preserved, cross-scope denial.
- DevOps / Observability scope: Canonical business errors and correlation IDs.
- Out of scope: Advanced reopen workflow and customer-facing closure confirmation.
- Exit criteria: Ticket lifecycle works from create through close with scope, audit, SLA, and history intact.
- Suggested GitHub issue title: `feat: implement resolve and close ticket workflow`

### Sprint 19: SLA Tracking Foundation

- Goal: Complete MVP SLA tracking, filtering, and operational visibility.
- Why this comes now: Basic SLA state exists for tickets; this sprint makes it reliable and visible across workflows.
- Deliverables: SLA policy management, SLA status evaluation, ticket SLA filters, SLA summary read models.
- Backend scope: Deterministic request-time evaluation for Within SLA, At Risk, Breached, and Completed; policy lookup by documented dimensions.
- Frontend scope: SLA badges, filters, and policy/admin screens if included in MVP permissions.
- Database scope: SLA policy/state indexes and final outcome preservation.
- Testing scope: Due date calculation, at-risk threshold, breach evaluation, completed outcome, filter correctness.
- DevOps / Observability scope: Safe logs for SLA evaluation failures and slow SLA queries.
- Out of scope: Background worker, business-hour calendars, pause/resume, predictive SLA.
- Exit criteria: SLA state is initialized, evaluated, displayed, filterable, and preserved at completion.
- Suggested GitHub issue title: `feat: implement MVP SLA tracking foundation`

### Sprint 20: Audit Log Foundation and Audit Views

- Goal: Complete reusable audit querying and operational audit views.
- Why this comes now: Audit records already exist from prior workflows; users now need controlled visibility into them.
- Deliverables: Audit query APIs, entity audit history views, Angular audit views.
- Backend scope: Append-only audit persistence conventions, scoped audit queries, filters by actor/action/entity/time.
- Frontend scope: Audit log list/detail and entity history panels for authorized roles.
- Database scope: Audit log indexes and immutability safeguards.
- Testing scope: Audit creation coverage for critical workflows, scoped audit visibility, tamper-resistant behavior where practical.
- DevOps / Observability scope: Keep audit logs distinct from technical logs.
- Out of scope: Advanced audit analytics, bulk audit export unless separately scoped.
- Exit criteria: Authorized users can inspect audit history without cross-scope data leakage.
- Suggested GitHub issue title: `feat: implement audit log queries and views`

### Sprint 21: Basic Operational Dashboards

- Goal: Implement scoped MVP dashboards for operational visibility.
- Why this comes now: Dashboards require stable ticket, assignment, SLA, audit, and scope data.
- Deliverables: Operational dashboard API/read models and Angular dashboard views.
- Backend scope: Scoped aggregates for open tickets, status, priority, SLA state, escalations, assigned workload, account/campaign filters.
- Frontend scope: Role-aware dashboard widgets, filters, and drill-in links.
- Database scope: Validate indexes for dashboard filters and aggregates.
- Testing scope: Aggregate correctness, cross-scope isolation, role-specific dashboard access.
- DevOps / Observability scope: Log slow dashboard queries and include request duration/correlation ID.
- Out of scope: Advanced Power BI dashboards, predictive analytics, custom report builder.
- Exit criteria: Managers, Supervisors, Agents, and Viewers see only scoped operational metrics appropriate to their roles.
- Suggested GitHub issue title: `feat: implement basic scoped operational dashboards`

### Sprint 22: Frontend Integration for MVP Flows

- Goal: Integrate the Angular UI across the complete MVP workflow.
- Why this comes now: Backend use cases are available and need a coherent user experience.
- Deliverables: End-to-end UI flow for login, user admin, organization, customers, ticket queue/detail/create/assign/status/comment/escalate/resolve/close, SLA, audit, dashboards.
- Backend scope: Fix integration gaps discovered by UI flows.
- Frontend scope: Route guards, interceptors, loading states, empty states, validation messages, role-aware navigation, scoped filters.
- Database scope: No schema changes unless required to close integration defects.
- Testing scope: Component/service tests for critical UI flows and manual smoke script.
- DevOps / Observability scope: Frontend environment validation and API error display consistency.
- Out of scope: New MVP business capabilities beyond documented workflows.
- Exit criteria: A seeded user can complete the full MVP workflow through the UI using fictional data.
- Suggested GitHub issue title: `feat: integrate Angular MVP workflows`

### Sprint 23: Integration Tests, API Tests, and E2E Smoke Tests

- Goal: Validate the MVP across layers.
- Why this comes now: The main workflows exist and need regression protection before release hardening.
- Deliverables: Integration test suite, API authorization/scope tests, E2E smoke tests.
- Backend scope: Test fixtures, seeded personas, API test coverage for critical workflows.
- Frontend scope: E2E smoke paths for core flows.
- Database scope: Test database lifecycle using SQL Server or Testcontainers where practical.
- Testing scope: Login, user admin, scope filtering, customer management, create/assign/status/comment/escalate/resolve/close, SLA, audit, dashboards.
- DevOps / Observability scope: Tests suitable for CI or clearly separated when too heavy for every push.
- Out of scope: Exhaustive browser matrix and production performance testing.
- Exit criteria: Critical MVP flows have automated regression coverage including negative security and workflow tests.
- Suggested GitHub issue title: `test: add MVP integration API and E2E smoke coverage`

### Sprint 24: CI Pipeline Hardening

- Goal: Make CI a reliable release gate for MVP.
- Why this comes now: Automated tests now exist and should protect stabilization.
- Deliverables: Hardened GitHub Actions workflow, backend build/test, frontend build/test, optional integration/E2E job separation, artifacts/logs.
- Backend scope: Ensure test commands are stable and deterministic.
- Frontend scope: Ensure build/test commands run in CI.
- Database scope: CI SQL Server/Testcontainers strategy if integration tests run in pipeline.
- Testing scope: Split fast and slower tests appropriately.
- DevOps / Observability scope: No secrets in logs, CI status required before merge, documented failure triage.
- Out of scope: Production deployment automation unless already documented as MVP.
- Exit criteria: CI provides a trustworthy signal for backend, frontend, and critical tests.
- Suggested GitHub issue title: `ci: harden MVP build test and validation pipeline`

### Sprint 25: MVP Stabilization and Release Checklist

- Goal: Stabilize MVP for demo, handoff, and future Phase 2 planning.
- Why this comes now: All MVP capabilities are present and need release-level review.
- Deliverables: Release checklist, demo script, UAT checklist, known limitations, final docs update, bug fixes from stabilization.
- Backend scope: Close defects, tighten authorization/scope gaps, review API consistency.
- Frontend scope: Polish critical screens, loading/error states, Viewer read-only UX, responsive MVP flows.
- Database scope: Final migration check, seed data review, no real data.
- Testing scope: Full MVP validation run and documented residual test gaps.
- DevOps / Observability scope: Verify `/health`, `/health/details` production behavior, logs, correlation IDs, CI green, no secrets committed.
- Out of scope: Phase 2/3 feature expansion.
- Exit criteria: MVP can be demonstrated end to end with fictional data, passing validation, documented limitations, and no known critical security gaps.
- Suggested GitHub issue title: `chore: complete MVP stabilization and release checklist`

## MVP Completion Criteria

The MVP is complete when:

- Clean Architecture boundaries are preserved.
- SQL Server, EF Core, Docker Compose, seed data, migrations, and CI are in place.
- JWT authentication works for active internal users.
- RBAC, permissions, and scope filtering are enforced by the backend.
- Frontend role visibility is present but not relied on as security.
- Admin can manage users, roles, scopes, and operational hierarchy for governance/configuration.
- Operational users can access only scoped regions, countries, accounts, campaigns, customers, tickets, dashboards, audit logs, and eligible-agent lookups.
- Customers exist only as internal ticket context.
- Tickets support creation, assignment, status updates, internal comments, escalation, resolution, closure, and history.
- SLA tracking is simple, deterministic, displayed, filterable, and preserved at completion.
- Critical changes produce audit records with actor, action, entity, timestamp, and relevant before/after context.
- Basic dashboards show scoped operational and SLA metrics.
- Tests cover success paths, negative authorization, cross-scope denial, invalid workflows, SLA behavior, audit behavior, and E2E smoke flows.
- Health checks, structured logs, correlation IDs, and safe error handling are implemented.
- All demo data is fictional.
- Non-MVP items remain parked.

## Phase 2 Parking Lot

- Enhanced supervisor dashboards and escalation queues.
- In-app notification center.
- Ticket attachments with access control.
- Advanced audit filters and optional audit export.
- Improved permissions matrix UI.
- More reporting-ready views and CSV exports.
- Background SLA worker if request-time evaluation is no longer sufficient.
- Saved filters and dashboard preferences.
- Expanded integration and E2E coverage.
- Application Insights integration beyond readiness checks.
- More complete Dockerized full-stack local run.

## Phase 3 Parking Lot

- Azure App Service, Azure Static Web Apps, Azure SQL, Azure Key Vault, and Application Insights deployment.
- Automated staging and production deployment workflows.
- Production-grade configuration and secret management.
- Advanced SLA calendars, business hours, pause/resume, and policy variants.
- Selected external integrations such as email, storage, webhooks, and Power BI dataset/export integration.
- Optional AI-assisted ticket classification after MVP and Phase 2 are stable.
- Predictive SLA risk after MVP and Phase 2 are stable.
- Enterprise SSO or Microsoft Entra ID integration.
- Performance testing and production hardening.
- Advanced alerting and support operations.

## Risks and Controls

| Risk | Control |
| --- | --- |
| Scope creep into AI, telephony, BI, HR, customer portal, SSO, or automation | Keep MVP checklist on every issue and move advanced work to Phase 2 or Phase 3 |
| Generic ticket CRUD implementation | Require hierarchy, RBAC, scope, SLA, audit, dashboards, and operational context in acceptance criteria |
| Ticket work starts before foundations | Do not begin ticket APIs until auth, authorization, scope, minimal audit, minimal SLA, customer, and hierarchy foundations exist |
| Business rules leak into controllers or Angular | Keep rules in Domain/Application and test them outside transport/UI layers |
| EF Core leaks into Domain/Application | Keep DbContext, configurations, migrations, and persistence services in Infrastructure |
| Frontend authorization drift | Treat UI hiding as UX only and test backend authorization directly |
| Scope filtering gaps | Add cross-scope API/integration tests for every scoped feature |
| Audit added too late | Require audit hooks and audit side effects in admin and ticket workflow exit criteria |
| SLA becomes too complex | Use deterministic MVP evaluation and defer workers, calendars, pause/resume, and prediction |
| Admin becomes a default ticket operator | Keep Admin focused on governance/configuration and ticket operations with operational roles |
| Dashboard data leaks | Reuse backend scope filters and test aggregate isolation |
| Secrets exposure | Use `.env.example`, local secrets, GitHub Secrets, and no committed real credentials |
| Insufficient supportability | Add safe errors, correlation IDs, structured logs, `/health`, and `/health/details` before business APIs expand |

## Validation Checklist

Before closing each implementation issue:

- [ ] Scope matches MVP or is explicitly parked for Phase 2/3.
- [ ] No AI classification, customer portal, enterprise SSO, advanced Power BI, advanced SLA calendars, or predictive SLA is introduced into MVP.
- [ ] Controllers remain thin.
- [ ] Application handlers coordinate validation, authorization, transactions, and audit.
- [ ] Domain rules do not depend on infrastructure, ASP.NET Core, EF Core, SQL Server, Angular, JWT, or logging.
- [ ] EF Core remains in Infrastructure.
- [ ] Backend authorization is enforced independently of frontend visibility.
- [ ] Scope filters apply to all scoped reads and writes.
- [ ] Critical changes create audit records.
- [ ] SLA behavior remains simple and deterministic for MVP.
- [ ] Admin behavior remains governance/configuration oriented.
- [ ] Negative authorization and cross-scope tests are included where applicable.
- [ ] Invalid workflow and validation tests are included where applicable.
- [ ] No real data, secrets, tokens, passwords, or connection strings are committed.
- [ ] Health, logging, correlation ID, and error behavior remain safe and supportable.
- [ ] Documentation is updated when behavior or architecture decisions change.
