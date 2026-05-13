# Testing Strategy

## Document Information

| Field | Value |
|---|---|
| Project | OpsSphere |
| Document | Testing Strategy |
| File | `docs/17-testing-strategy.md` |
| Version | 1.0 |
| Status | Draft |
| Project Type | Enterprise Support Operations Platform |
| Related Issue | #6 |
| Phase | Delivery Planning |

---

## 1. Purpose

This document defines the testing strategy for OpsSphere.

The purpose of the testing strategy is to describe how the platform will be validated before and during implementation.

OpsSphere is planned as a maintainable, testable, deployable, and supportable enterprise application. For that reason, testing must cover business rules, backend workflows, API behavior, frontend behavior, end-to-end user flows, and user acceptance scenarios.

This document connects:

- Functional requirements.
- Business rules.
- Use cases.
- Domain behavior.
- API behavior.
- Frontend workflows.
- Delivery quality.
- CI/CD validation.

---

## 2. Scope

This testing strategy covers the initial delivery planning phase for OpsSphere.

The strategy includes:

- Unit tests.
- Integration tests.
- API tests.
- Frontend tests.
- End-to-end tests.
- User acceptance testing scenarios.
- Test data strategy.
- Test environment expectations.
- CI validation expectations.
- Coverage priorities.

This document does not define final test implementation code. It defines the quality plan that will guide implementation.

---

## 3. Testing Goals

The main goals of the OpsSphere testing strategy are:

1. Validate core business rules before features are considered complete.
2. Protect the ticket lifecycle from invalid workflow behavior.
3. Verify role-based and scope-based access control.
4. Confirm that API endpoints behave consistently.
5. Validate frontend workflows from the user's perspective.
6. Ensure integration between application logic, persistence, authentication, authorization, and audit history.
7. Provide confidence that the system can be deployed through a repeatable CI/CD pipeline.
8. Support maintainability by catching regressions early.

---

## 4. Testing Principles

OpsSphere will follow these testing principles:

- Test business rules close to the domain layer.
- Test application workflows through commands, queries, and handlers.
- Test persistence and API behavior with realistic integration tests.
- Test critical user journeys through end-to-end tests.
- Keep tests deterministic and repeatable.
- Use fictional sample data only.
- Do not depend on production data.
- Automate tests as much as possible.
- Keep tests aligned with use cases and business rules.
- Treat authorization and auditability as first-class testing concerns.

---

## 5. Test Pyramid

OpsSphere will use a practical test pyramid.

