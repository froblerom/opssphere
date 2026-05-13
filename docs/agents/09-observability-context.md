# OpsSphere Observability Context

## Purpose

This document provides compressed observability and support context for agents working inside OpsSphere.

Use this file when the task touches:

- Logging
- Error handling
- Health checks
- Audit logs
- Metrics
- Application Insights
- Correlation IDs
- Support troubleshooting
- Operational dashboards
- Incident review

---

## Observability Goals

OpsSphere observability should:

```text
Detect application problems early.
Make production issues easier to diagnose.
Provide traceability for critical business actions.
Support operational monitoring for tickets, SLAs, escalations, and workload.
Separate technical logs from business audit logs.
Provide reliable signals for support and troubleshooting.
Prepare the application for Azure Application Insights.
Make the system easier to maintain after deployment.
```

---

## Observability Principles

```text
Logs should be structured, searchable, and consistent.
Health checks should clearly indicate whether the system can operate.
Errors should be handled consistently and safely.
Audit logs should preserve business traceability.
Metrics should describe both technical health and operational behavior.
Sensitive data should not be exposed in logs.
Support workflows should be based on evidence, not guessing.
Observability should be included early instead of added only after failures occur.
```

---

## Structured Logging

OpsSphere uses:

```text
Serilog
```

Logs may write to:

```text
Console
File for local development, optional
Application Insights for future Azure deployment
```

Recommended log fields:

```text
Timestamp
Level
Message
CorrelationId
RequestId
UserId, when available
UserRole, when available
TraceId, when available
Endpoint
HttpMethod
StatusCode
ElapsedMilliseconds
ExceptionType, when applicable
Environment
ApplicationVersion
```

---

## What Should Be Logged

```text
Application startup
Application shutdown
API request summaries
Authentication failures
Authorization failures
Validation failures when useful for diagnostics
Unhandled exceptions
Database connectivity failures
Background job execution
SLA evaluation job results
External service failures in future phases
Deployment version or build identifier
Health check failures
```

---

## What Must Not Be Logged

```text
Passwords
Password hashes
JWT tokens
Refresh tokens
Full connection strings
API keys
Secret values
Personal data beyond what is necessary
Full customer issue descriptions when not required
Payment or financial data, if introduced in future
```

---

## Correlation ID Strategy

Each API request should have a correlation ID.

Recommended behavior:

```text
If request includes correlation ID:
  Use incoming correlation ID.

If request does not include correlation ID:
  Generate a new correlation ID.
```

Recommended header:

```text
X-Correlation-Id
```

Correlation ID should appear in:

```text
API logs
Error responses when appropriate
Application logs
Audit logs when useful
Application Insights traces
```

---

## Health Checks

OpsSphere should expose:

```http
GET /health
```

Optional detailed endpoint:

```http
GET /health/details
```

Detailed endpoint should be protected or disabled in production if it exposes sensitive information.

Minimum checks:

```text
API process is running
Database connection is available
Required configuration is present
```

Future checks:

```text
Background worker status
SLA monitoring job status
Application Insights connectivity
Email service availability
Storage service availability
Key Vault access
```

Health statuses:

```text
Healthy
Degraded
Unhealthy
```

---

## Error Handling Principles

```text
Do not expose internal stack traces to users.
Return consistent API error responses.
Log technical details internally.
Include correlation ID in error responses when useful.
Separate validation errors from authorization errors and unexpected failures.
Preserve business rule failure messages when safe.
Avoid leaking sensitive information.
```

Recommended error response:

```json
{
  "errorCode": "TICKET_VALIDATION_FAILED",
  "message": "The request could not be completed because one or more fields are invalid.",
  "correlationId": "f4d2c9e3b7a14c66a0d9f40e2c5b91ab",
  "details": [
    {
      "field": "priority",
      "message": "Priority is required."
    }
  ]
}
```

---

## Error Categories

| Category | Example | Expected HTTP Status |
|---|---|---|
| Validation Error | Missing required ticket field | `400 Bad Request` |
| Authentication Error | Missing or expired token | `401 Unauthorized` |
| Authorization Error | User attempts action outside scope | `403 Forbidden` |
| Not Found Error | Ticket does not exist | `404 Not Found` |
| Business Rule Error | Closing unresolved ticket | `400 Bad Request` |
| Conflict Error | Duplicate user email | `409 Conflict` |
| Unexpected Error | Unhandled exception | `500 Internal Server Error` |

