# Observability and Support

## Document Information

| Field | Value |
|---|---|
| Project | OpsSphere |
| Document | Observability and Support |
| File | `docs/19-observability-and-support.md` |
| Version | 1.0 |
| Status | Draft |
| Project Type | Enterprise Support Operations Platform |
| Related Issue | #6 |
| Phase | Delivery Planning |

---

## 1. Purpose

This document defines the initial observability and support strategy for OpsSphere.

The purpose of this document is to describe how the platform will be monitored, logged, diagnosed, supported, and reviewed from an operational perspective.

OpsSphere is planned as a maintainable, testable, deployable, and supportable enterprise application. For that reason, the system must provide clear visibility into application health, errors, audit events, performance signals, and operational usage.

This document covers:

- Structured logging.
- Health checks.
- Error handling.
- Audit logs.
- Metrics.
- Application Insights readiness.
- Operational dashboards.
- Support process expectations.
- Troubleshooting practices.
- Operational review expectations.

---

## 2. Observability Goals

The main goals of the OpsSphere observability strategy are:

1. Detect application problems early.
2. Make production issues easier to diagnose.
3. Provide traceability for critical business actions.
4. Support operational monitoring for tickets, SLAs, escalations, and workload.
5. Separate technical logs from business audit logs.
6. Provide reliable signals for support and troubleshooting.
7. Prepare the application for Azure Application Insights.
8. Make the system easier to maintain after deployment.

---

## 3. Observability Principles

OpsSphere will follow these principles:

- Logs should be structured, searchable, and consistent.
- Health checks should clearly indicate whether the system can operate.
- Errors should be handled consistently and safely.
- Audit logs should preserve business traceability.
- Metrics should describe both technical health and operational behavior.
- Sensitive data should not be exposed in logs.
- Support workflows should be based on evidence, not guessing.
- Observability should be included early instead of added only after failures occur.

---

# 4. Observability Areas

OpsSphere observability is divided into the following areas:

| Area | Purpose |
|---|---|
| Structured Logging | Capture technical events in a consistent searchable format |
| Health Checks | Confirm that required system dependencies are available |
| Error Handling | Return safe user-facing errors and log diagnostic details |
| Audit Logs | Preserve traceability of critical business actions |
| Metrics | Measure technical and operational behavior |
| Application Insights | Centralize telemetry, traces, failures, and performance data in Azure |
| Operational Dashboard | Show business-facing operational health |
| Support Process | Define how issues are triaged and resolved |

---

# 5. Structured Logging

## 5.1 Purpose

Structured logging allows OpsSphere to record technical events in a consistent format that can be searched, filtered, and analyzed.

Structured logs are intended for developers, support owners, and technical operators.

## 5.2 Logging Technology

OpsSphere will use:

```text
Serilog
```

Serilog should be configured to produce structured logs that can be written to:

```text
Console
File, optional for local development
Application Insights, future Azure deployment
```

## 5.3 Logging Format

Logs should include consistent fields.

Recommended fields:

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

## 5.4 Log Levels

OpsSphere should use log levels consistently.

| Level | Usage |
|---|---|
| Trace | Very detailed diagnostic information, normally disabled in production |
| Debug | Development diagnostics |
| Information | Normal application flow and important lifecycle events |
| Warning | Unexpected but recoverable situations |
| Error | Failed operations requiring investigation |
| Critical | Severe failures that may make the system unavailable |

## 5.5 What Should Be Logged

The system should log:

- Application startup.
- Application shutdown.
- API requests and responses at summary level.
- Authentication failures.
- Authorization failures.
- Validation failures when useful for diagnostics.
- Unhandled exceptions.
- Database connectivity failures.
- Background job execution.
- SLA evaluation job results.
- External service failures in future phases.
- Deployment version or build identifier.
- Health check failures.

## 5.6 What Should Not Be Logged

The system must avoid logging sensitive information.

Do not log:

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

## 5.7 Example Structured Log Events

