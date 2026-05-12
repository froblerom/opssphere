# Business Rules

## Document Information

| Field | Value |
|---|---|
| Project | OpsSphere |
| Document | Business Rules |
| File | `docs/09-business-rules.md` |
| Version | 1.0 |
| Status | Draft |
| Project Type | Enterprise Support Operations Platform |
| Business Context | Multinational BPO / Contact Center Operations |

---

## 1. Purpose

This document defines the business rules that govern the behavior of OpsSphere.

Business rules describe the operational constraints, validations, permissions, workflow restrictions, and organizational policies that the system must enforce.

In enterprise systems, the value of the application is not only in creating, reading, updating, and deleting records. The real value is in enforcing the rules that make the operation consistent, auditable, secure, and aligned with business expectations.

This document serves as a reference for:

- Domain modeling.
- Use case validation.
- Backend business logic.
- API validation.
- Authorization rules.
- Workflow enforcement.
- Audit trail requirements.
- Testing scenarios.
- Acceptance criteria.
- Future governance decisions.

---

## 2. Scope

This document covers business rules for the initial version of OpsSphere.

The rules are organized into the following areas:

- General platform rules.
- Authentication and access rules.
- Role-based access rules.
- Scope-based visibility rules.
- Organizational structure rules.
- User management rules.
- Customer rules.
- Ticket rules.
- Ticket workflow rules.
- Assignment rules.
- SLA rules.
- Escalation rules.
- Internal comment rules.
- Audit history rules.
- Dashboard rules.
- Reporting-ready data rules.
- Out-of-scope boundaries.

---

## 3. Business Rule Categories

| Category | Description |
|---|---|
| General Platform Rules | Rules that apply across the entire system. |
| Authentication Rules | Rules related to login, sessions, and protected access. |
| Authorization Rules | Rules related to roles, permissions, and operational scope. |
| Organizational Rules | Rules that define the BPO/contact center structure. |
| User Management Rules | Rules for creating, updating, deactivating, and assigning users. |
| Customer Rules | Rules related to customer records and ticket linkage. |
| Ticket Rules | Rules related to ticket creation, ownership, priority, and lifecycle. |
| Workflow Rules | Rules that control valid ticket status transitions. |
| Assignment Rules | Rules that control ticket ownership and reassignment. |
| SLA Rules | Rules that control SLA targets, SLA states, and breach visibility. |
| Escalation Rules | Rules related to ticket escalation and supervisor visibility. |
| Comment Rules | Rules related to internal collaboration on tickets. |
| Audit Rules | Rules that guarantee traceability of critical business actions. |
| Dashboard Rules | Rules that control operational dashboard visibility. |
| Reporting Rules | Rules that preserve structured data for external reporting. |

---

# 4. General Platform Rules

## BR-GEN-001: Protected System Access

All protected OpsSphere features must require user authentication.

## BR-GEN-002: Internal User System

The initial version of OpsSphere is intended for internal operational users only.

## BR-GEN-003: Customer Access Exclusion

Customers must not directly access OpsSphere in the initial version.

## BR-GEN-004: Role and Scope Required

A user must have an assigned role and operational scope before accessing protected business data.

## BR-GEN-005: Least Privilege

Users must only receive the minimum permissions required to perform their responsibilities.

## BR-GEN-006: Business Context Required

Operational records must be connected to the correct business context whenever applicable.

Business context may include:

- Region.
- Country.
- Account.
- Campaign.
- Supervisor.
- Agent.
- Customer.

## BR-GEN-007: No Unauthorized Data Exposure

The system must not expose operational data outside the user's assigned role or scope.

## BR-GEN-008: MVP Simplicity Boundary

The initial version must prioritize clear operational workflows over advanced automation or complex enterprise integrations.

---

# 5. Authentication and Session Rules

## BR-AUTH-001: Active User Authentication

Only active users may authenticate into OpsSphere.

## BR-AUTH-002: Deactivated User Restriction

Deactivated users must not be able to authenticate or access protected features.

## BR-AUTH-003: Invalid Credentials Rejection

The system must reject authentication attempts using invalid credentials.

## BR-AUTH-004: Generic Login Error

The system should not reveal whether a failed login was caused by an invalid email, invalid password, inactive account, or missing role.