```text
Manual / UAT Tests
  - Business acceptance scenarios
  - Stakeholder review
  - Final workflow validation

E2E Tests
  - Browser-based critical journeys
  - Login, ticket creation, assignment, escalation, dashboard review

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

The lower levels should contain more tests because they are faster, cheaper, and easier to run frequently.

The upper levels should focus on fewer, high-value business-critical scenarios.

---

# 6. Unit Testing Strategy

## 6.1 Purpose

Unit tests validate isolated pieces of business logic without depending on the database, API server, browser, or external services.

Unit tests should provide fast feedback during development.

## 6.2 Backend Unit Test Scope

Backend unit tests should cover:

- Domain entities.
- Domain services.
- Value objects.
- Application command handlers where dependencies can be mocked.
- Validators.
- Business rule enforcement.
- Status transition rules.
- SLA calculation rules.
- Assignment eligibility rules.
- Escalation rules.
- Authorization helper logic.
- Audit event creation logic when isolated.

## 6.3 Recommended Backend Unit Test Tools

| Area | Tool |
|---|---|
| Test Framework | xUnit |
| Assertions | FluentAssertions |
| Mocking | Moq or NSubstitute |
| Test Data | Builder pattern or test data factories |

## 6.4 Unit Test Examples

### Ticket Workflow Rules

```text
Given a ticket with status Open
When an eligible agent is assigned
Then the ticket status should become Assigned
```

```text
Given a ticket with status In Progress
When the user resolves the ticket
Then the ticket status should become Resolved
```

```text
Given a ticket with status Open
When the user attempts to close the ticket
Then the system should reject the transition
```

### Escalation Rules

```text
Given an active ticket
When the user escalates it with a reason
Then the ticket should become escalated
And the escalation reason should be stored
```

```text
Given a closed ticket
When the user attempts to escalate it
Then the system should reject the escalation
```

### Comment Rules

```text
Given an active ticket
When the user adds an empty internal comment
Then the system should reject the comment
```

### Assignment Rules

```text
Given a ticket in a campaign
When a supervisor assigns it to an agent outside the campaign scope
Then the assignment should be rejected
```

### SLA Rules

```text
Given a high-priority ticket
When the SLA target is initialized
Then the SLA target should be shorter than a normal-priority ticket
```

## 6.5 Unit Test Naming Convention

Recommended naming format:

```text
MethodName_StateUnderTest_ExpectedBehavior
```

Example:

```text
CloseTicket_WhenTicketIsNotResolved_ShouldRejectClosure
EscalateTicket_WhenReasonIsMissing_ShouldRejectEscalation
AssignTicket_WhenAgentIsOutsideScope_ShouldRejectAssignment
```

## 6.6 Unit Test Completion Criteria

A unit-tested feature should satisfy:

- Domain rules are covered.
- Invalid cases are tested.
- Success cases are tested.
- Edge cases are tested when relevant.
- Tests can run without database access.
- Tests are deterministic.

---

# 7. Integration Testing Strategy

## 7.1 Purpose

Integration tests validate that multiple parts of the backend work together correctly.

These tests should verify behavior across:

- Application layer.
- Domain layer.
- Infrastructure layer.
- Entity Framework Core.
- SQL Server.
- Authentication and authorization when needed.
- Audit persistence.

## 7.2 Integration Test Scope

Integration tests should cover:

- Command handler persistence.
- Query handler filtering.
- EF Core mappings.
- Database constraints.
- Transaction behavior.
- Audit log creation.
- Ticket lifecycle persistence.
- SLA state persistence.
- Role and scope filtering.
- User deactivation behavior.
- Assignment and reassignment history.
- Escalation history.
- Dashboard query correctness.

## 7.3 Recommended Integration Test Tools

| Area | Tool |
|---|---|
| Test Framework | xUnit |
| API Test Host | WebApplicationFactory |
| Database | SQL Server Testcontainers |
| Persistence | Entity Framework Core |
| Assertions | FluentAssertions |

## 7.4 Database Integration Strategy

Integration tests should run against a real SQL Server test database using Testcontainers when possible.

This avoids false confidence from testing only against in-memory providers.

```text
Test Runner
  → Starts SQL Server Testcontainer
  → Applies EF Core migrations
  → Seeds fictional test data
  → Runs integration tests
  → Disposes database container