```text
Event: UserLoginFailed
Level: Warning
Fields:
  - CorrelationId
  - EmailHash or UserIdentifier
  - ReasonCode
  - ClientIp, if available
```

```text
Event: TicketCreated
Level: Information
Fields:
  - CorrelationId
  - TicketId
  - TicketNumber
  - AccountId
  - CampaignId
  - CreatedByUserId
```

```text
Event: ScopeAccessDenied
Level: Warning
Fields:
  - CorrelationId
  - UserId
  - UserRole
  - RequestedEntity
  - RequestedEntityId
  - ReasonCode
```

```text
Event: UnhandledException
Level: Error
Fields:
  - CorrelationId
  - Endpoint
  - ExceptionType
  - SafeErrorMessage
```

---

# 6. Correlation and Request Tracing

## 6.1 Purpose

Correlation IDs help connect logs generated during the same request or operation.

This is important when troubleshooting workflows that pass through multiple layers.

Example flow:

```text
Angular Frontend
  → ASP.NET Core API
  → Application Handler
  → Domain Logic
  → EF Core / SQL Server
  → Audit Log
```

## 6.2 Correlation ID Strategy

Each API request should have a correlation ID.

Recommended behavior:

```text
If request includes correlation ID:
  Use incoming correlation ID.

If request does not include correlation ID:
  Generate a new correlation ID.
```

The correlation ID should be included in:

- API logs.
- Error responses when appropriate.
- Application logs.
- Audit logs when useful.
- Application Insights traces.

## 6.3 Correlation Header

Recommended header:

```text
X-Correlation-Id
```

## 6.4 Support Value

When a user reports an issue, support can use the correlation ID to locate related logs.

Example:

```text
User reports: "I could not create ticket OPS-000145."

Support checks:
  - CorrelationId from error response or logs
  - API request log
  - Validation error log
  - Authorization decision log
  - Audit log presence or absence
```

---

# 7. Health Checks

## 7.1 Purpose

Health checks confirm whether the application and its dependencies are available.

Health checks are used by:

- Developers.
- Support owners.
- Deployment validation.
- Cloud hosting platforms.
- Monitoring tools.

## 7.2 Health Check Endpoint

OpsSphere should expose a health check endpoint.

```text
GET /health
```

Optional detailed endpoint:

```text
GET /health/details
```

The detailed endpoint should be protected or disabled in production if it exposes sensitive information.

## 7.3 Minimum Health Checks

The minimum health checks should include:

```text
API process is running
Database connection is available
Required configuration is present
```

## 7.4 Future Health Checks

Future health checks may include:

```text
Background worker status
SLA monitoring job status
Application Insights connectivity
Email service availability
Storage service availability
Key Vault access
```

## 7.5 Health Check Statuses

| Status | Meaning |
|---|---|
| Healthy | Application and required dependencies are available |
| Degraded | Application is running but one or more non-critical dependencies have issues |
| Unhealthy | Application cannot operate correctly |

## 7.6 Example Health Response

```json
{
  "status": "Healthy",
  "timestampUtc": "2026-05-13T00:00:00Z",
  "checks": [
    {
      "name": "api",
      "status": "Healthy"
    },
    {
      "name": "sql-server",
      "status": "Healthy"
    },
    {
      "name": "configuration",
      "status": "Healthy"
    }
  ]
}
```

## 7.7 Health Check Use in Deployment

After deployment, the health endpoint should be used to confirm that the application started correctly.

Deployment validation should include:

```text
Deploy application
  → Call /health
  → Confirm database connectivity
  → Run smoke tests
  → Review logs
```

---

# 8. Error Handling

## 8.1 Purpose

Error handling ensures that failures are returned to users safely and consistently while preserving enough diagnostic detail for support and developers.

## 8.2 Error Handling Principles

OpsSphere should follow these error handling principles:

- Do not expose internal stack traces to users.
- Return consistent API error responses.
- Log technical details internally.
- Include a correlation ID in error responses when useful.
- Separate validation errors from authorization errors and unexpected failures.
- Preserve business rule failure messages when safe.
- Avoid leaking sensitive information.

## 8.3 API Error Response Format

A consistent error response should be used.

Recommended format:

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

## 8.4 Error Categories

| Category | Example | Expected HTTP Status |
|---|---|---|
| Validation Error | Missing required ticket field | `400 Bad Request` |
| Authentication Error | Missing or expired token | `401 Unauthorized` |
| Authorization Error | User attempts action outside scope | `403 Forbidden` |
| Not Found Error | Ticket does not exist | `404 Not Found` |
| Business Rule Error | Closing unresolved ticket | `400 Bad Request` |
| Conflict Error | Duplicate user email | `409 Conflict` |
| Unexpected Error | Unhandled exception | `500 Internal Server Error` |

## 8.5 Business Rule Error Examples

```text
Ticket cannot be closed before it is resolved.
Escalation reason is required.
Viewer users cannot modify operational records.
User cannot access records outside assigned scope.
Closed tickets cannot be modified unless reopened by an authorized role.
```

## 8.6 Frontend Error Handling

The Angular frontend should:

- Display clear validation messages.
- Redirect unauthenticated users to login.
- Show access denied messages for forbidden actions.
- Show safe generic messages for unexpected errors.
- Preserve correlation ID when available.
- Avoid exposing raw technical exceptions to users.

## 8.7 Error Logging Expectations

Unexpected errors should log:

```text
CorrelationId
Endpoint
HttpMethod
UserId, when available
ExceptionType
Safe message
Stack trace in technical logs
Environment
ApplicationVersion
```

Sensitive data should be excluded.

---

# 9. Audit Logs

## 9.1 Purpose

Audit logs preserve traceability for critical business actions.

Audit logs are not the same as technical logs.

Technical logs help developers diagnose system behavior.

Audit logs help the business answer:

```text
Who changed what?
When did it happen?
What was the previous value?
What is the new value?
Which business entity was affected?
```

## 9.2 Audit Log Scope

Audit logs should capture critical actions such as:

- User login events, optional depending on implementation.
- User created.
- User deactivated.
- Role assigned.
- Scope assigned.
- Region created or updated.
- Account created or updated.
- Campaign created or updated.
- Ticket created.
- Ticket assigned.
- Ticket reassigned.
- Ticket status changed.
- Ticket priority changed.
- Internal comment added.
- Ticket escalated.
- Ticket resolved.
- Ticket closed.
- SLA state changed when system-generated.
- Permission changed.

## 9.3 Audit Log Fields

Recommended audit fields:

```text
AuditLogId
TimestampUtc
ActorUserId
ActorRole
Action
EntityType
EntityId
EntityDisplayName
PreviousValue
NewValue
Reason
CorrelationId
Source
```

## 9.4 Audit Log Example

```json
{
  "auditLogId": "9fc0b4e8-41e2-4f51-9723-7058b4204b4a",
  "timestampUtc": "2026-05-13T00:00:00Z",
  "actorUserId": "2b7a9c5f-0f0e-4e35-ae1d-438de07d67a1",
  "actorRole": "Supervisor",
  "action": "TicketAssigned",
  "entityType": "Ticket",
  "entityId": "OPS-000145",
  "previousValue": {
    "assignedAgent": null,
    "status": "Open"
  },
  "newValue": {
    "assignedAgent": "Ana López",
    "status": "Assigned"
  },
  "reason": "Supervisor assigned ticket to eligible agent.",
  "correlationId": "f4d2c9e3b7a14c66a0d9f40e2c5b91ab",
  "source": "WebApp"
}
```

## 9.5 Audit Log Rules

Audit logs should follow these rules:

- Critical business actions must be audited.
- Audit records should be append-only.
- Audit records should not be edited by normal users.
- Audit visibility must respect role and scope.
- Audit records should preserve historical context.
- Audit records should remain available even if related users or operational records are deactivated.
- Sensitive data should be minimized.