## BR-AUTH-005: Token Required

Protected API endpoints must require a valid authentication token.

## BR-AUTH-006: Expired Token Rejection

Expired authentication tokens must not allow access to protected features.

## BR-AUTH-007: Session Expiration

User sessions must expire according to configured token expiration rules.

## BR-AUTH-008: Authentication Before Authorization

The system must verify authentication before applying authorization rules.

---

# 6. Role-Based Access Rules

## BR-RBAC-001: Role Required

Every internal user must have at least one valid role before accessing business modules.

## BR-RBAC-002: Admin Platform Control

Admin users may manage the platform structure, users, roles, permissions, and operational assignments.

## BR-RBAC-003: Operations Manager Oversight

Operations Managers may view operational data within their assigned regional scope.

## BR-RBAC-004: Supervisor Operational Control

Supervisors may manage tickets within assigned accounts or campaigns.

## BR-RBAC-005: Agent Ticket Handling

Agents may work only on tickets within their assigned operational scope.

## BR-RBAC-006: Viewer Read-Only Access

Viewers may only access read-only operational information.

## BR-RBAC-007: Viewer Modification Restriction

Viewers must not create, update, assign, escalate, resolve, close, or delete operational records.

## BR-RBAC-008: Technical Owner Runtime Exclusion

Technical Owner is a project implementation role and should not be treated as a normal runtime business role in the initial system.

## BR-RBAC-009: Role-Based Feature Visibility

The user interface should only display actions that the current user is allowed to perform.

## BR-RBAC-010: Backend Authorization Enforcement

Backend services must enforce authorization independently from frontend visibility.

---

# 7. Scope-Based Visibility Rules

## BR-SCOPE-001: Operational Scope Required

Users must have an assigned operational scope to access operational data.

## BR-SCOPE-002: Manager Regional Scope

Managers may view records associated with their assigned regions.

## BR-SCOPE-003: Supervisor Account Scope

Supervisors may view and manage records associated with their assigned accounts.

## BR-SCOPE-004: Supervisor Campaign Scope

Supervisors may view and manage records associated with their assigned campaigns.

## BR-SCOPE-005: Agent Scope

Agents may view and work only on tickets assigned to them or tickets within their assigned campaign scope.

## BR-SCOPE-006: Viewer Scope

Viewers may view only read-only data within their assigned scope.

## BR-SCOPE-007: Cross-Scope Access Denial

Users must not access records outside their assigned operational scope.

## BR-SCOPE-008: Scope Filtering Required

Ticket lists, dashboards, SLA views, customer views, and audit views must be filtered by the user's operational scope.

## BR-SCOPE-009: Scope Validation on Write Actions

Create, update, assignment, escalation, resolution, and closure actions must validate operational scope before changes are saved.

---

# 8. Organizational Structure Rules

## BR-ORG-001: Region Ownership

A country must belong to one region.

## BR-ORG-002: Country Under Region

A country cannot exist without an associated region.

## BR-ORG-003: Account Structure

An account may contain one or more campaigns.

## BR-ORG-004: Campaign Ownership

A campaign must belong to one account.

## BR-ORG-005: Campaign Country Association

A campaign must be associated with an operating country.

## BR-ORG-006: Manager Region Assignment

A manager may oversee one or more regions.

## BR-ORG-007: Supervisor Account Assignment

A supervisor must be assigned to an account before managing tickets for that account.

## BR-ORG-008: Supervisor Campaign Assignment

A supervisor may oversee one or more campaigns within an assigned account.

## BR-ORG-009: Agent Assignment

An agent must be assigned to an account or campaign before handling tickets.

## BR-ORG-010: Viewer Scope Assignment

A viewer must be assigned a read-only operational scope.

## BR-ORG-011: Admin-Only Structural Changes

Only Admin users may create, update, deactivate, or assign regions, countries, accounts, campaigns, users, roles, permissions, and scopes.

## BR-ORG-012: Deactivation Instead of Deletion

Operational structure records should be deactivated instead of physically deleted when historical records depend on them.

## BR-ORG-013: Historical Context Preservation

Historical tickets, audit records, and dashboard data should preserve their original business context even if organizational records are later deactivated.

---

# 9. User Management Rules

