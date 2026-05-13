# OpsSphere Business Rules Context

## Purpose

This document provides compressed business rules context for agents working inside OpsSphere.

Use this file when the task touches:

- Authentication
- Authorization
- Scope visibility
- Ticket workflows
- SLA behavior
- Escalation
- Comments
- Audit history
- Dashboards
- Reporting
- User management
- Organizational structure

---

## General Platform Rules

```text
All protected OpsSphere features require authentication.
The MVP is intended for internal operational users only.
Customers must not directly access OpsSphere in the MVP.
A user must have an assigned role and operational scope before accessing protected business data.
Users must receive least privilege.
Operational records must be connected to the correct business context.
The system must not expose operational data outside the user's assigned role or scope.
The MVP must prioritize clear operational workflows over advanced automation.
```

---

## Authentication Rules

```text
Only active users may authenticate.
Deactivated users must not authenticate or access protected features.
Invalid credentials must be rejected.
Login errors should not reveal whether the email, password, account status, or role caused the failure.
Protected API endpoints must require a valid token.
Expired tokens must not allow access.
Authentication must be verified before authorization.
```

---

## Authorization Rules

OpsSphere authorization depends on:

```text
Role
+ Permission
+ Operational Scope
```

General rules:

```text
Every internal user must have at least one valid role.
Admin may manage platform structure, users, roles, permissions, and operational assignments.
Operations Manager may view operational data within assigned regional scope.
Supervisor may manage tickets within assigned account or campaign scope.
Agent may work only on tickets within assigned operational scope.
Viewer may only access read-only operational information.
Backend services must enforce authorization independently from frontend visibility.
Frontend may hide unauthorized actions, but hidden UI is not security.
```

---

## Authorization Decision Model

Every protected action should follow this decision model:

```text
1. Is the user authenticated?
2. Is the user active?
3. Does the user have the required role?
4. Does the user have the required permission?
5. Does the user have scope over the target resource?
6. Is the resource in a state that allows the action?
7. Is the action audit-sensitive?
8. If all checks pass, allow the action.
```

---

## Scope-Based Visibility Rules

```text
Users must have assigned operational scope to access operational data.
Managers may view records associated with assigned regions.
Supervisors may view and manage records associated with assigned accounts or campaigns.
Agents may view and work only on assigned tickets or tickets within assigned campaign scope.
Viewers may view only read-only data within assigned scope.
Users must not access records outside assigned operational scope.
Ticket lists, dashboards, SLA views, customer views, and audit views must be filtered by operational scope.
Create, update, assignment, escalation, resolution, and closure actions must validate operational scope before saving changes.
```

---

## Organizational Structure Rules

```text
A country must belong to one region.
A country cannot exist without an associated region.
An account may contain one or more campaigns.
A campaign must belong to one account.
A campaign must be associated with an operating country.
A manager may oversee one or more regions.
A supervisor must be assigned to an account before managing tickets for that account.
A supervisor may oversee one or more campaigns within an assigned account.
An agent must be assigned to an account or campaign before handling tickets.
A viewer must be assigned a read-only operational scope.
Only Admin users may create, update, deactivate, or assign regions, countries, accounts, campaigns, users, roles, permissions, and scopes.
Operational structure records should be deactivated instead of physically deleted when historical records depend on them.
Historical tickets, audit records, and dashboard data should preserve original business context even if organizational records are later deactivated.
```

---

## User Management Rules

```text
Only Admin users may create, update, deactivate, or manage users.
A user must have a valid role before accessing protected business modules.
Operational users must have assigned scope before viewing operational data.
Only active users may log in.
Deactivated users must be blocked from authentication and protected features.
Deactivating a user must not remove historical tickets, assignments, comments, or audit records.
Any role assignment or role change must be recorded in audit history.
Any operational scope assignment or scope change must be recorded in audit history.
User deactivation must be recorded in audit history.
If user reactivation is supported, only Admin users may reactivate users.
User identity should remain consistent across tickets, comments, assignments, and audit history.
```

---

## Customer Rules

```text
A ticket must be linked to a customer.
Customer records must only be visible to users with authorized operational scope.
Authorized users may view customer ticket history only within their permitted scope.
Customers must not directly access OpsSphere in the MVP.
Customer data used in tickets should remain consistent for operational traceability.
Only authorized users may update customer records.
Customer records linked to tickets should not be physically deleted if required for historical traceability.
```

---

## Ticket Creation Rules

