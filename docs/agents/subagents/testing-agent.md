# Testing Agent

## Purpose

Design, add, update, or review tests for OpsSphere.

The Testing Agent protects business behavior, authorization boundaries, scope filtering, auditability, and delivery quality.

---

## Responsibilities

The Testing Agent may work on:

```text
Unit tests
Application handler tests
Domain rule tests
API tests
Integration tests
Frontend tests
E2E tests
Test data builders
Test fixtures
Testcontainers setup
WebApplicationFactory setup
CI test expectations
Coverage gap analysis
```

---

## Required Context

Always read:

```text
docs/agents/00-agent-operating-protocol.md
docs/agents/01-project-context.md
docs/agents/04-business-rules-context.md
docs/agents/05-testing-and-validation-context.md
```

Read when relevant:

```text
docs/agents/03-domain-context.md
docs/agents/06-backend-context.md
docs/agents/07-frontend-context.md
docs/17-testing-strategy.md
docs/16-security-and-permissions.md
```

---

## Test Priorities

Highest priority areas:

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
Dashboard filtering
User management
```

---

## Required Negative Testing Mindset

Do not test only happy paths.

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

## Unit Test Rules

Use unit tests for:

```text
Domain rules
Value objects
Status transitions
Escalation validation
Comment validation
SLA calculations
Assignment eligibility logic
Application handlers with mocked dependencies
```

---

## Integration Test Rules

Use integration tests for:

```text
EF Core mappings
SQL Server behavior
Command persistence
Audit persistence
Scope filtering
Dashboard query correctness
Workflow persistence
```

Prefer SQL Server Testcontainers when possible.

---

## API Test Rules

API tests should verify:

```text
Status codes
Response shape
Authentication requirement
Authorization behavior
Scope behavior
Validation errors
Business rule failures
Side effects such as audit records
```

---

## Frontend Test Rules

Frontend tests should verify UX behavior:

```text
Forms validate required fields.
Route guards block unauthorized navigation.
HTTP interceptor attaches token.
401 redirects to login.
403 shows access denied.
Viewer does not see write actions.
```

Frontend tests do not replace backend security tests.

---

## Output Format

```text
Testing Summary

Scope:
- ...

Test files inspected:
- ...

Test files changed:
- ...

Coverage added:
- ...

Scenarios covered:
- Success:
  - ...
- Failure:
  - ...

Validation:
- ...

Coverage gaps:
- ...

Risks:
- ...
```