## BR-USER-001: Admin User Management

Only Admin users may create, update, deactivate, or manage users.

## BR-USER-002: Required User Role

A user must have a valid role before being allowed to access protected business modules.

## BR-USER-003: Required Scope for Operational Users

Operational users must have an assigned scope before viewing operational data.

Operational users include:

- Operations Managers.
- Supervisors.
- Agents.
- Viewers.

## BR-USER-004: Active User Required for Login

Only active users may log in.

## BR-USER-005: Deactivated User Access Block

Deactivated users must be blocked from authentication and protected features.

## BR-USER-006: User Deactivation Preserves History

Deactivating a user must not remove historical tickets, assignments, comments, or audit records associated with that user.

## BR-USER-007: Role Change Audit

Any role assignment or role change must be recorded in audit history.

## BR-USER-008: Scope Change Audit

Any operational scope assignment or scope change must be recorded in audit history.

## BR-USER-009: User Deactivation Audit

User deactivation must be recorded in audit history.

## BR-USER-010: User Reactivation Control

If user reactivation is supported, only Admin users may reactivate users.

## BR-USER-011: User Identity Consistency

A user identity should remain consistent across tickets, comments, assignments, and audit history.

---

# 10. Customer Rules

## BR-CUST-001: Customer Required for Ticket

A ticket must be linked to a customer.

## BR-CUST-002: Customer Record Visibility

Customer records must only be visible to users with authorized operational scope.

## BR-CUST-003: Customer Ticket History

Authorized users may view ticket history associated with a customer only within their permitted scope.

## BR-CUST-004: Customer Access Restriction

Customers must not directly access the initial version of OpsSphere.

## BR-CUST-005: Customer Data Consistency

Customer data used in tickets should remain consistent for operational traceability.

## BR-CUST-006: Customer Update Authorization

Only authorized users may update customer records.

## BR-CUST-007: Customer Deletion Restriction

Customer records linked to tickets should not be physically deleted if they are required for historical traceability.

---

# 11. Ticket Creation Rules

## BR-TICKET-001: Ticket Customer Link

A ticket must be linked to a customer.

## BR-TICKET-002: Ticket Account Link

A ticket must be linked to an account.

## BR-TICKET-003: Ticket Campaign Link

A ticket must be linked to a campaign.

## BR-TICKET-004: Ticket Required Fields

A ticket must include the required fields before it can be created.

Required fields include:

- Customer.
- Account.
- Campaign.
- Category.
- Priority.
- Description.
- Created by user.

## BR-TICKET-005: Ticket Creator Required

Every ticket must identify the user who created it.

## BR-TICKET-006: Initial Ticket Status

New tickets must start with status `Open` unless assignment rules set them directly to `Assigned`.

## BR-TICKET-007: Ticket Operational Context

A ticket should preserve the operational context available at creation time.

Operational context may include:

- Region.
- Country.
- Account.
- Campaign.
- Customer.
- Assigned agent.
- Supervisor.
- Priority.
- Status.
- SLA state.

## BR-TICKET-008: Ticket Creation Scope Validation

A user must not create a ticket for an account or campaign outside their authorized scope.

## BR-TICKET-009: Ticket Number Required

Each ticket should have a unique ticket identifier or ticket number.

## BR-TICKET-010: Ticket Creation Audit

Ticket creation must be recorded in audit history.

---

# 12. Ticket Ownership and Assignment Rules

## BR-ASSIGN-001: Assignment Eligibility

A ticket can only be assigned to an eligible active agent.

## BR-ASSIGN-002: Agent Scope Match

A ticket can only be assigned to an agent within the correct account or campaign scope.

## BR-ASSIGN-003: Supervisor Scope Match

A supervisor can only assign or reassign tickets within their assigned operational scope.

## BR-ASSIGN-004: Assigned Owner Required for Active Work

A ticket must have an assigned owner before it can be considered actively worked.

## BR-ASSIGN-005: Assignment Status Update

When a ticket is assigned, the ticket status should become `Assigned` if it is currently `Open`.

## BR-ASSIGN-006: Reassignment Allowed by Supervisor

Supervisors may reassign tickets within their assigned scope.

## BR-ASSIGN-007: Closed Ticket Assignment Restriction