```text
A ticket must be linked to a customer.
A ticket must be linked to an account.
A ticket must be linked to a campaign.
A ticket must have a category.
A ticket must have a priority.
A ticket must have a description.
A ticket must have a creator.
New tickets must start as Open unless assignment rules set them directly to Assigned.
Ticket creation must initialize or associate SLA information.
Ticket creation must be recorded in audit history.
Ticket visibility must respect role and operational scope.
Users cannot create tickets outside their operational scope.
```

---

## Ticket Workflow Rules

Initial statuses:

```text
Open
Assigned
In Progress
Waiting for Customer
Escalated
Resolved
Closed
```

Workflow rules:

```text
Ticket status changes must follow valid workflow transitions.
Invalid transitions must be rejected.
A ticket must be resolved before it can be closed.
Closed tickets cannot be modified unless reopened by an authorized supervisor or Admin.
Status changes must be recorded in ticket history.
Status changes must be recorded in audit history.
```

---

## Assignment Rules

```text
A ticket can only be assigned to an eligible active agent.
The target agent must belong to the correct account or campaign scope.
Supervisors can only assign tickets within their operational scope.
Closed tickets cannot be assigned unless a reopening workflow is allowed.
Assignment and reassignment must be recorded in ticket history.
Assignment and reassignment must be recorded in audit history.
Assignment should update ticket status when appropriate.
```

---

## Internal Comment Rules

```text
Internal comments must be linked to a ticket.
Internal comments must have an author.
Internal comments must have a timestamp.
Internal comments cannot be empty.
Internal comments must respect role and operational scope.
Internal comments should support collaboration between agents, supervisors, and managers.
Internal comment creation must be recorded when required by audit rules.
```

---

## Escalation Rules

```text
Escalated tickets require an escalation reason.
Closed tickets cannot be escalated.
Escalation must validate user role, permission, and operational scope.
Escalation must update ticket status to Escalated when appropriate.
Escalations must appear in supervisor escalation queues.
Escalation events must be recorded in ticket history.
Escalation events must be recorded in audit history.
```

---

## Resolution and Closure Rules

```text
A ticket should be resolved when the customer issue has been handled.
Resolution should preserve final SLA outcome.
Resolution must be recorded in audit history.
A ticket must be resolved before closure.
Closure must be recorded in audit history.
Closed tickets preserve final operational history.
Closed tickets cannot be edited unless reopening is explicitly supported and authorized.
```

---

## SLA Rules

```text
Each ticket must have an SLA target.
SLA timers start when the ticket is created.
SLA state must be visible to authorized users.
Tickets approaching breach should be marked At Risk.
Tickets exceeding the SLA target must be marked Breached.
Completed tickets should preserve their final SLA outcome.
SLA dashboard data must respect role and operational scope.
```

MVP simplicity boundary:

```text
Do not add advanced SLA calendars, pause rules, business-hour calculations, or predictive SLA modeling unless explicitly requested.
```

---

## Dashboard Rules

```text
Dashboards must respect role and operational scope.
Dashboards must not expose records outside user scope.
Dashboard metrics must be based on consistent filters.
Dashboard data should support daily operational visibility.
Dashboard data should not attempt to replace Power BI.
```

---

## Reporting Rules

```text
OpsSphere should produce clean, structured operational data.
Reporting-ready data should preserve region, country, account, campaign, supervisor, agent, ticket, SLA, and audit context.
Basic CSV export or reporting-ready views may be included.
Advanced business intelligence belongs outside the core MVP.
Power BI may consume exported or reporting-ready data in future phases.
```

---

## Audit Rules

Audit records are required for critical actions.

Examples:

```text
Ticket created
Ticket assigned
Ticket reassigned
Ticket status changed
Ticket priority changed
Ticket escalated
Ticket resolved
Ticket closed
Internal comment added
User created
User updated
User deactivated
Role assigned
Role removed
Scope assigned
Scope updated
Region created
Country created
Account created
Campaign created
Permission changed
```

Audit records should capture:

```text
Actor
Action
Entity type
Entity id
Timestamp
Previous value when applicable
New value when applicable
Reason when applicable
Correlation ID when useful
```

---

## Out-of-Scope Rules

Do not add these to MVP unless explicitly requested:

```text
Advanced Power BI dashboards
AI ticket classification
Predictive SLA modeling
Telephony integration
Call recording
Omnichannel integrations
Workforce scheduling
Payroll
HR management
External customer portal
Real-time chat
Mobile app
Complex notification engine
Enterprise SSO
Multi-tenant billing
```