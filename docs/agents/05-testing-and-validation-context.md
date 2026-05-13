# OpsSphere Testing and Validation Context

## Purpose

This document provides compressed testing and validation context for agents working inside OpsSphere.

Use this file when the task touches:

- Domain behavior
- Application handlers
- API endpoints
- Authorization
- Scope filtering
- Persistence
- Audit logging
- Frontend forms
- End-to-end workflows
- CI/CD validation
- Definition of done

---

## Testing Philosophy

OpsSphere must be validated as an enterprise application, not only as a CRUD project.

Testing must cover:

```text
Business rules
Backend workflows
API behavior
Frontend behavior
End-to-end user flows
Authorization
Scope filtering
Auditability
Persistence
Delivery quality
CI/CD validation
```

---

## Testing Principles

Agents must follow these testing principles:

```text
Test business rules close to the domain layer.
Test application workflows through commands, queries, and handlers.
Test persistence and API behavior with realistic integration tests.
Test critical user journeys through end-to-end tests.
Keep tests deterministic and repeatable.
Use fictional sample data only.
Do not depend on production data.
Automate tests as much as possible.
Keep tests aligned with use cases and business rules.
Treat authorization and auditability as first-class testing concerns.
```

---

## Practical Test Pyramid

```text
Manual / UAT Tests
  - Business acceptance scenarios
  - Stakeholder review
  - Final workflow validation

E2E Tests
  - Browser-based critical journeys
  - Login
  - Ticket creation
  - Assignment
  - Escalation
  - Dashboard review

Frontend Tests
  - Components
  - Forms
  - Guards
  - Services
  - UI behavior

API / Integration Tests
  - HTTP endpoints
  - Authentication
  - Authorization
  - Persistence
  - Audit logging
  - Database behavior

Unit Tests
  - Domain rules
  - Application handlers
  - Validators
  - Workflow rules
  - Authorization helpers
```

Lower levels should contain more tests.

Upper levels should focus on fewer, high-value business-critical scenarios.

---

## Unit Test Scope

Use unit tests for:

```text
Domain entities
Domain services
Value objects
Application command handlers where dependencies can be mocked
Validators
Business rule enforcement
Status transition rules
SLA calculation rules
Assignment eligibility rules
Escalation rules
Authorization helper logic
Audit event creation logic when isolated
```

Examples:

```text
CloseTicket_WhenTicketIsNotResolved_ShouldRejectClosure
EscalateTicket_WhenReasonIsMissing_ShouldRejectEscalation
AssignTicket_WhenAgentIsOutsideScope_ShouldRejectAssignment
AddComment_WhenCommentIsEmpty_ShouldRejectComment
```

---

## Integration Test Scope

Use integration tests for:

```text
Command handler persistence
Query handler filtering
EF Core mappings
Database constraints
Transaction behavior
Audit log creation
Ticket lifecycle persistence
SLA state persistence
Role and scope filtering
User deactivation behavior
Assignment and reassignment history
Escalation history
Dashboard query correctness
```

Recommended approach:

```text
Integration tests should use SQL Server Testcontainers when possible.
Avoid relying only on in-memory providers for persistence behavior.
```

---

## API Test Scope

API tests should cover:

```text
Authentication endpoints
Protected endpoint access
Role-based authorization
Scope-based authorization
Request validation
Response status codes
Response DTO shape
Error handling
Ticket lifecycle endpoints
User management endpoints
Dashboard endpoints
Audit history endpoints
```

Expected response behavior:

| Scenario | Expected Response |
|---|---|
| Successful creation | `201 Created` |
| Successful read | `200 OK` |
| Successful update | `200 OK` or `204 No Content` |
| Validation failure | `400 Bad Request` or `422 Unprocessable Entity` |
| Missing authentication | `401 Unauthorized` |
| Insufficient permissions | `403 Forbidden` |
| Resource not found | `404 Not Found` |
| Business rule violation | `400 Bad Request` |
| Conflict | `409 Conflict` |
| Unexpected error | `500 Internal Server Error` with safe response |

---

## Frontend Test Scope

Frontend tests should cover:

```text
Components
Forms
Validators
Route guards
Services
HTTP interceptors
Role-aware navigation
Scope-aware UI behavior
Error message handling
Login flow behavior
Ticket creation form behavior
Ticket status actions
Dashboard filtering
```

Important rule:

```text
Frontend tests can verify UX behavior, but backend tests must verify real security.
```

---

## E2E Test Scope

E2E tests should focus only on critical journeys:

```text
Login
Create ticket
Assign ticket
Update ticket status
Escalate ticket
Resolve ticket
Close ticket
View SLA dashboard
Admin manages user
Viewer cannot modify records
```

Do not overuse E2E tests.

Most business rules should be covered by unit, integration, and API tests.

---

## Highest Priority Coverage Areas

Agents should prioritize tests for:

```text
Authentication
Authorization
Scope filtering
Ticket creation
Ticket assignment
Ticket status workflow
Escalation
Resolution
Closure
Audit logging
SLA state
User management
Dashboard filtering
```

---

## Negative Test Priority

Do not only test happy paths.

Important negative tests:

```text
Invalid credentials are rejected.
Deactivated users cannot log in.
Unauthenticated requests return 401.
Unauthorized role returns 403.
Cross-scope access is denied.
Viewer cannot modify records.
Ticket cannot be closed before resolution.
Closed ticket cannot be assigned.
Escalation without reason is rejected.
Empty internal comment is rejected.
Ticket creation missing required fields is rejected.
Agent outside campaign cannot receive assignment.
```

---

## Test Data Strategy

Use fictional sample data only.

Example local seed data:

```text
Region: Latin America
Countries: Mexico, Colombia, Costa Rica
Accounts: NovaBank, Streamly, Shopora
Campaigns: Credit Card Support, Fraud Review Support, Creator Support
Roles: Admin, OperationsManager, Supervisor, Agent, Viewer
```

Never use real company, client, employee, or customer data.

---

## CI Testing Expectations

CI should validate:

```text
Restore dependencies
Build backend
Run backend unit tests
Run backend integration tests
Build frontend
Run frontend tests
Run optional API collection tests
Run optional E2E smoke tests
Publish test results
```

---

## Recommended Quality Gate

```text
Pull Request
  → Build backend
  → Run backend tests
  → Build frontend
  → Run frontend tests
  → Run integration/API tests when available
  → Review changed files
  → Confirm issue scope
```

A pull request is not ready if:

```text
Backend does not build.
Frontend does not build.
Unit tests fail.
Integration tests fail.
Critical API tests fail.
Formatting or linting checks fail if configured.
Test failures are ignored without explanation.
The change does not match the issue scope.
```

---

## Validation Commands

Actual commands may change during implementation.

Expected command families:

```text
dotnet restore
dotnet build
dotnet test
```

```text
npm install
npm run build
npm run test
```

```text
docker compose up -d
```

```text
docker compose down
```

---

## Definition of Done

A feature or change is done only when:

```text
Code builds.
Relevant tests pass.
Business rules remain enforced.
Authorization and scope rules are considered.
Audit behavior is considered.
Acceptance criteria are satisfied.
No unrelated scope was introduced.
Documentation is updated when behavior changes.
Validation was run or explicitly explained.
```