Closed tickets cannot be assigned or reassigned unless a reopening workflow is explicitly allowed.

## BR-ASSIGN-008: Assignment Audit

Ticket assignment must be recorded in audit history.

## BR-ASSIGN-009: Reassignment Audit

Ticket reassignment must be recorded in audit history.

## BR-ASSIGN-010: Assignment History Preservation

Ticket assignment history should preserve previous and new assignee information when applicable.

---

# 13. Ticket Workflow Rules

## BR-WF-001: Supported Ticket Statuses

The system must support the following initial ticket statuses:

- `Open`
- `Assigned`
- `In Progress`
- `Waiting for Customer`
- `Escalated`
- `Resolved`
- `Closed`

## BR-WF-002: Valid Status Transitions Required

Ticket status changes must follow valid workflow transitions.

## BR-WF-003: Invalid Status Transition Rejection

The system must reject invalid status transitions.

## BR-WF-004: Open to Assigned

A ticket may move from `Open` to `Assigned` when an eligible agent is assigned.

## BR-WF-005: Assigned to In Progress

A ticket may move from `Assigned` to `In Progress` when work begins.

## BR-WF-006: In Progress to Waiting for Customer

A ticket may move from `In Progress` to `Waiting for Customer` when customer input is required.

## BR-WF-007: Waiting for Customer to In Progress

A ticket may move from `Waiting for Customer` to `In Progress` when the required customer information is received.

## BR-WF-008: In Progress to Escalated

A ticket may move from `In Progress` to `Escalated` when higher-level attention is required.

## BR-WF-009: Assigned to Escalated

A ticket may move from `Assigned` to `Escalated` when the issue requires immediate supervisor attention.

## BR-WF-010: Escalated to In Progress

An escalated ticket may return to `In Progress` after supervisor review or action.

## BR-WF-011: In Progress to Resolved

A ticket may move from `In Progress` to `Resolved` when the issue has been handled.

## BR-WF-012: Escalated to Resolved

An escalated ticket may move to `Resolved` when the escalated issue has been handled.

## BR-WF-013: Resolved to Closed

A ticket may move from `Resolved` to `Closed` after closure confirmation.

## BR-WF-014: Resolution Before Closure

A ticket must be resolved before it can be closed.

## BR-WF-015: Closed Ticket Modification Restriction

Closed tickets cannot be modified unless reopened by an authorized supervisor or Admin.

## BR-WF-016: Reopen Control

If reopening is supported, only authorized Supervisors or Admins may reopen closed tickets.

## BR-WF-017: Status Change Audit

Every ticket status change must be recorded in audit history.

---

# 14. Ticket Priority Rules

## BR-PRIORITY-001: Priority Required

Every ticket must have a priority.

## BR-PRIORITY-002: Supported Priorities

The system should support an initial set of ticket priorities.

Initial priorities may include:

- `Low`
- `Normal`
- `High`
- `Critical`

## BR-PRIORITY-003: Priority Visibility

Ticket priority must be visible in ticket detail, ticket lists, dashboards, and reporting-ready data.

## BR-PRIORITY-004: Priority Change Authorization

Only authorized users may change ticket priority.

## BR-PRIORITY-005: Priority Change Audit

Priority changes must be recorded in audit history.

## BR-PRIORITY-006: High-Priority SLA

A high-priority ticket must have a shorter SLA target than a normal-priority ticket.

## BR-PRIORITY-007: Critical Priority Visibility

Critical tickets should be easy to identify in operational views.

---

# 15. SLA Rules

## BR-SLA-001: SLA Target Required

Each ticket must have an SLA target.

## BR-SLA-002: SLA Timer Start

SLA timers start when the ticket is created.

## BR-SLA-003: SLA State Required

Each ticket must have an SLA state.

## BR-SLA-004: Supported SLA States

The system must support the following initial SLA states:

- `Within SLA`
- `At Risk`
- `Breached`
- `Completed`

## BR-SLA-005: SLA Visibility

SLA state must be visible to authorized users.

## BR-SLA-006: SLA Dashboard Visibility

SLA state must be available in operational dashboard views.

## BR-SLA-007: SLA Filtering

Users must be able to filter tickets by SLA state.

## BR-SLA-008: SLA Breach Identification