## 9.6 Audit Log Access

Recommended access:

| Role | Audit Access |
|---|---|
| Admin | Global audit access |
| Operations Manager | Audit access within assigned regions |
| Supervisor | Audit access within assigned accounts or campaigns |
| Agent | Limited ticket-level history where authorized |
| Viewer | Read-only audit access within assigned scope |

---

# 10. Metrics

## 10.1 Purpose

Metrics provide measurable signals about system behavior.

OpsSphere should track both:

```text
Technical metrics
Operational metrics
```

Technical metrics help maintain the application.

Operational metrics help understand the support operation.

---

## 10.2 Technical Metrics

Recommended technical metrics:

| Metric | Purpose |
|---|---|
| Request count | Understand API usage |
| Request duration | Detect slow endpoints |
| Error rate | Detect failing operations |
| Authentication failure count | Detect login issues or suspicious activity |
| Authorization failure count | Detect access issues |
| Database query duration | Detect persistence bottlenecks |
| Health check status | Detect service availability |
| Background job duration | Monitor SLA worker performance |
| Failed background jobs | Detect automation failures |

## 10.3 Operational Metrics

Recommended operational metrics:

| Metric | Purpose |
|---|---|
| Open ticket count | Monitor workload |
| Assigned ticket count | Monitor active ownership |
| Overdue ticket count | Monitor SLA risk |
| At-risk ticket count | Detect tickets close to breach |
| Escalated ticket count | Monitor supervisor attention |
| Tickets by priority | Understand urgency mix |
| Tickets by status | Understand lifecycle distribution |
| Tickets by agent | Understand workload distribution |
| Tickets by supervisor | Understand team workload |
| Tickets by account | Understand client workload |
| Tickets by campaign | Understand operational workload |
| Average resolution time | Measure support efficiency |
| SLA compliance rate | Measure service performance |
| Reassignment count | Detect ownership instability |
| Comment count by ticket | Understand collaboration intensity |

## 10.4 Metric Dimensions

Metrics should support useful dimensions when possible:

```text
Region
Country
Account
Campaign
Supervisor
Agent
Priority
Status
SLA State
Date Range
```

## 10.5 Metrics Non-Goals

The MVP does not need to implement a full analytics platform.

The goal is to expose useful operational signals, not replace tools such as Power BI.

---

# 11. Application Insights

## 11.1 Purpose

Application Insights is the planned Azure monitoring and telemetry target for OpsSphere.

It will support:

- Request telemetry.
- Dependency telemetry.
- Exception tracking.
- Performance monitoring.
- Logs and traces.
- Availability checks.
- Dashboarding in Azure.
- Troubleshooting production issues.

## 11.2 Application Insights Readiness

OpsSphere should be designed so Application Insights can be enabled in Azure without major architectural changes.

The application should support:

```text
Application Insights connection string from configuration
Structured logs forwarded to Application Insights
Request telemetry
Exception telemetry
Dependency telemetry for SQL Server
Correlation IDs
Environment tags
Application version tags
```

## 11.3 Recommended Telemetry Properties

Telemetry should include:

```text
Environment
ApplicationName
ApplicationVersion
CorrelationId
UserRole, when safe
Endpoint
AccountId, when useful and safe
CampaignId, when useful and safe
```

## 11.4 Application Insights Usage Examples

Support can use Application Insights to answer:

```text
Which API endpoint is failing?
Which requests are slow?
Which exceptions increased after deployment?
Is the database dependency failing?
Did the error start after a specific release?
Which correlation ID belongs to this user-reported issue?
```

## 11.5 Application Insights Non-Goals for MVP

The MVP does not require:

- Advanced alerting rules.
- Complex dashboards.
- Distributed tracing across many microservices.
- Custom telemetry for every domain event.
- Production-scale cost optimization.

These may be added in later phases.

---

# 12. Operational Dashboard

## 12.1 Purpose

