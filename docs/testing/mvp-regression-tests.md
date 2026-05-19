# MVP Regression Test Guide

This document describes the automated integration test coverage for the OpsSphere MVP. All tests run against an in-memory SQLite database using fictional seed data. No real infrastructure or credentials are required.

## 1. Authentication

Tests in `tests/OpsSphere.IntegrationTests/Auth/AuthApiTests.cs`.

- Login with a valid seeded user returns a JWT with correct claims.
- Login with an invalid password returns the generic error message.
- Login with an unknown email returns the generic error message.
- Login with an inactive user returns the generic error message.
- The error response for an inactive user and a missing user are identical (timing-safe).
- `GET /api/auth/me` without a token returns 401.
- `GET /api/auth/me` with a valid token returns the current user profile including roles, permissions, and scopes.
- The JWT payload does not contain permissions, scopes, or the signing key.
- Auth responses do not expose the password hash.

## 2. Authorization and Scoping

Tests in `tests/OpsSphere.IntegrationTests/Authorization/AuthorizationApiTests.cs`.

- Role-based permission checks enforce access to protected endpoints.
- Scope-based authorization filters results server-side by region, account, and campaign.
- 403 responses do not expose the signing key or password data.
- Admin bypasses scope restrictions and sees all data.

## 3. User Management

Tests in `tests/OpsSphere.IntegrationTests/UserManagement/UserManagementApiTests.cs`.

- Admin can create, view, update, assign roles, and deactivate users.
- Duplicate email on create returns 409.
- Missing required fields on create return 400 with field-level error details.
- A deactivated user cannot log in.
- Non-admin roles are forbidden from all user management endpoints.
- User management responses do not expose password hashes, temporary passwords, or signing keys.

## 4. Organization Management

Tests in `tests/OpsSphere.IntegrationTests/OrganizationManagement/OrganizationApiTests.cs`.

- Admin can create, update, and deactivate the full hierarchy: region, country, account, campaign.
- Duplicate codes on create return 409.
- Missing or inactive parent entities are rejected with validation errors.
- Deactivating a parent with active children is rejected as a business rule violation.
- Scoped users see only the organization records within their assigned scope.
- User scopes can be assigned and reactivated according to role constraints.

## 5. Customer Management

Tests in `tests/OpsSphere.IntegrationTests/CustomerManagement/CustomerManagementApiTests.cs`.

- Admin can create, update, and deactivate customers.
- Inactive or missing accounts are rejected on customer creation.
- Scoped agents and supervisors see only customers belonging to their account scope.
- Cross-scope customer reads and writes return 404 to non-admin users.
- Customer ticket history is returned correctly.
- Audit logs for customer operations do not contain PII.

## 6. Ticket Management

Tests in `tests/OpsSphere.IntegrationTests/TicketManagement/TicketManagementApiTests.cs` and `TicketLifecycleGapsApiTests.cs`.

- Agents and supervisors can create tickets within their scope.
- Ticket creation sets status to Open and initializes SLA state and due date.
- Ticket creation creates a status history row and an SLA state row.
- Invalid priority values, missing required fields, and cross-scope customers are rejected.
- Out-of-scope campaign creates return 404.
- `GET /api/tickets` returns only tickets within the caller's scope.
- Cross-scope ticket reads return 404.
- Priority updates return the previous and new priority values.
- The escalation queue returns escalated tickets scoped to the caller.
- Ticket status history is returned and grows after each status change.
- Comments can be added and retrieved from `GET /api/tickets/{id}/comments`.

## 7. Audit Logs

Tests in `tests/OpsSphere.IntegrationTests/AuditManagement/AuditApiTests.cs` and `TicketLifecycleAuditTests.cs`.

- Audit logs are written for all ticket lifecycle actions: created, assigned, status changed, priority changed, escalated, resolved, closed, comment added.
- `GET /api/audit-logs` returns logs scoped to the caller's authorization profile.
- `GET /api/audit-logs/{id}` returns detail with previous/new values and correlation ID.
- `GET /api/audit-logs/entity/{entityType}/{entityId}` returns the full history for a given entity.
- Out-of-scope entity history returns 404.
- List responses do not expose previousValue or newValue fields.
- Date range filters work correctly.
- Invalid date ranges return 400.
- Audit logs cannot be modified: PUT and DELETE return 404 or 405.

## 8. SLA and Dashboard

Tests in `tests/OpsSphere.IntegrationTests/TicketManagement/TicketManagementApiTests.cs`, `DashboardManagement/DashboardApiTests.cs`, and `DashboardManagement/SlaMultiStateDashboardTests.cs`.

- `GET /api/sla/summary` returns evaluated counts for all four SLA states: WithinSla, AtRisk, Breached, Completed.
- SLA state is evaluated at request time, not from the stored value.
- SLA summary is scoped to the caller's authorization profile.
- The operational dashboard requires the `dashboard.view` permission.
- Dashboard totals and groupings (by status, priority, SLA state, account, campaign, agent, supervisor) are correct.
- Dashboard filters (account, campaign, status, date range) intersect correctly with scope.
- An out-of-scope account filter returns zero counts.
- An invalid date range filter returns 400.

## 9. Health and Middleware

Tests in `tests/OpsSphere.IntegrationTests/Health/HealthCheckTests.cs` and `Middleware/ExceptionHandlingTests.cs`.

- `GET /health` and `GET /health/details` return 200 with correct content type.
- Health responses do not expose the signing key, password data, or connection string details.
- All responses include an `X-Correlation-Id` header.
- Unhandled exceptions return a 500 response with the standard error envelope.
- Error responses do not expose internal exception messages, stack traces, or secrets.

## 10. Persistence

Tests in `tests/OpsSphere.IntegrationTests/Persistence/`.

- Seed data is applied correctly: all seeded entities, roles, permissions, and scopes are present after startup.
- Soft-deleted entities are excluded from all query results.
- All EF Core migrations apply cleanly to a real SQL Server instance (Heavy category, CI/CD only).

## Running the Tests

```
dotnet test tests/OpsSphere.IntegrationTests --filter "Category!=Heavy"
```

To run the migration smoke test (requires Docker):

```
dotnet test tests/OpsSphere.IntegrationTests --filter "Category=Heavy"
```

## CI Validation

For how these commands map to GitHub Actions CI jobs, which jobs are required before merge, Docker and Testcontainers notes, and failure triage guidance, see [ci-validation.md](ci-validation.md).