---

## Business Audit Logs

Business audit logs are different from technical logs.

Audit logs preserve traceability for business actions such as:

```text
Ticket created
Ticket assigned
Ticket status changed
Ticket escalated
Ticket resolved
Ticket closed
Internal comment added
User created
User deactivated
Role changed
Scope changed
Organization record changed
Permission changed
```

Audit logs should support answering:

```text
Who did it?
When did it happen?
What changed?
Which record was affected?
What was the previous value?
What is the new value?
Was there a reason?
Which correlation ID connects the action to technical logs?
```

---

## Metrics

Technical metrics may include:

```text
Request count
Request duration
Error rate
Database query duration
Authentication failure count
Authorization failure count
Health check status
Background job duration
```

Operational metrics may include:

```text
Open tickets
Overdue tickets
Tickets by status
Tickets by priority
Tickets by account
Tickets by campaign
Tickets by agent
Tickets by supervisor
SLA state counts
Escalated ticket count
Average resolution time
Ticket aging
```

---

## Support Investigation Flow

```text
Receive issue report
  → Identify affected user and workflow
  → Check severity
  → Collect correlation ID if available
  → Check health endpoint
  → Review application logs
  → Review audit logs
  → Reproduce in staging or local environment
  → Identify root cause
  → Fix or document workaround
  → Validate fix
  → Update support notes if needed
```

---

## Support Information to Collect

```text
User role
User scope
Time of issue
Affected ticket number
Affected account or campaign
Action attempted
Error message shown
Correlation ID
Browser or client information, if relevant
Recent deployment version
```

Do not collect sensitive data unless strictly necessary.

---

## Troubleshooting Scenarios

## User Cannot Log In

Check:

```text
Is the API healthy?
Is the database available?
Does the user exist?
Is the user active?
Does the user have a role?
Are credentials valid?
Are authentication errors being logged?
```

Signals:

```text
Authentication failure logs
User status
Health check
API error response
```

---

## User Cannot See Expected Tickets

Check:

```text
Does the user have the correct role?
Does the user have assigned operational scope?
Does the ticket belong to the user's scope?
Is the ticket active or filtered out?
Is the dashboard or ticket list applying filters?
```

Signals:

```text
Scope access logs
Ticket query filters
User scope records
Audit history
```

---

## Ticket Creation Fails

Check:

```text
Are required fields present?
Is the user authorized to create tickets?
Is the selected account or campaign inside user scope?
Does the customer exist?
Is the database available?
Was a business rule violated?
```

Signals:

```text
Validation errors
Authorization logs
Correlation ID
API response
Database logs
```

---

## Ticket Cannot Be Closed

Check:

```text
Is the ticket resolved?
Is the user authorized to close it?
Is the ticket inside user scope?
Is the ticket already closed?
Does workflow allow the transition?
```

Signals:

```text
Business rule error
Ticket status history
Audit logs
Workflow validation logs
```

---

## SLA Dashboard Looks Incorrect

Check:

```text
Are SLA states calculated correctly?
Are filters applied correctly?
Does user scope restrict visible data?
Are background SLA checks running if implemented?
Are there old test records affecting counts?
```

Signals:

```text
SLA state records
Dashboard query logs
Background job logs
Audit records
```

---

## Application Is Slow

Check:

```text
Which endpoint is slow?
Is the database query slow?
Is the dashboard loading too much data?
Are indexes missing?
Did slowness start after a deployment?
```

Signals:

```text
Request duration metrics
Database dependency telemetry
Application Insights performance data
Logs with elapsed milliseconds
```

---

## Support Knowledge Base

Recommended folder:

```text
docs/support/
```

Possible files:

```text
docs/support/troubleshooting.md
docs/support/common-errors.md
docs/support/deployment-checklist.md
docs/support/incident-template.md
```

Support article format:

```text
Title
Symptoms
Affected Area
Likely Causes
How to Diagnose
Resolution
Related Logs or Metrics
Related Business Rules
```