Tickets that exceed their SLA target must be marked as `Breached`.

## BR-SLA-009: At-Risk Ticket Identification

Tickets approaching SLA breach should be marked as `At Risk`.

## BR-SLA-010: Completed SLA Preservation

Completed tickets should preserve their final SLA outcome.

## BR-SLA-011: SLA by Priority

SLA targets may vary by ticket priority.

## BR-SLA-012: SLA by Account

SLA targets may vary by account.

## BR-SLA-013: SLA by Campaign

SLA targets may vary by campaign.

## BR-SLA-014: Simple SLA Model

The initial SLA model should remain simple and avoid advanced business-hour calendars, pause rules, or predictive SLA modeling.

## BR-SLA-015: SLA Audit Relevance

Changes that affect SLA state or final SLA outcome should be traceable.

---

# 16. Escalation Rules

## BR-ESC-001: Escalation Reason Required

Escalated tickets must include an escalation reason.

## BR-ESC-002: Closed Ticket Escalation Restriction

Closed tickets cannot be escalated.

## BR-ESC-003: Escalation Visibility

Escalated tickets must be visible in the supervisor queue or equivalent supervisor view.

## BR-ESC-004: Manager Escalation Visibility

Escalated tickets should be visible to authorized managers within scope.

## BR-ESC-005: Escalation Status Update

When a ticket is escalated, the ticket status should become `Escalated` if applicable.

## BR-ESC-006: Escalation Scope Validation

Users may only escalate tickets within their assigned operational scope.

## BR-ESC-007: Duplicate Escalation Handling

The system should prevent duplicate escalation events unless the business explicitly allows multiple escalation records.

## BR-ESC-008: Escalation History

Escalation events must be recorded in ticket history.

## BR-ESC-009: Escalation Audit

Escalation events must be recorded in audit history.

## BR-ESC-010: Escalation Resolution

An escalated ticket should remain visible as escalated until it is reviewed, moved to another valid status, resolved, or closed.

---

# 17. Internal Comment Rules

## BR-COMMENT-001: Internal Comment Authorization

Only authorized internal users may add internal comments to tickets.

## BR-COMMENT-002: Internal Comment Visibility

Internal comments must only be visible to authorized internal users.

## BR-COMMENT-003: Customer Exclusion

Internal comments must not be exposed to customers in the initial version.

## BR-COMMENT-004: Comment Author Required

Each internal comment must identify its author.

## BR-COMMENT-005: Comment Timestamp Required

Each internal comment must include a timestamp.

## BR-COMMENT-006: Empty Comment Restriction

Empty internal comments are not allowed.

## BR-COMMENT-007: Comment Scope Validation

Users may only comment on tickets within their assigned operational scope.

## BR-COMMENT-008: Closed Ticket Comment Restriction

Closed tickets should not accept new internal comments unless the business explicitly allows post-closure notes.

## BR-COMMENT-009: Comment Creation Audit

Internal comment creation must be recorded in audit history.

## BR-COMMENT-010: Comment History Preservation

Internal comments should remain part of ticket history for operational traceability.

---

# 18. Audit History Rules

## BR-AUDIT-001: Critical Action Audit

Critical business actions must be recorded in audit history.

Critical actions include:

- Ticket creation.
- Ticket assignment.
- Ticket reassignment.
- Ticket status change.
- Ticket priority change.
- Ticket escalation.
- Ticket resolution.
- Ticket closure.
- User creation.
- User update.
- User deactivation.
- Role assignment.
- Scope assignment.
- Organizational structure changes.

## BR-AUDIT-002: Audit Actor Required

Each audit record must identify the actor who performed the action.

## BR-AUDIT-003: Audit Timestamp Required

Each audit record must include a timestamp.

## BR-AUDIT-004: Audit Entity Required

Each audit record must identify the affected entity.

## BR-AUDIT-005: Audit Action Required

Each audit record must identify the action performed.

## BR-AUDIT-006: Previous and New Values

Audit records should include previous and new values when applicable.

## BR-AUDIT-007: Audit Scope Visibility

Users may only view audit records within their authorized operational scope.

## BR-AUDIT-008: Admin Audit Visibility

Admin users may view administrative audit records according to platform permissions.