```

## 7.5 Integration Test Examples

### Create Ticket Integration Test

```text
Given an authenticated Agent with valid campaign scope
And a valid customer exists
When the Agent creates a ticket
Then the ticket is persisted
And the ticket is linked to the customer
And the ticket includes account and campaign context
And the initial SLA state is created
And an audit record is created
```

### Assign Ticket Integration Test

```text
Given a Supervisor with account scope
And an active Agent in the same account
And an open ticket in that account
When the Supervisor assigns the ticket
Then the ticket assigned agent is updated
And the status becomes Assigned
And assignment history is persisted
And an audit record is created
```

### Escalate Ticket Integration Test

```text
Given an Agent with ticket access
And an active ticket
When the Agent escalates the ticket with a valid reason
Then the ticket status becomes Escalated
And the escalation record is persisted
And the ticket appears in the supervisor escalation query
And an audit record is created
```

### Scope Filtering Integration Test

```text
Given two accounts
And a Supervisor assigned only to Account A
When the Supervisor requests the ticket list
Then only tickets from Account A are returned
And tickets from Account B are not returned
```

## 7.6 Integration Test Completion Criteria

A workflow should have integration coverage when:

- It changes persistent business state.
- It depends on authorization or scope.
- It creates audit history.
- It affects dashboard or reporting data.
- It has important database relationships.

---

# 8. API Testing Strategy

## 8.1 Purpose

API tests validate the HTTP behavior of OpsSphere.

These tests confirm that the backend exposes predictable, secure, and usable API endpoints.

## 8.2 API Test Scope

API tests should cover:

- Authentication endpoints.
- Protected endpoint access.
- Role-based authorization.
- Scope-based authorization.
- Request validation.
- Response status codes.
- Response DTO shape.
- Error handling.
- Ticket lifecycle endpoints.
- User management endpoints.
- Dashboard endpoints.
- Audit history endpoints.

## 8.3 Recommended API Test Tools

| Area | Tool |
|---|---|
| Automated API Tests | WebApplicationFactory + xUnit |
| Manual API Exploration | Swagger / OpenAPI |
| API Collections | Postman |
| CI API Collection Runner | Newman, optional |

## 8.4 API Response Expectations

API endpoints should return consistent HTTP responses.

| Scenario | Expected Response |
|---|---|
| Successful creation | `201 Created` |
| Successful read | `200 OK` |
| Successful update | `200 OK` or `204 No Content` |
| Validation failure | `400 Bad Request` |
| Missing authentication | `401 Unauthorized` |
| Insufficient permissions | `403 Forbidden` |
| Resource not found | `404 Not Found` |
| Business rule violation | `400 Bad Request` or domain-specific problem response |
| Unexpected error | `500 Internal Server Error` with safe error response |

## 8.5 API Test Examples

### Authentication Required

```text
Given an unauthenticated request
When the request is sent to a protected ticket endpoint
Then the API should return 401 Unauthorized
```

### Role Restriction

```text
Given a Viewer user
When the user attempts to create a ticket
Then the API should return 403 Forbidden
```

### Scope Restriction

```text
Given an Agent assigned to Campaign A
When the Agent attempts to create a ticket for Campaign B
Then the API should reject the request
```

### Validation Error

```text
Given a create ticket request without required fields
When the request is submitted
Then the API should return validation errors
And no ticket should be created
```

### Successful Ticket Creation

```text
Given a valid authenticated Agent
When the Agent submits a valid create ticket request
Then the API should return 201 Created
And the response should include the created ticket identifier
```

## 8.6 API Test Completion Criteria

An API endpoint should be considered test-covered when:

- Success path is tested.
- Validation failure is tested.
- Unauthorized access is tested.
- Forbidden access is tested when applicable.
- Response shape is verified.
- Important side effects are verified.

---

# 9. Frontend Testing Strategy

## 9.1 Purpose

Frontend tests validate Angular behavior from the client-side perspective.

Frontend testing should confirm that the user interface behaves correctly, forms validate user input, routes are protected, and users only see actions they are allowed to perform.

## 9.2 Frontend Test Scope

Frontend tests should cover:

- Components.
- Forms.
- Validators.
- Route guards.
- HTTP interceptors.
- Authentication state.
- Role-aware navigation.
- Scope-aware UI behavior.
- Ticket creation form.
- Ticket detail view.
- Ticket status update behavior.
- Internal comment form.
- Escalation form.
- Dashboard filters.
- Error message rendering.

## 9.3 Recommended Frontend Test Tools

| Area | Tool |
|---|---|
| Angular Unit Tests | Jasmine/Karma or Jest |
| Component Tests | Angular Testing Library, optional |
| E2E Tests | Playwright |
| Mock API | MSW or test doubles, optional |

The final tool selection can be confirmed during frontend implementation.

## 9.4 Frontend Test Examples

### Login Form

```text
Given the login form is displayed
When the user submits empty credentials
Then validation messages should be displayed
And no login request should be sent
```

### Ticket Creation Form

```text
Given an Agent is creating a ticket
When required fields are missing
Then the submit action should be disabled or validation messages should appear
```

### Role-Based Navigation

```text
Given a Viewer user is logged in
When the sidebar is displayed
Then create, edit, assign, escalate, resolve, and close actions should not be visible
```

### Route Guard

```text
Given an unauthenticated user
When the user attempts to access the dashboard route
Then the user should be redirected to the login page
```

### API Error Display

```text
Given the API returns a validation error
When the user submits a form
Then the frontend should display a clear validation message
```

## 9.5 Frontend Test Completion Criteria

A frontend workflow should be considered test-covered when:

- Required field validation is tested.
- Important user actions are tested.
- Loading and error states are tested.
- Role-based UI behavior is tested.
- Route access behavior is tested when applicable.

---

# 10. End-to-End Testing Strategy

## 10.1 Purpose

End-to-end tests validate complete user journeys through the browser.

These tests should cover the most important business workflows from login to final result.

E2E tests should be fewer than unit or integration tests, but they should cover critical operational paths.

## 10.2 Recommended E2E Tool

```text
Playwright
```

Playwright is recommended because it can validate real browser behavior and can be integrated into CI pipelines.

## 10.3 E2E Test Scope

Initial E2E tests should cover:

- User login.
- Ticket creation.
- Ticket assignment.
- Ticket status update.
- Internal comment creation.
- Ticket escalation.
- Ticket resolution.
- Ticket closure.
- SLA dashboard visibility.
- Viewer read-only restrictions.
- Admin user management flow.

## 10.4 E2E Test Data

E2E tests should use seeded fictional data.

Example test users:

```text
admin@opssphere.test
manager@opssphere.test
supervisor@opssphere.test
agent@opssphere.test
viewer@opssphere.test
```

Example operational structure:

```text
Region: Latin America
Country: Mexico
Account: NovaBank
Campaign: Credit Card Support
Supervisor: Laura Torres
Agent: Ana López
Customer: Mateo Rivera
```

## 10.5 E2E Test Examples

### E2E: Create Ticket

```text
Given an Agent logs into OpsSphere
When the Agent creates a valid ticket for a customer
Then the ticket appears in the ticket queue
And the ticket detail page shows status Open
And the ticket shows SLA information
```

### E2E: Supervisor Assigns Ticket

```text
Given a Supervisor logs into OpsSphere
And an open ticket exists within the supervisor's account
When the Supervisor assigns the ticket to an eligible Agent
Then the ticket shows the assigned agent
And the ticket status becomes Assigned
```

### E2E: Escalate Ticket

```text
Given an Agent is viewing an active ticket
When the Agent escalates the ticket with a reason
Then the ticket status becomes Escalated
And the Supervisor can see it in the escalation queue
```

### E2E: Viewer Read-Only Access

```text
Given a Viewer logs into OpsSphere
When the Viewer opens the ticket detail page
Then ticket information is visible
But create, edit, assign, escalate, resolve, and close actions are not available
```

## 10.6 E2E Completion Criteria

A critical workflow should be E2E-covered when:

- It represents a core use case.
- It spans frontend, API, application logic, and database.
- It validates role or scope behavior.
- It is important for demo or portfolio review.
- It protects against high-risk regression.

---

# 11. User Acceptance Testing Strategy

## 11.1 Purpose

User Acceptance Testing validates that OpsSphere supports the intended business workflows.

UAT scenarios should be written in business language and connected to use cases.

## 11.2 UAT Participants

Potential UAT reviewers:

- Technical Owner.
- Simulated Admin.
- Simulated Operations Manager.
- Simulated Supervisor.
- Simulated Agent.
- Simulated Viewer.

Because OpsSphere is a portfolio project, UAT can be performed using role-based test personas and scripted review scenarios.

## 11.3 UAT Scenario Format

Each UAT scenario should include:

```text
Scenario ID
Related Use Case
Actor
Goal
Preconditions
Steps
Expected Result
Pass / Fail
Notes
```

---

# 12. Use Case to Test Coverage Matrix

| Use Case | Unit Tests | Integration Tests | API Tests | Frontend Tests | E2E Tests | UAT |
|---|---:|---:|---:|---:|---:|---:|
| UC-001 Authenticate User | Yes | Yes | Yes | Yes | Yes | Yes |
| UC-002 Create Ticket | Yes | Yes | Yes | Yes | Yes | Yes |
| UC-003 Assign Ticket | Yes | Yes | Yes | Yes | Yes | Yes |
| UC-004 Update Ticket Status | Yes | Yes | Yes | Yes | Yes | Yes |
| UC-005 Add Internal Comment | Yes | Yes | Yes | Yes | Yes | Yes |
| UC-006 Escalate Ticket | Yes | Yes | Yes | Yes | Yes | Yes |
| UC-007 Resolve Ticket | Yes | Yes | Yes | Yes | Yes | Yes |
| UC-008 Close Ticket | Yes | Yes | Yes | Yes | Yes | Yes |
| UC-009 View SLA Dashboard | Limited | Yes | Yes | Yes | Yes | Yes |
| UC-010 Manage Users | Yes | Yes | Yes | Yes | Limited | Yes |

---

# 13. UAT Scenarios

## UAT-001: Authenticate User

| Field | Value |
|---|---|
| Related Use Case | UC-001 Authenticate User |
| Actor | Admin, Operations Manager, Supervisor, Agent, Viewer |
| Goal | Confirm that valid users can access OpsSphere and invalid users cannot. |

### Preconditions

- Test users exist.
- Users have valid roles.
- At least one inactive user exists.

### Steps

1. Open the login page.
2. Log in with a valid active user.
3. Confirm successful access.
4. Log out.
5. Attempt login with invalid credentials.
6. Attempt login with a deactivated user.

### Expected Result

- Active users can log in.
- Invalid credentials are rejected.
- Deactivated users are rejected.
- Protected pages require authentication.

---

## UAT-002: Create Ticket

| Field | Value |
|---|---|
| Related Use Case | UC-002 Create Ticket |
| Actor | Agent |
| Goal | Confirm that an Agent can create a ticket within assigned scope. |

### Preconditions

- Agent is active.
- Agent belongs to a valid account or campaign.
- Customer exists or can be created.
- Account and campaign exist.

### Steps

1. Log in as Agent.
2. Open the ticket creation page.
3. Select customer, account, and campaign.
4. Enter category, priority, subject, and description.
5. Submit the ticket.
6. Open the created ticket.

### Expected Result

- Ticket is created.
- Ticket has initial status.
- Ticket has customer, account, and campaign context.
- Ticket has SLA information.
- Ticket creation is visible in ticket history or audit history.

---

## UAT-003: Assign Ticket

| Field | Value |
|---|---|
| Related Use Case | UC-003 Assign Ticket |
| Actor | Supervisor |
| Goal | Confirm that a Supervisor can assign a ticket to an eligible Agent. |

### Preconditions

- Supervisor is assigned to the account or campaign.
- Agent is active and eligible.
- Open ticket exists.

### Steps

1. Log in as Supervisor.
2. Open the ticket detail page.
3. Select the assignment action.
4. Choose an eligible Agent.
5. Confirm assignment.

### Expected Result

- Ticket is assigned to the selected Agent.
- Ticket status changes to Assigned when applicable.
- Assignment is visible in ticket history.
- Assignment is recorded in audit history.

---

## UAT-004: Update Ticket Status

| Field | Value |
|---|---|
| Related Use Case | UC-004 Update Ticket Status |
| Actor | Agent |
| Goal | Confirm that ticket status can move through valid workflow transitions. |

### Preconditions

- Agent has access to the ticket.
- Ticket exists and is assigned.

### Steps

1. Log in as Agent.
2. Open an assigned ticket.
3. Change status to In Progress.
4. Change status to Waiting for Customer.
5. Change status back to In Progress.
6. Attempt an invalid transition if available.

### Expected Result

- Valid status transitions are accepted.
- Invalid transitions are rejected.
- Status history is updated.
- Audit history records the changes.

---

## UAT-005: Add Internal Comment

| Field | Value |
|---|---|
| Related Use Case | UC-005 Add Internal Comment |
| Actor | Agent |
| Goal | Confirm that internal comments can be added to tickets. |

### Preconditions

- Agent has access to the ticket.
- Ticket is not closed.

### Steps

1. Log in as Agent.
2. Open a ticket.
3. Add an internal comment.
4. Submit the comment.
5. Refresh or reopen the ticket.

### Expected Result

- Comment is saved.
- Comment shows author and timestamp.
- Empty comments are rejected.
- Comment is visible only to authorized internal users.

---

## UAT-006: Escalate Ticket

| Field | Value |
|---|---|
| Related Use Case | UC-006 Escalate Ticket |
| Actor | Agent, Supervisor |
| Goal | Confirm that tickets can be escalated with a reason and reviewed by a Supervisor. |

### Preconditions

- Active ticket exists.
- Agent has access to the ticket.
- Supervisor has scope over the ticket.

### Steps

1. Log in as Agent.
2. Open an active ticket.
3. Select the escalate action.
4. Enter an escalation reason.
5. Submit escalation.
6. Log in as Supervisor.
7. Open the escalation queue.

### Expected Result

- Ticket status becomes Escalated.
- Escalation reason is saved.
- Ticket appears in supervisor escalation view.
- Escalation is recorded in ticket history and audit history.

---

## UAT-007: Resolve Ticket

| Field | Value |
|---|---|
| Related Use Case | UC-007 Resolve Ticket |
| Actor | Agent |
| Goal | Confirm that an active ticket can be resolved. |

### Preconditions

- Ticket is assigned or in progress.
- Agent has permission to resolve the ticket.

### Steps

1. Log in as Agent.
2. Open an active ticket.
3. Enter resolution information.
4. Resolve the ticket.

### Expected Result

- Ticket status becomes Resolved.
- Resolution details are saved.
- SLA outcome is preserved.
- Resolution is recorded in audit history.

---

## UAT-008: Close Ticket

| Field | Value |
|---|---|
| Related Use Case | UC-008 Close Ticket |
| Actor | Agent or Supervisor |
| Goal | Confirm that only resolved tickets can be closed. |

### Preconditions

- Resolved ticket exists.
- User has permission to close the ticket.

### Steps

1. Log in as authorized user.
2. Open a resolved ticket.
3. Close the ticket.
4. Attempt to modify the closed ticket.

### Expected Result

- Resolved ticket can be closed.
- Closed ticket cannot be modified unless reopening is allowed.
- Closure is recorded in audit history.

---

## UAT-009: View SLA Dashboard

| Field | Value |
|---|---|
| Related Use Case | UC-009 View SLA Dashboard |
| Actor | Supervisor, Operations Manager, Viewer |
| Goal | Confirm that SLA dashboard data is visible according to role and scope. |

### Preconditions

- Tickets exist with different SLA states.
- Users have assigned scopes.

### Steps

1. Log in as Supervisor.
2. Open the SLA dashboard.
3. Review tickets within scope.
4. Log in as Viewer.
5. Open the SLA dashboard.
6. Confirm read-only access.

### Expected Result

- SLA dashboard shows scoped data.
- At-risk and breached tickets are visible.
- Users cannot see data outside their scope.
- Viewer cannot modify records.

---

## UAT-010: Manage Users

| Field | Value |
|---|---|
| Related Use Case | UC-010 Manage Users |
| Actor | Admin |
| Goal | Confirm that Admin can manage users, roles, and scopes. |

### Preconditions

- Admin user exists.
- Roles and operational structure exist.

### Steps

1. Log in as Admin.
2. Create a new user.
3. Assign a role.
4. Assign operational scope.
5. Deactivate the user.
6. Attempt login with deactivated user.

### Expected Result

- User is created.
- Role is assigned.
- Scope is assigned.
- Deactivated user cannot log in.
- User management changes are recorded in audit history.

---

# 14. Business Rule Test Coverage

The following business rules should receive explicit automated test coverage.

| Area | Rule Type | Test Level |
|---|---|---|
| Authentication | Only active users can authenticate | API / Integration |
| Authorization | Protected resources require valid token | API |
| RBAC | Viewers cannot modify data | API / E2E |
| Scope | Users cannot access records outside assigned scope | Integration / API |
| Ticket Creation | Tickets require customer, account, campaign, priority, and description | Unit / API |
| Assignment | Tickets can only be assigned to eligible active agents | Unit / Integration |
| Workflow | Invalid status transitions are rejected | Unit / API |
| Escalation | Escalated tickets require a reason | Unit / API |
| Resolution | Resolved tickets preserve resolution details | Integration |
| Closure | Tickets must be resolved before closure | Unit / API |
| Comments | Empty comments are rejected | Unit / API |
| SLA | SLA state is visible and filterable | Integration / E2E |
| Audit | Critical changes create audit records | Integration |

---

# 15. Test Data Strategy

## 15.1 Test Data Principles

Test data should be:

- Fictional.
- Repeatable.
- Easy to reset.
- Representative of the business domain.
- Safe for public portfolio use.
- Aligned with documented examples.

## 15.2 Recommended Seed Data

```text
Region:
  - Latin America

