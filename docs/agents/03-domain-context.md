# OpsSphere Domain Context

## Purpose

This document provides compressed domain context for agents working inside OpsSphere.

Use this file when the task touches:

- Entities
- Business concepts
- Use cases
- Ticket workflow
- SLA behavior
- Escalations
- Audit history
- Operational hierarchy
- Role and scope behavior
- Database modeling
- API workflows

---

## Domain Identity

OpsSphere models a multinational BPO/contact center operation.

It is not a generic ticketing system.

The domain connects:

```text
Organizational structure
Operational users
Customers
Tickets
SLA tracking
Assignment
Escalation
Internal collaboration
Resolution
Closure
Audit history
Dashboards
Reporting-ready data
```

---

## Operational Hierarchy

The main hierarchy is:

```text
Region
  → Country
    → Account
      → Campaign
        → Supervisor
          → Agent
```

Tickets exist inside this operational context.

---

## Core Domain Concepts

Initial domain concepts:

```text
Region
Country
Account
Campaign
User
Role
Permission
Scope
Manager
Supervisor
Agent
Viewer
Customer
Ticket
TicketStatus
TicketPriority
SLA
SLAState
Escalation
Queue
Comment
Resolution
AuditTrail
Dashboard
ReportingView
```

---

## Main Relationships

```text
Region
  contains Countries

Country
  belongs to Region
  contains Accounts

Account
  belongs to Country
  contains Campaigns
  has Tickets
  may have Supervisors
  may have Agents

Campaign
  belongs to Account
  may belong to an operating Country
  has Tickets
  has assigned Agents
  may have assigned Supervisors

User
  has Role
  has Permission through Role
  has Operational Scope
  may act as Admin, Operations Manager, Supervisor, Agent, or Viewer

Customer
  has Tickets
  does not directly access OpsSphere in the MVP

Ticket
  belongs to Customer
  belongs to Account
  belongs to Campaign
  has Region/Country context through operational structure
  has Creator
  may have Assigned Agent
  may have Supervisor visibility
  has Priority
  has Status
  has SLA
  has Comments
  may have Escalations
  may have Resolution
  has Audit Trail records

AuditTrail
  records critical business actions
  references actor, action, timestamp, entity, previous value, and new value where applicable
```

---

## User and Access Concepts

## User

A User is an internal person who can authenticate into OpsSphere.

Users include:

```text
Admin
Operations Manager
Supervisor
Agent
Viewer
```

Customers are not system users in the MVP.

Rules:

```text
Only active users may authenticate.
Deactivated users cannot access protected features.
Users must have a valid role.
Operational users must have assigned scope.
User management changes must be audited.
```

---

## Role

Initial runtime roles:

```text
Admin
Operations Manager
Supervisor
Agent
Viewer
```

Rules:

```text
Every internal user must have at least one valid role.
Admin manages platform structure.
Operations Manager oversees assigned regions.
Supervisor manages assigned accounts or campaigns.
Agent works within assigned scope.
Viewer has read-only access.
Technical Owner is not a runtime business role.
Customer is not a runtime user in the MVP.
```

---

## Permission

Permissions define granular capabilities.

Examples:

```text
users.manage
roles.manage
organization.manage
tickets.create
tickets.assign
tickets.update_status
tickets.comment
tickets.escalate
tickets.resolve
tickets.close
sla.view
dashboard.view
reports.view
audit.view
```

Rules:

```text
Permissions must be enforced by backend authorization.
Users must not perform actions outside assigned permissions.
Critical permissions should have tests.
```

---

## Scope

Scope defines operational visibility boundaries.

Initial scope types:

```text
Region
Country
Account
Campaign
AssignedTickets
```

Rules:

```text
Users may only access operational data within assigned scope.
Managers view records within assigned regions.
Supervisors manage tickets within assigned accounts or campaigns.
Agents work on assigned tickets or tickets within assigned campaign scope.
Viewers access read-only data within assigned scope.
Ticket lists, dashboards, SLA views, customer views, and audit views must be filtered by scope.
Write actions must validate scope before saving changes.
```

---

## Ticket Domain

A Ticket is the main operational support case.