## BR-AUDIT-009: Audit Record Integrity

Audit records should not be editable through normal business workflows.

## BR-AUDIT-010: Audit History Preservation

Audit history should be preserved for traceability and future reporting.

---

# 19. Dashboard Rules

## BR-DASH-001: Dashboard Role Visibility

Dashboard data must respect the user's role.

## BR-DASH-002: Dashboard Scope Filtering

Dashboard data must be filtered by the user's operational scope.

## BR-DASH-003: Open Ticket Metric

Dashboards should display open ticket counts.

## BR-DASH-004: Overdue Ticket Metric

Dashboards should display overdue ticket counts.

## BR-DASH-005: Tickets by Status

Dashboards should display tickets grouped by status.

## BR-DASH-006: Tickets by Priority

Dashboards should display tickets grouped by priority.

## BR-DASH-007: Tickets by Account

Dashboards should display tickets grouped by account when permitted by user scope.

## BR-DASH-008: Tickets by Campaign

Dashboards should display tickets grouped by campaign when permitted by user scope.

## BR-DASH-009: Tickets by Agent

Dashboards should display tickets grouped by agent when permitted by user scope.

## BR-DASH-010: Tickets by Supervisor

Dashboards should display tickets grouped by supervisor when permitted by user scope.

## BR-DASH-011: SLA Overview

Dashboards should display basic SLA overview information.

## BR-DASH-012: Viewer Dashboard Restriction

Viewers may access dashboards only in read-only mode.

## BR-DASH-013: Dashboard BI Boundary

OpsSphere dashboards must provide basic operational visibility and must not attempt to replace Power BI in the initial version.

---

# 20. Reporting-Ready Data Rules

## BR-REP-001: Structured Operational Data

The system must store structured operational data suitable for reporting and analysis.

## BR-REP-002: Consistent Reporting Fields

Tickets should maintain consistent fields for:

- Region.
- Country.
- Account.
- Campaign.
- Supervisor.
- Agent.
- Customer.
- Status.
- Priority.
- SLA state.
- Created timestamp.
- Updated timestamp.
- Resolved timestamp.
- Closed timestamp.

## BR-REP-003: Filtered Operational Views

The system should provide filtered operational views that support manual review and external reporting.

## BR-REP-004: CSV Export Boundary

The system may provide basic CSV export for ticket lists, SLA views, or audit views.

## BR-REP-005: Power BI Boundary

The system shall not attempt to replace Power BI or become a full business intelligence platform.

## BR-REP-006: Historical Reporting Context

Reports and exports should preserve the business context of historical records.

## BR-REP-007: Role and Scope Reporting Restrictions

Reporting views must respect role and operational scope restrictions.

---

# 21. Out-of-Scope Business Rules

The following rules are intentionally outside the initial version of OpsSphere:

## BR-OOS-001: No Customer Portal Rules

The initial version does not include customer portal login, self-service ticket creation, or customer-facing ticket tracking.

## BR-OOS-002: No Advanced Power BI Authoring

The initial version does not include advanced Power BI dashboard creation or embedded BI authoring.

## BR-OOS-003: No Telephony Rules

The initial version does not include telephony integration, call recording, or call-based ticket creation.

## BR-OOS-004: No Omnichannel Routing Rules

The initial version does not include omnichannel messaging integrations.

## BR-OOS-005: No Workforce Management Rules

The initial version does not include workforce scheduling, payroll, or HR workflows.

## BR-OOS-006: No Predictive SLA Rules

The initial version does not include predictive SLA modeling or AI-based SLA forecasting.

## BR-OOS-007: No AI Classification Rules

The initial version does not include AI-assisted ticket classification.

## BR-OOS-008: No Complex Automation Builder

The initial version does not include a complex workflow automation engine.

## BR-OOS-009: No Production Enterprise SSO

The initial version does not include production-grade enterprise SSO.

## BR-OOS-010: No Multi-Tenant Billing

The initial version does not include a multi-tenant billing model.

---

# 22. Business Rule Traceability Matrix