Countries:
  - Mexico
  - Colombia
  - Costa Rica

Accounts:
  - NovaBank
  - Streamly
  - Shopora

Campaigns:
  - Credit Card Support
  - Fraud Review Support
  - Creator Support
  - Content Moderation Appeals

Roles:
  - Admin
  - OperationsManager
  - Supervisor
  - Agent
  - Viewer

Ticket Statuses:
  - Open
  - Assigned
  - In Progress
  - Waiting for Customer
  - Escalated
  - Resolved
  - Closed

Priorities:
  - Low
  - Normal
  - High
  - Critical

SLA States:
  - Within SLA
  - At Risk
  - Breached
  - Completed
```

## 15.3 Test User Personas

| Persona | Role | Scope |
|---|---|---|
| Admin User | Admin | Global |
| LATAM Manager | Operations Manager | Latin America |
| NovaBank Supervisor | Supervisor | NovaBank |
| Credit Card Agent | Agent | NovaBank / Credit Card Support |
| Streamly Agent | Agent | Streamly / Creator Support |
| Read-Only Viewer | Viewer | Latin America or selected accounts |

---

# 16. Test Environment Strategy

## 16.1 Local Development Testing

Local development should support:

- Backend tests from the command line.
- Frontend tests from the command line.
- SQL Server through Docker.
- Full application startup through Docker Compose.
- Seeded fictional data.
- Local Swagger/OpenAPI testing.

Expected local commands may include:

```text
dotnet test
npm test
npm run test
npm run e2e
docker compose up
```

Final commands may change based on implementation decisions.

## 16.2 CI Testing Environment

The CI pipeline should run tests automatically on pull requests and main branch updates.

Expected CI validation:

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

## 16.3 Test Database Strategy

The test database should be isolated from development and production data.

Recommended approach:

```text
Unit tests:
  - No database.