A ticket must be linked to:

```text
Customer
Account
Campaign
Creator
Priority
Status
SLA target or SLA state
```

A ticket may have:

```text
Assigned Agent
Supervisor visibility
Comments
Escalations
Resolution
Audit records
```

---

## Ticket Statuses

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

Rules:

```text
New tickets start as Open unless assignment rules set them directly to Assigned.
Status transitions must be valid.
A ticket must be resolved before it can be closed.
Closed tickets cannot be modified unless reopened by an authorized role.
Status changes must be recorded in ticket history and audit history.
```

---

## Ticket Priority

Ticket priority represents operational urgency.

Initial priority values may include:

```text
Low
Normal
High
Critical
```

Rules:

```text
Every ticket must have a priority.
Priority may affect SLA target.
Priority changes must be audited.
```

---

## Assignment Rules

Rules:

```text
A ticket can only be assigned to an eligible active agent.
The assigned agent must belong to the correct account or campaign scope.
Supervisors can only assign tickets within their operational scope.
Closed tickets cannot be assigned unless a reopening workflow exists.
Assignment and reassignment must be recorded in audit history.
Assignment may update ticket status to Assigned when appropriate.
```

---

## Internal Comment Rules

Rules:

```text
Internal comments are linked to a ticket.
Internal comments have an author and timestamp.
Internal comments cannot be empty.
Internal comments are visible only to authorized users within scope.
Internal comment creation must be audited when required.
```

---

## Escalation Rules

Rules:

```text
Escalated tickets require an escalation reason.
Closed tickets cannot be escalated.
Escalations must be visible to authorized supervisors.
Escalation events must be recorded in ticket history and audit history.
Escalated tickets should appear in supervisor escalation queues.
```

---

## Resolution and Closure Rules

Resolution rules:

```text
A ticket must include resolution information before closure.
Resolution must preserve SLA outcome.
Resolution must be audited.
```

Closure rules:

```text
A ticket must be resolved before it can be closed.
Closed tickets cannot be modified unless reopened by an authorized supervisor or Admin.
Closure must be audited.
```

---

## SLA Domain

SLA tracking helps agents, supervisors, managers, and viewers identify operational risk.

SLA states:

```text
Within SLA
At Risk
Breached
Completed
```

Rules:

```text
Each ticket must have an SLA target.
SLA tracking starts when the ticket is created.
Tickets approaching breach should be marked At Risk.
Tickets exceeding the target should be marked Breached.
Completed tickets preserve final SLA outcome.
SLA dashboard data must respect role and scope.
```

MVP boundary:

```text
Keep SLA simple in MVP.
Use target due date, SLA state, at-risk, breached, and completed.
Defer business-hour calendars, pause rules, and predictive SLA modeling.
```

---

## Audit Domain

Audit history preserves traceability.

Audit records should capture:

```text
Who performed the action
When it happened
What entity was affected
What changed
Previous value when applicable
New value when applicable
Correlation ID when useful
```

Audit-sensitive actions include:

```text
Ticket creation
Ticket assignment
Ticket reassignment
Ticket status change
Ticket priority change
Ticket escalation
Ticket resolution
Ticket closure
Internal comment creation
User creation
User update
User deactivation
Role assignment
Role change
Scope assignment
Scope change
Organization structure changes
Permission changes
```

---

## Dashboard and Reporting Domain

Dashboards provide operational visibility.

Initial dashboard metrics:

```text
Open tickets
Overdue tickets
Tickets by status
Tickets by priority
Tickets by account
Tickets by campaign
Tickets by agent
Tickets by supervisor
SLA status overview
Escalated tickets
Ticket aging
```

Rules:

```text
Dashboard data must respect role and scope.
Dashboard metrics must be clearly defined.
Reporting-ready data must preserve operational context.
OpsSphere does not replace Power BI.
```

---

## Domain Modeling Principle

Use this principle when modeling entities:

```text
Model the business operation, not only database tables.
```

Avoid:

```text
Overengineering.
Premature abstractions.
Advanced AI behavior.
Telephony concepts.
Payroll/HR/workforce management concepts.
Customer portal concepts.
Full BI concepts.
```