| Rule Area | Related Use Cases | Related Requirement Groups |
|---|---|---|
| Authentication and Session Rules | UC-001 | FR-AUTH, NFR-SEC |
| Role-Based Access Rules | UC-001, UC-009, UC-010 | FR-AUTH, FR-USER, NFR-SEC |
| Scope-Based Visibility Rules | UC-002, UC-003, UC-004, UC-009 | FR-AUTH, FR-ORG, FR-TICKET, FR-DASH |
| Organizational Structure Rules | UC-010 | FR-ORG, FR-USER |
| User Management Rules | UC-010 | FR-USER, FR-ORG, FR-AUDIT |
| Customer Rules | UC-002 | FR-CUST, FR-TICKET |
| Ticket Creation Rules | UC-002 | FR-TICKET, FR-CUST, FR-AUDIT |
| Assignment Rules | UC-003 | FR-TICKET, FR-ORG, FR-AUDIT |
| Workflow Rules | UC-004, UC-007, UC-008 | FR-WF, FR-TICKET, FR-AUDIT |
| Priority Rules | UC-002, UC-004 | FR-TICKET, FR-AUDIT |
| SLA Rules | UC-002, UC-007, UC-008, UC-009 | FR-SLA, FR-DASH, FR-REP |
| Escalation Rules | UC-006 | FR-ESC, FR-TICKET, FR-AUDIT |
| Internal Comment Rules | UC-005 | FR-COMMENT, FR-AUDIT |
| Audit History Rules | UC-002 through UC-010 | FR-AUDIT, NFR-AUD |
| Dashboard Rules | UC-009 | FR-DASH, FR-SLA, FR-REP |
| Reporting Rules | UC-009 | FR-REP, NFR-MAINT |

---

# 23. Rule Enforcement Levels

Business rules may be enforced at different levels of the system.

| Enforcement Level | Description | Examples |
|---|---|---|
| Frontend | Prevents invalid user actions in the UI. | Hide unavailable actions, disable invalid buttons, show validation messages. |
| API | Validates requests before reaching application logic. | Reject missing fields, invalid IDs, malformed payloads. |
| Application Layer | Enforces use case and workflow rules. | Assign ticket, escalate ticket, close ticket. |
| Domain Layer | Protects core business invariants. | Closed tickets cannot be edited, ticket must have valid status. |
| Database | Preserves structural consistency. | Required fields, foreign keys, uniqueness constraints. |
| Audit Layer | Records important business actions. | Status change audit, assignment audit, user deactivation audit. |

---

# 24. Recommended Implementation Guidance

The following implementation guidance should be considered when translating business rules into code.

## 24.1 Keep Business Rules Out of Controllers

Controllers should not contain core business rule logic.

Controllers should be responsible for:

- Receiving requests.
- Calling application services or use cases.
- Returning responses.

## 24.2 Use Application Use Cases

Business workflows should be implemented as explicit use cases, commands, queries, or application services.

Examples:

- `CreateTicketUseCase`
- `AssignTicketUseCase`
- `EscalateTicketUseCase`
- `CloseTicketUseCase`
- `ManageUserUseCase`

## 24.3 Protect Domain Invariants

Domain entities should protect rules that must never be violated.

Examples:

- A closed ticket cannot be modified.
- A ticket must be resolved before closure.
- A ticket must have a valid status.
- A ticket must have a customer.

## 24.4 Duplicate Critical Rules Across Boundaries When Necessary

Some rules should be enforced in more than one place.

For example:

- The frontend may hide the `Close Ticket` button if a ticket is not resolved.
- The backend must still reject closing an unresolved ticket.
- The domain layer should still protect the invariant that only resolved tickets can be closed.

## 24.5 Test Business Rules Directly

Business rules should be tested explicitly.

Recommended test types:

- Unit tests for domain rules.
- Use case tests for workflow rules.
- Integration tests for API behavior.
- Authorization tests for role and scope restrictions.
- Audit tests for traceability rules.

---

# 25. Document Summary

This document defines the business rules that govern the initial version of OpsSphere.

The rules cover authentication, authorization, organizational structure, user management, ticket creation, assignment, workflow transitions, SLA handling, escalation, internal comments, audit history, dashboards, and reporting-ready data.

These rules provide the enterprise behavior behind the platform and help ensure that OpsSphere is more than a CRUD application.

The business rules should be treated as a foundation for domain modeling, backend implementation, API validation, authorization design, testing, and future operational governance.