Integration tests:
  - SQL Server Testcontainer.

Local manual testing:
  - Docker Compose SQL Server.

CI:
  - SQL Server container or test service.
```

---

# 17. CI Quality Gates

A pull request should not be considered ready if:

- Backend does not build.
- Frontend does not build.
- Unit tests fail.
- Integration tests fail.
- Critical API tests fail.
- Formatting or linting checks fail if configured.
- Test failures are ignored without explanation.

Recommended quality gate:

```text
Pull Request
  → Build backend
  → Run backend tests
  → Build frontend
  → Run frontend tests
  → Run integration/API tests
  → Review changed files
  → Confirm issue scope
```

---

# 18. Coverage Priorities

OpsSphere should prioritize coverage based on business risk.

## 18.1 Highest Priority

The highest priority areas are:

- Authentication.
- Authorization.
- Scope filtering.
- Ticket creation.
- Ticket assignment.
- Ticket status workflow.
- Escalation.
- Resolution and closure.
- Audit logging.
- SLA state visibility.

## 18.2 Medium Priority

Medium priority areas are:

- Dashboard queries.
- Reporting filters.
- Customer management.
- User management.
- Operational structure management.
- Frontend form validation.

## 18.3 Lower Priority

Lower priority areas are:

- Visual styling.
- Static content.
- Simple read-only screens.
- Non-critical UI layout variations.

---

# 19. Definition of Done for Tested Features

A feature should be considered done only when:

- Business rules are implemented.
- Required unit tests are added.
- Required integration or API tests are added.
- Frontend behavior is tested where applicable.
- Authorization behavior is validated.
- Scope behavior is validated where applicable.
- Audit behavior is validated for critical actions.
- Error cases are covered.
- Tests pass locally.
- Tests pass in CI.
- Documentation is updated when behavior changes.

---

# 20. Testing Risks and Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Too many tests depend on UI | Tests become slow and fragile | Keep most coverage in unit, integration, and API tests |
| Business rules only tested through E2E | Hard to diagnose failures | Test domain and application rules directly |
| In-memory database hides SQL issues | False confidence | Use SQL Server Testcontainers for integration tests |
| Authorization is only hidden in frontend | Security risk | Test backend authorization policies directly |
| Scope filtering is under-tested | Data exposure risk | Add integration and API tests for cross-scope access |
| Audit logging is forgotten | Loss of traceability | Include audit expectations in workflow tests |
| Test data becomes inconsistent | Flaky tests | Use builders, factories, and deterministic seeds |
| E2E suite becomes too slow | CI delays | Keep E2E focused on critical journeys |

---

# 21. Initial Test Backlog

The following test backlog should guide early implementation.

## Backend Unit Tests

- Ticket creation validation.
- Ticket status transition rules.
- Ticket closure rule.
- Escalation reason rule.
- Empty comment rule.
- Assignment eligibility rule.
- Priority and SLA target rule.
- Viewer modification restriction helpers.
- Scope matching helpers.

## Backend Integration Tests

- Create ticket persists ticket, SLA, and audit record.
- Assign ticket updates owner, status, history, and audit.
- Escalate ticket creates escalation and audit.
- Resolve ticket preserves SLA outcome.
- Close ticket rejects unresolved tickets.
- Query tickets filters by user scope.
- Dashboard query filters by user scope.
- User deactivation blocks login.

## API Tests

- Login success.
- Login failure.
- Protected endpoint without token.
- Viewer cannot create ticket.
- Agent cannot create ticket outside scope.
- Supervisor can assign ticket within scope.
- Invalid ticket request returns validation error.
- Escalation without reason is rejected.
- Closed ticket cannot be modified.

## Frontend Tests

- Login form validation.
- Ticket creation form validation.
- Escalation form requires reason.
- Viewer cannot see write actions.
- Route guard redirects unauthenticated users.
- Dashboard renders SLA state.
- API validation errors are displayed.

## E2E Tests

- Login and dashboard access.
- Create ticket.
- Assign ticket.
- Escalate ticket.
- Resolve and close ticket.
- Viewer read-only journey.
- Admin creates and deactivates user.

---

# 22. Summary

The OpsSphere testing strategy is designed to validate the platform as an enterprise-grade application.

Testing will not focus only on whether screens load or endpoints respond. It will validate the business behavior that makes OpsSphere valuable:

- Role-based access.
- Scope-based visibility.
- Ticket lifecycle correctness.
- SLA tracking.
- Escalation handling.
- Auditability.
- Operational dashboard reliability.
- Maintainable backend and frontend workflows.

This strategy ensures that OpsSphere can be built with confidence as a testable, maintainable, deployable, and supportable enterprise application.