The operational dashboard provides business-facing visibility into ticket workload, SLA health, escalations, and team performance.

This dashboard is different from technical monitoring.

Technical monitoring answers:

```text
Is the application healthy?
```

Operational dashboard answers:

```text
Is the support operation healthy?
```

## 12.2 Dashboard Users

The operational dashboard may be used by:

- Operations Managers.
- Supervisors.
- Agents.
- Viewers.
- Admins, when needed.

## 12.3 Dashboard Scope Rules

Dashboard data must respect role and scope.

Examples:

```text
Operations Manager:
  Sees assigned regions.

Supervisor:
  Sees assigned accounts or campaigns.

Agent:
  Sees assigned tickets or campaign scope.

Viewer:
  Sees read-only data within assigned scope.

Admin:
  May see global operational data depending on permissions.
```

## 12.4 Initial Dashboard Widgets

Initial dashboard widgets may include:

```text
Open Tickets
Overdue Tickets
At-Risk Tickets
Escalated Tickets
Tickets by Status
Tickets by Priority
Tickets by Agent
Tickets by Account
Tickets by Campaign
SLA Compliance Summary
Average Resolution Time
```

## 12.5 Dashboard Filters

Recommended filters:

```text
Region
Country
Account
Campaign
Supervisor
Agent
Priority
Status
SLA State
Created Date Range
```

## 12.6 Dashboard Refresh Strategy

For the MVP, the dashboard can use request-based refresh.

```text
User opens dashboard
  → API queries current data
  → Dashboard displays latest available values
```

Future versions may include:

```text
Scheduled refresh
Background aggregation
Real-time updates
SignalR notifications
Power BI integration
```

## 12.7 Operational Dashboard Non-Goals

The operational dashboard does not need to become:

- A full business intelligence platform.
- A Power BI replacement.
- A real-time command center.
- A predictive analytics system.
- A complex executive reporting suite.

Those capabilities may be handled by future phases or external BI tools.

---

# 13. Support Strategy

## 13.1 Purpose

The support strategy defines how issues should be reviewed, diagnosed, prioritized, and resolved.

Even as a portfolio project, OpsSphere should demonstrate supportability.

## 13.2 Support Principles

Support should follow these principles:

- Start with user impact.
- Use logs and audit records to confirm behavior.
- Use correlation IDs when available.
- Separate technical errors from business rule rejections.
- Preserve user trust by returning clear, safe messages.
- Document recurring issues.
- Fix root causes instead of only symptoms.

## 13.3 Support Triage Categories

| Category | Description | Example |
|---|---|---|
| Access Issue | User cannot log in or lacks permission | Agent cannot access ticket queue |
| Workflow Issue | Ticket process does not behave as expected | Ticket cannot be closed |
| Data Issue | Data appears missing or incorrect | Supervisor cannot see assigned ticket |
| SLA Issue | SLA state appears incorrect | Ticket marked breached too early |
| Performance Issue | System is slow or timing out | Dashboard takes too long to load |
| Deployment Issue | Problem after release | API fails after new deployment |
| Integration Issue | External dependency problem | Future email service failure |

## 13.4 Support Severity Levels

| Severity | Description | Example |
|---|---|---|
| Sev 1 | Critical system outage or major security issue | Users cannot log in |
| Sev 2 | Core workflow broken | Ticket creation fails |
| Sev 3 | Important but limited issue | Dashboard filter incorrect |
| Sev 4 | Minor issue or cosmetic problem | Text label typo |

## 13.5 Support Investigation Flow

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

## 13.6 Support Information to Collect

When investigating an issue, collect:

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

Sensitive data should not be collected unless strictly necessary.

---

# 14. Troubleshooting Scenarios

## 14.1 User Cannot Log In

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

Relevant signals:

```text
Authentication failure logs
User status
Health check
API error response
```

## 14.2 User Cannot See Expected Tickets

Check:

```text
Does the user have the correct role?
Does the user have assigned operational scope?
Does the ticket belong to the user's scope?
Is the ticket active or filtered out?
Is the dashboard or ticket list applying filters?
```

Relevant signals:

```text
Scope access logs
Ticket query filters
User scope records
Audit history
```

## 14.3 Ticket Creation Fails

Check:

```text
Are required fields present?
Is the user authorized to create tickets?
Is the selected account or campaign inside user scope?
Does the customer exist?
Is the database available?
Was a business rule violated?
```

Relevant signals:

```text
Validation errors
Authorization logs
Correlation ID
API response
Database logs
```

## 14.4 Ticket Cannot Be Closed

Check:

```text
Is the ticket resolved?
Is the user authorized to close it?
Is the ticket inside user scope?
Is the ticket already closed?
Does workflow allow the transition?
```

Relevant signals:

```text
Business rule error
Ticket status history
Audit logs
Workflow validation logs
```

## 14.5 SLA Dashboard Looks Incorrect

Check:

```text
Are SLA states calculated correctly?
Are filters applied correctly?
Does user scope restrict visible data?
Are background SLA checks running if implemented?
Are there old test records affecting counts?
```

Relevant signals:

```text
SLA state records
Dashboard query logs
Background job logs
Audit records
```

## 14.6 Application Is Slow

Check:

```text
Which endpoint is slow?
Is the database query slow?
Is the dashboard loading too much data?
Are indexes missing?
Did slowness start after a deployment?
```

Relevant signals:

```text
Request duration metrics
Database dependency telemetry
Application Insights performance data
Logs with elapsed milliseconds
```

---

# 15. Support Knowledge Base

## 15.1 Purpose

A support knowledge base helps document recurring issues and standard resolutions.

For OpsSphere, this may start as a simple Markdown document or folder.

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

## 15.2 Common Support Article Format

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

---

# 16. Incident Review

## 16.1 Purpose

Incident review helps prevent repeated failures.

An incident review should be created for major production or staging issues.

## 16.2 Incident Review Template

```text
# Incident Review

## Summary

Briefly describe what happened.

## Impact

Who or what was affected?

## Timeline

When was the issue detected?
When was it mitigated?
When was it resolved?

## Root Cause

What caused the issue?

## Detection

How was it detected?

## Resolution

What fixed the issue?

## Preventive Actions

What will prevent this from happening again?

## Related Logs / Metrics

List relevant correlation IDs, logs, or metrics.

## Related Issue / PR

Link related work.
```

---

# 17. Alerting Strategy

## 17.1 Purpose

Alerts notify maintainers when important failures occur.

The MVP may not require full alert automation, but the system should be designed to support it.

## 17.2 Initial Alert Candidates

Future alerting may be configured for:

```text
API is unavailable
Health check is unhealthy
Error rate exceeds threshold
Request duration exceeds threshold
Database connection failures
Authentication failures spike
SLA background job fails
Overdue tickets exceed threshold
Escalated tickets exceed threshold
```

## 17.3 Alert Severity

| Alert | Severity |
|---|---|
| API unavailable | High |
| Database unavailable | High |
| Login failures spike | Medium or High |
| Ticket creation failures | High |
| SLA job failure | Medium |
| Dashboard slow response | Medium |
| Overdue ticket spike | Medium |
| Single validation error | Low or no alert |

## 17.4 Alerting Non-Goals for MVP

The MVP does not require:

- Complex on-call rotation.
- PagerDuty integration.
- Multi-region alerting.
- Advanced anomaly detection.
- Business-hours alert routing.

---

# 18. Data Privacy and Logging Safety

## 18.1 Purpose

Observability must not create a data exposure risk.

## 18.2 Logging Safety Rules

OpsSphere should follow these rules:

```text
Do not log passwords.
Do not log JWT tokens.
Do not log secrets.
Do not log full connection strings.
Avoid logging full customer descriptions unless required.
Avoid logging sensitive personal data.
Use identifiers instead of full personal data where possible.
Sanitize exception details in user-facing responses.
```

## 18.3 Audit Safety Rules

Audit logs should capture enough information to preserve traceability without storing unnecessary sensitive data.

Recommended approach:

```text
Store entity IDs.
Store safe display names when needed.
Store previous and new values for business-critical fields.
Avoid storing full confidential text in audit records unless required.
```

---

# 19. Observability Implementation Backlog

The following backlog should guide future implementation.

## Structured Logging

- Add Serilog.
- Add request logging middleware.
- Add correlation ID middleware.
- Add structured log fields.
- Add environment and version enrichers.
- Add safe exception logging.

## Health Checks

- Add `/health` endpoint.
- Add database health check.
- Add configuration health check.
- Add optional `/health/details` endpoint.
- Add deployment smoke check.

## Error Handling

- Add global exception handling middleware.
- Add consistent API error response model.
- Add validation error response model.
- Add business rule error response model.
- Add correlation ID to error responses.

## Audit Logs

- Add audit log entity.
- Add audit log persistence.
- Add audit event creation for ticket lifecycle.
- Add audit event creation for user and role changes.
- Add audit event creation for scope changes.
- Add audit query endpoints.
- Add audit UI views.

## Metrics

- Add request duration metrics.
- Add error rate metrics.
- Add ticket count metrics.
- Add SLA state metrics.
- Add escalation metrics.
- Add dashboard query performance tracking.

## Application Insights

- Add Application Insights package.
- Configure telemetry connection string.
- Forward structured logs.
- Track exceptions.
- Track dependencies.
- Add environment and version tags.

## Operational Dashboard

- Add dashboard query endpoints.
- Add dashboard frontend widgets.
- Add role and scope filtering.
- Add SLA summary widget.
- Add escalation summary widget.
- Add workload summary widget.

## Support Documentation

- Add troubleshooting guide.
- Add incident review template.
- Add deployment validation checklist.
- Add common support scenarios.

---

# 20. Definition of Done for Observability

A feature should be considered supportable when:

```text
Important failures are logged.
Unexpected errors are handled safely.
Critical business changes are audited.
Authorization failures are diagnosable.
Scope-related rejections are diagnosable.
Health checks remain valid.
Metrics are considered for high-value workflows.
Support can identify what happened without guessing.
```

For critical workflows such as ticket creation, assignment, escalation, resolution, and closure, observability should include:

```text
Structured logs
Audit records
Safe error responses
Correlation ID support
Test coverage for expected audit behavior
```

---

# 21. Observability and Support Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Logs are unstructured | Troubleshooting becomes slow | Use Serilog structured logging |
| Sensitive data is logged | Security and privacy risk | Sanitize logs and avoid secrets |
| No correlation ID | Hard to trace requests | Add correlation ID middleware |
| Health checks are missing | Deployment issues go unnoticed | Add `/health` endpoint |
| Errors expose stack traces | Security and user trust risk | Use global error handling |
| Audit logs are incomplete | Loss of business traceability | Audit critical actions |
| Dashboard ignores scope | Data exposure risk | Enforce scope filtering in backend |
| Metrics are too limited | Operational issues are hard to detect | Track technical and business metrics |
| Application Insights added too late | Cloud troubleshooting is weak | Design telemetry readiness early |
| Support process is undefined | Issues are handled inconsistently | Document triage and troubleshooting flow |

---

# 22. Summary

The OpsSphere observability and support strategy ensures that the platform can be operated and maintained responsibly.

The system should not only implement business workflows. It should also make those workflows visible, traceable, diagnosable, and supportable.

The initial observability foundation includes:

- Structured logging with Serilog.
- Correlation IDs.
- Health checks.
- Consistent error handling.
- Business audit logs.
- Technical and operational metrics.
- Application Insights readiness.
- Operational dashboards.
- Support and troubleshooting practices.

This strategy demonstrates that OpsSphere is planned as an enterprise-grade application that can be monitored, supported, and improved after deployment.