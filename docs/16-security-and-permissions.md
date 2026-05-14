# Security and Permissions

## Document Information

| Field | Value |
|---|---|
| Project | OpsSphere |
| Document | Security and Permissions |
| File | `docs/16-security-and-permissions.md` |
| Version | 1.0 |
| Status | Draft |
| Project Type | Enterprise Support Operations Platform |
| Backend | .NET 10 / ASP.NET Core Web API |
| Authentication | JWT |
| Authorization | RBAC + Scope-Based Access Control |
| Related Issue | #5 |

---

## 1. Purpose

This document defines the initial security and permissions model for OpsSphere.

The purpose is to make authentication, authorization, role behavior, permission boundaries, operational scope, and audit-sensitive actions clear before implementation begins.

This document defines:

- User roles.
- Runtime role boundaries.
- Permission catalog.
- Permissions matrix.
- Authentication strategy.
- Authorization rules.
- Scope-based access rules.
- Frontend and backend security responsibilities.
- Audit-sensitive actions.
- Security testing expectations.
- Future security considerations.

This document is especially important because OpsSphere is designed as an enterprise operations platform, not a simple ticketing application.

---

## 2. Security Goals

OpsSphere security should support the following goals:

1. Protect all business data behind authentication.
2. Enforce role-based access control.
3. Enforce operational scope restrictions.
4. Prevent unauthorized data exposure.
5. Prevent users from modifying records outside their responsibility.
6. Keep customers outside the system in the initial version.
7. Preserve audit history for critical business actions.
8. Keep backend authorization independent from frontend visibility.
9. Apply least privilege.
10. Support future enterprise security improvements without redesigning the whole system.

---

## 3. Security Model Summary

OpsSphere uses three authorization dimensions:

```text
Role
  + Permission
  + Operational Scope
```

A user is allowed to perform an action only when all required conditions are satisfied.

Example:

```text
A Supervisor may assign a ticket only if:
  - The user is authenticated.
  - The user has the Supervisor role.
  - The user has the tickets.assign permission.
  - The ticket belongs to the supervisor's assigned account or campaign.
  - The target agent is eligible and active.
```

---

# 4. Runtime Roles

OpsSphere supports the following runtime business roles:

| Role | Runtime Access Type | Main Responsibility |
|---|---|---|
| Admin | Global administrative access | Manage users, roles, permissions, structure, assignments, and configuration. |
| Operations Manager | Regional oversight access | Monitor performance, SLA state, workload, escalations, dashboards, reports, and audit history within assigned regions. |
| Supervisor | Account or campaign operational access | Manage agents, ticket assignment, escalations, SLA risk, and ticket execution within assigned scope. |
| Agent | Assigned operational access | Create, update, comment, escalate, resolve, and close tickets within assigned scope and permissions. |
| Viewer | Read-only access | View dashboards, tickets, reports, and audit history within assigned scope. |

The following are not runtime business roles in the initial system:

| Actor | Reason |
|---|---|
| Customer | Customers are linked to tickets but do not directly access OpsSphere in the MVP. |
| Technical Owner | Technical Owner is an implementation/project role, not a business runtime role. |

---

# 5. Role Descriptions

## 5.1 Admin

Admin users manage the platform configuration and operational structure.

Admins may manage:

```text
Users
Roles
Permissions
Regions
Countries
Accounts
Campaigns
Manager assignments
Supervisor assignments
Agent assignments
Viewer scopes
Operational catalogs
Audit visibility
```

Security expectations:

```text
Admin actions are highly privileged.
Admin changes must be audited.
Admin users should still authenticate like all other users.
Admin users should not bypass audit logging.
```

---

## 5.2 Operations Manager

Operations Managers oversee one or more regions.

Operations Managers may:

```text
View dashboards within assigned regions.
View ticket lists within assigned regions.
View SLA state within assigned regions.
View escalations within assigned regions.
View reports within assigned regions.
View audit history for operational records within assigned regions.
```

Operations Managers generally should not:

```text
Create platform users.
Assign roles.
Assign permissions.
Manage regions, countries, accounts, or campaigns.
Modify tickets unless explicitly allowed by future business rules.
Bypass scope restrictions.
```

---

## 5.3 Supervisor

Supervisors manage tickets and agents within assigned accounts or campaigns.

Supervisors may:

```text
View tickets within assigned scope.
Assign and reassign tickets within assigned scope.
View eligible agents.
Update ticket status when permitted.
Add internal comments.
Escalate tickets.
Resolve tickets when permitted.
Close resolved tickets when permitted.
View SLA dashboards within assigned scope.
View audit history within assigned scope.
```

Supervisors may not:

```text
Manage users globally.
Assign roles globally.
Manage platform permissions.
Access records outside assigned account or campaign scope.
Assign tickets to agents outside the correct operational scope.
```

---

## 5.4 Agent

Agents handle support tickets within assigned operational scope.

Agents may:

```text
Create tickets within assigned account or campaign.
View assigned tickets.
View tickets within assigned campaign if allowed.
Update ticket status.
Add internal comments.
Escalate tickets.
Resolve tickets.
Close tickets when allowed.
View limited SLA information for their tickets or scope.
```

Agents may not:

```text
Manage users.
Assign roles.
Manage permissions.
Manage operational structure.
Assign tickets unless explicitly allowed.
View data outside assigned scope.
Modify closed tickets unless reopening is supported and authorized.
```

---

## 5.5 Viewer

Viewers have read-only operational visibility.

Viewers may:

```text
View dashboards within assigned scope.
View ticket lists within assigned scope.
View ticket details within assigned scope.
View reports within assigned scope.
View audit history within assigned scope.
Export reports if explicitly permitted.
```

Viewers may not:

```text
Create tickets.
Update tickets.
Assign tickets.
Add internal comments.
Escalate tickets.
Resolve tickets.
Close tickets.
Manage users.
Manage roles.
Manage permissions.
Manage operational structure.
Deactivate records.
```

---

# 6. Permission Catalog

Permissions are granular capabilities assigned to roles.

Permission codes should use lowercase dot notation.

Example:

```text
tickets.create
tickets.assign
audit.view
```

---

## 6.1 Identity and Access Permissions

| Permission Code | Description |
|---|---|
| `users.view` | View users. |
| `users.manage` | Create, update, deactivate, and manage users. |
| `roles.view` | View roles. |
| `roles.manage` | Manage roles. |
| `permissions.view` | View permissions. |
| `permissions.manage` | Manage role-permission assignments. |
| `scopes.view` | View user scopes. |
| `scopes.manage` | Assign or update user operational scopes. |

---

## 6.2 Organization Permissions

| Permission Code | Description |
|---|---|
| `organization.view` | View regions, countries, accounts, and campaigns. |
| `organization.manage` | Create, update, deactivate, or assign organization records. |
| `regions.manage` | Manage regions. |
| `countries.manage` | Manage countries. |
| `accounts.manage` | Manage accounts. |
| `campaigns.manage` | Manage campaigns. |
| `assignments.manage` | Manage manager, supervisor, agent, and viewer assignments. |

---

## 6.3 Customer Permissions

| Permission Code | Description |
|---|---|
| `customers.view` | View customer records within scope. |
| `customers.create` | Create customer records within scope. |
| `customers.update` | Update customer records within scope. |
| `customers.history.view` | View customer ticket history within scope. |

---

## 6.4 Ticket Permissions

| Permission Code | Description |
|---|---|
| `tickets.view` | View tickets within scope. |
| `tickets.create` | Create tickets within scope. |
| `tickets.update` | Update editable ticket fields within scope. |
| `tickets.assign` | Assign or reassign tickets within scope. |
| `tickets.update_status` | Update ticket status within scope. |
| `tickets.update_priority` | Update ticket priority within scope. |
| `tickets.comment` | Add internal comments within scope. |
| `tickets.escalate` | Escalate tickets within scope. |
| `tickets.resolve` | Resolve tickets within scope. |
| `tickets.close` | Close resolved tickets within scope. |
| `tickets.history.view` | View ticket history within scope. |
| `tickets.reopen` | Reopen closed tickets if future reopening is supported. |

---

## 6.5 SLA Permissions

| Permission Code | Description |
|---|---|
| `sla.view` | View SLA state and SLA dashboards within scope. |
| `sla.policies.view` | View SLA policies. |
| `sla.policies.manage` | Create or update SLA policies. |
| `sla.evaluate` | Trigger or run SLA evaluation. Usually system-only or admin-only. |

---

## 6.6 Dashboard and Reporting Permissions

| Permission Code | Description |
|---|---|
| `dashboard.view` | View operational dashboards within scope. |
| `reports.view` | View reports within scope. |
| `reports.export` | Export reports within scope. |

---

## 6.7 Audit Permissions

| Permission Code | Description |
|---|---|
| `audit.view` | View audit history within scope. |
| `audit.admin_view` | View administrative audit history. |
| `audit.export` | Export audit records if enabled. |

---

# 7. Initial Permissions Matrix

The following matrix defines the initial recommended permissions by role.

Legend:

```text
Y = Allowed
N = Not allowed
S = Allowed only within assigned operational scope
RO = Read-only
```

---

## 7.1 Identity and Access Matrix

| Permission | Admin | Operations Manager | Supervisor | Agent | Viewer |
|---|:---:|:---:|:---:|:---:|:---:|
| `users.view` | Y | N | N | N | N |
| `users.manage` | Y | N | N | N | N |
| `roles.view` | Y | N | N | N | N |
| `roles.manage` | Y | N | N | N | N |
| `permissions.view` | Y | N | N | N | N |
| `permissions.manage` | Y | N | N | N | N |
| `scopes.view` | Y | N | N | N | N |
| `scopes.manage` | Y | N | N | N | N |

---

## 7.2 Organization Matrix

| Permission | Admin | Operations Manager | Supervisor | Agent | Viewer |
|---|:---:|:---:|:---:|:---:|:---:|
| `organization.view` | Y | S | S | S | S |
| `organization.manage` | Y | N | N | N | N |
| `regions.manage` | Y | N | N | N | N |
| `countries.manage` | Y | N | N | N | N |
| `accounts.manage` | Y | N | N | N | N |
| `campaigns.manage` | Y | N | N | N | N |
| `assignments.manage` | Y | N | N | N | N |

---

## 7.3 Customer Matrix

| Permission | Admin | Operations Manager | Supervisor | Agent | Viewer |
|---|:---:|:---:|:---:|:---:|:---:|
| `customers.view` | Y | S | S | S | S |
| `customers.create` | Y | N | S | S | N |
| `customers.update` | Y | N | S | S | N |
| `customers.history.view` | Y | S | S | S | S |

---

## 7.4 Ticket Matrix

| Permission | Admin | Operations Manager | Supervisor | Agent | Viewer |
|---|:---:|:---:|:---:|:---:|:---:|
| `tickets.view` | Y | S | S | S | S |
| `tickets.create` | N | N | S | S | N |
| `tickets.update` | N | N | S | S | N |
| `tickets.assign` | N | N | S | N | N |
| `tickets.update_status` | N | N | S | S | N |
| `tickets.update_priority` | N | N | S | S | N |
| `tickets.comment` | N | S | S | S | N |
| `tickets.escalate` | N | N | S | S | N |
| `tickets.resolve` | N | N | S | S | N |
| `tickets.close` | N | N | S | S | N |
| `tickets.history.view` | Y | S | S | S | S |
| `tickets.reopen` | Y | N | S | N | N |

Design note:

```text
Admin can view operational records for governance, but Admin is not the default ticket operator.
Ticket operations are primarily owned by Supervisors and Agents.
```

---

## 7.5 SLA Matrix

| Permission | Admin | Operations Manager | Supervisor | Agent | Viewer |
|---|:---:|:---:|:---:|:---:|:---:|
| `sla.view` | Y | S | S | S | S |
| `sla.policies.view` | Y | S | S | N | N |
| `sla.policies.manage` | Y | N | N | N | N |
| `sla.evaluate` | Y | N | N | N | N |

---

## 7.6 Dashboard and Reporting Matrix

| Permission | Admin | Operations Manager | Supervisor | Agent | Viewer |
|---|:---:|:---:|:---:|:---:|:---:|
| `dashboard.view` | Y | S | S | S | S |
| `reports.view` | Y | S | S | N | S |
| `reports.export` | Y | S | S | N | S |

---

## 7.7 Audit Matrix

| Permission | Admin | Operations Manager | Supervisor | Agent | Viewer |
|---|:---:|:---:|:---:|:---:|:---:|
| `audit.view` | Y | S | S | N | S |
| `audit.admin_view` | Y | N | N | N | N |
| `audit.export` | Y | S | N | N | S |

---

# 8. Authentication Strategy

## 8.1 Authentication Method

OpsSphere will use JWT authentication for the MVP.

Authentication flow:

```text
User enters credentials.
API validates credentials.
API verifies active user state.
API loads assigned roles.
API loads assigned permissions.
API loads assigned operational scope.
API generates JWT.
Frontend stores token.
Frontend sends token with protected API requests.
API validates token on every protected request.
```

---

## 8.2 Login Rules

Login must enforce:

```text
User must exist.
User must be active.
Credentials must be valid.
User must have at least one valid role.
Operational users must have compatible scope.
Invalid login errors must be generic.
Successful login may update LastLoginAt.
```

Generic login error example:

```text
Invalid email or password.
```

Avoid exposing:

```text
Email does not exist.
Password is wrong.
User is deactivated.
User has missing role.
```

---

## 8.3 JWT Content

The JWT should include only the minimum useful claims.

Suggested claims:

```text
sub
email
display_name
role
permission codes or permission version
scope summary or scope reference
iat
exp
jti
```

Sensitive data must not be stored in the JWT.

Do not store:

```text
Password hash
Full permission matrix if too large
Confidential customer data
Internal audit details
Secrets
Connection strings
```

---

## 8.4 Token Expiration

The MVP should use short-lived access tokens.

Recommended initial policy:

```text
Access token expires after a configured duration.
Expired tokens are rejected.
Frontend redirects to login when token expires.
```

Refresh tokens may be added in a future phase.

---

## 8.5 Password Security

Passwords must be stored using a secure password hashing mechanism.

Security rules:

```text
Never store plaintext passwords.
Never log passwords.
Never return passwords through API responses.
Use framework-supported password hashing where possible.
Use HTTPS in deployed environments.
```

---

## 8.6 Logout

For the MVP, logout may be handled by the frontend removing the access token.

If refresh tokens are added later, server-side token revocation should be introduced.

MVP logout behavior:

```text
Frontend removes token.
Protected requests without token are rejected.
```

---

# 9. Authorization Strategy

## 9.1 Authorization Layers

Authorization should be enforced in multiple layers:

| Layer | Responsibility |
|---|---|
| Angular Frontend | Hide unavailable actions and routes for better UX. |
| API Controllers | Require authentication and high-level policies. |
| Application Layer | Enforce use case authorization and scope checks. |
| Domain Layer | Enforce business invariants independent from user interface. |
| Database Queries | Apply scope filters to prevent data leakage. |

The backend is the source of truth.

Frontend route guards and hidden buttons are not security controls by themselves.

---

## 9.2 Authorization Decision Model

A request should be authorized using this decision model:

```text
1. Is the user authenticated?
2. Is the user active?
3. Does the user have a required role?
4. Does the user have the required permission?
5. Does the user have scope over the target resource?
6. Is the resource in a state that allows the action?
7. Is the action audit-sensitive?
8. If all checks pass, allow the action.
```

---

## 9.3 Policy-Based Authorization

ASP.NET Core policies should be used for high-level authorization.

Example policy names:

```text
CanManageUsers
CanManageOrganization
CanViewTickets
CanCreateTickets
CanAssignTickets
CanUpdateTicketStatus
CanCommentOnTickets
CanEscalateTickets
CanResolveTickets
CanCloseTickets
CanViewSla
CanViewDashboard
CanViewReports
CanViewAudit
```

Policies should be backed by:

```text
Role checks
Permission checks
Scope checks
Resource state checks when needed
```

---

## 9.4 Resource-Based Authorization

Resource-based authorization is required when permission depends on the specific record.

Examples:

```text
Can this Supervisor assign this ticket?
Can this Agent view this ticket?
Can this Viewer see this audit record?
Can this Manager view this campaign's SLA data?
Can this user update this customer?
```

Resource-based checks should consider:

```text
RegionId
CountryId
AccountId
CampaignId
AssignedAgentUserId
SupervisorUserId
CreatedByUserId
Current ticket status
Current ticket SLA state
```

---

# 10. Scope-Based Access Control

## 10.1 Scope Types

OpsSphere supports the following initial scope types:

| Scope Type | Meaning |
|---|---|
| Region | User can access data under one or more regions. |
| Country | User can access data under one or more countries. |
| Account | User can access data under one or more accounts. |
| Campaign | User can access data under one or more campaigns. |
| AssignedTickets | User can access tickets assigned to them. |

---

## 10.2 Scope Rules by Role

| Role | Recommended Scope |
|---|---|
| Admin | Global administrative scope. |
| Operations Manager | Region scope. |
| Supervisor | Account or campaign scope. |
| Agent | Campaign, account, or assigned-ticket scope. |
| Viewer | Region, country, account, or campaign read-only scope. |

---

## 10.3 Scope Filtering Rules

The following data must be filtered by operational scope:

```text
Ticket lists
Ticket detail
Customer records
Customer ticket history
SLA dashboards
Operational dashboards
Reports
Audit logs related to operational entities
Eligible agent lookups
Campaign lists
Account lists
```

---

## 10.4 Scope Validation on Write Actions

The following write actions must validate operational scope before saving changes:

```text
Create ticket
Update ticket
Assign ticket
Update ticket status
Update ticket priority
Add internal comment
Escalate ticket
Resolve ticket
Close ticket
Create customer
Update customer
Create assignment
Update assignment
```

---

## 10.5 Cross-Scope Access Denial

If a user attempts to access or modify data outside their assigned scope, the system must reject the request.

Recommended API behavior:

```text
403 Forbidden
```

or:

```text
404 Not Found
```

Use `404 Not Found` when revealing that the resource exists would leak information.

---

# 11. Frontend Security Responsibilities

The Angular frontend should:

```text
Protect routes using auth guards.
Hide actions the user cannot perform.
Use the current user profile to build role-aware navigation.
Send JWT token on protected API requests.
Redirect to login when token is missing or expired.
Display authorization errors clearly.
Avoid storing sensitive data in local UI state.
Avoid logging tokens or sensitive user data.
```

The Angular frontend must not:

```text
Assume hidden buttons are sufficient security.
Make authorization decisions that the backend does not enforce.
Expose restricted data returned by mistake.
Store passwords.
Log JWT tokens.
```

---

# 12. Backend Security Responsibilities

The ASP.NET Core Web API must:

```text
Require authentication for protected endpoints.
Validate JWT tokens.
Reject expired or invalid tokens.
Enforce role-based authorization.
Enforce permission-based authorization.
Enforce scope-based authorization.
Validate resource state before write actions.
Apply scope filters to queries.
Hash passwords securely.
Avoid logging passwords or tokens.
Return consistent error responses.
Create audit records for audit-sensitive actions.
```

The backend must not:

```text
Trust frontend visibility rules.
Return records before applying scope filters.
Allow deactivated users to authenticate.
Allow users without roles to access business modules.
Allow operational users without scope to access business data.
Expose sensitive authentication failure details.
```

---

# 13. Audit-Sensitive Actions

Audit logging is required for critical business and administrative actions.

Audit records should include:

```text
ActorUserId
Action
EntityType
EntityId
PreviousValue when applicable
NewValue when applicable
Timestamp
IpAddress when available
UserAgent when available
CorrelationId when available
```

---

## 13.1 User and Access Audit Actions

The following user and access actions must be audited:

```text
UserCreated
UserUpdated
UserDeactivated
UserReactivated
RoleAssigned
RoleRemoved
RolePermissionsUpdated
ScopeAssigned
ScopeUpdated
ScopeRemoved
PasswordChanged
LoginSucceeded when login auditing is enabled
LoginFailed when security auditing is enabled
```

---

## 13.2 Organization Audit Actions

The following organization actions must be audited:

```text
RegionCreated
RegionUpdated
RegionDeactivated
CountryCreated
CountryUpdated
CountryDeactivated
AccountCreated
AccountUpdated
AccountDeactivated
CampaignCreated
CampaignUpdated
CampaignDeactivated
ManagerAssigned
ManagerAssignmentUpdated
ManagerAssignmentDeactivated
SupervisorAssigned
SupervisorAssignmentUpdated
SupervisorAssignmentDeactivated
AgentAssigned
AgentAssignmentUpdated
AgentAssignmentDeactivated
```

---

## 13.3 Customer Audit Actions

The following customer actions must be audited:

```text
CustomerCreated
CustomerUpdated
CustomerDeactivated
CustomerMerged if future merge is supported
```

---

## 13.4 Ticket Audit Actions

The following ticket actions must be audited:

```text
TicketCreated
TicketUpdated
TicketAssigned
TicketReassigned
TicketStatusChanged
TicketPriorityChanged
TicketCommentAdded
TicketEscalated
TicketResolved
TicketClosed
TicketReopened if future reopening is supported
```

---

## 13.5 SLA Audit Actions

The following SLA actions should be audited:

```text
SlaPolicyCreated
SlaPolicyUpdated
SlaPolicyDeactivated
SlaStateChanged
SlaBreached
SlaCompleted
```

---

## 13.6 Reporting and Export Audit Actions

The following reporting actions may be audited depending on implementation:

```text
ReportExported
AuditLogExported
TicketReportExported
SlaReportExported
```

---

# 14. Audit Immutability Rules

Audit records should be treated as immutable.

Normal business workflows must not:

```text
Update audit records.
Delete audit records.
Soft delete audit records.
Allow users to manually edit audit entries.
```

If audit corrections are ever required, the correction itself should create a new audit event.

---

# 15. Security Error Handling

## 15.1 Authentication Errors

Authentication failures should return:

```text
401 Unauthorized
```

Example response:

```json
{
  "error": {
    "code": "unauthorized",
    "message": "Authentication is required."
  }
}
```

---

## 15.2 Authorization Errors

Authorization failures should return:

```text
403 Forbidden
```

Example response:

```json
{
  "error": {
    "code": "forbidden",
    "message": "You are not authorized to perform this action."
  }
}
```

---

## 15.3 Resource Visibility Errors

If returning `403 Forbidden` would reveal the existence of a restricted resource, the API may return:

```text
404 Not Found
```

Example:

```json
{
  "error": {
    "code": "not_found",
    "message": "The requested resource was not found."
  }
}
```

---

## 15.4 Validation Errors

Validation failures should return:

```text
422 Unprocessable Entity
```

or:

```text
400 Bad Request
```

Example response:

```json
{
  "error": {
    "code": "validation_error",
    "message": "The request contains validation errors.",
    "details": [
      {
        "field": "escalationReason",
        "message": "Escalation reason is required."
      }
    ]
  }
}
```

---

# 16. Sensitive Data Handling

The system must not log or expose sensitive data.

Do not log:

```text
Passwords
Password hashes
JWT tokens
Authorization headers
Connection strings
Secrets
Customer confidential details beyond what is operationally necessary
```

Do not return through API responses:

```text
PasswordHash
Internal security configuration
Secret keys
Full token signing information
Infrastructure secrets
```

---

# 17. Least Privilege Rules

OpsSphere should follow least privilege principles.

Rules:

```text
Users receive only the permissions required for their role.
Viewers are read-only by default.
Agents operate only within assigned scope.
Supervisors operate only within assigned account or campaign scope.
Operations Managers operate only within assigned regional scope.
Admins manage configuration but should not be treated as default ticket operators.
Audit access must be explicitly controlled.
Export access must be explicitly controlled.
```

---

# 18. Default Role Permission Sets

## 18.1 Admin Default Permissions

```text
users.view
users.manage
roles.view
roles.manage
permissions.view
permissions.manage
scopes.view
scopes.manage
organization.view
organization.manage
regions.manage
countries.manage
accounts.manage
campaigns.manage
assignments.manage
customers.view
customers.create
customers.update
customers.history.view
tickets.view
tickets.history.view
tickets.reopen
sla.view
sla.policies.view
sla.policies.manage
sla.evaluate
dashboard.view
reports.view
reports.export
audit.view
audit.admin_view
audit.export
```

---

## 18.2 Operations Manager Default Permissions

```text
organization.view
customers.view
customers.history.view
tickets.view
tickets.history.view
tickets.comment
sla.view
sla.policies.view
dashboard.view
reports.view
reports.export
audit.view
audit.export
```

Scope:

```text
Region
```

---

## 18.3 Supervisor Default Permissions

```text
organization.view
customers.view
customers.create
customers.update
customers.history.view
tickets.view
tickets.create
tickets.update
tickets.assign
tickets.update_status
tickets.update_priority
tickets.comment
tickets.escalate
tickets.resolve
tickets.close
tickets.history.view
tickets.reopen
sla.view
sla.policies.view
dashboard.view
reports.view
reports.export
audit.view
```

Scope:

```text
Account
Campaign
```

---

## 18.4 Agent Default Permissions

```text
organization.view
customers.view
customers.create
customers.update
customers.history.view
tickets.view
tickets.create
tickets.update
tickets.update_status
tickets.update_priority
tickets.comment
tickets.escalate
tickets.resolve
tickets.close
tickets.history.view
sla.view
dashboard.view
```

Scope:

```text
Campaign
Account
AssignedTickets
```

---

## 18.5 Viewer Default Permissions

```text
organization.view
customers.view
customers.history.view
tickets.view
tickets.history.view
sla.view
dashboard.view
reports.view
reports.export
audit.view
```

Scope:

```text
Region
Country
Account
Campaign
```

Access type:

```text
Read-only
```

---

# 19. Endpoint Security Mapping

## 19.1 Authentication Endpoints

| Endpoint | Required Access |
|---|---|
| `POST /api/auth/login` | Anonymous |
| `GET /api/auth/me` | Authenticated user |
| `POST /api/auth/logout` | Authenticated user |

---

## 19.2 User and Role Endpoints

| Endpoint | Required Access |
|---|---|
| `POST /api/users` | Admin + `users.manage` |
| `GET /api/users` | Admin + `users.view` |
| `GET /api/users/{id}` | Admin + `users.view` |
| `PUT /api/users/{id}` | Admin + `users.manage` |
| `PUT /api/users/{id}/roles` | Admin + `roles.manage` |
| `PUT /api/users/{id}/scopes` | Admin + `scopes.manage` |
| `POST /api/users/{id}/deactivate` | Admin + `users.manage` |
| `GET /api/roles` | Admin + `roles.view` |
| `GET /api/permissions` | Admin + `permissions.view` |
| `PUT /api/roles/{id}/permissions` | Admin + `permissions.manage` |

---

## 19.3 Organization Endpoints

| Endpoint | Required Access |
|---|---|
| `GET /api/regions` | Authenticated + scope filtered |
| `POST /api/regions` | Admin + `regions.manage` |
| `PUT /api/regions/{id}` | Admin + `regions.manage` |
| `POST /api/regions/{id}/deactivate` | Admin + `regions.manage` |
| `GET /api/countries` | Authenticated + scope filtered |
| `POST /api/countries` | Admin + `countries.manage` |
| `PUT /api/countries/{id}` | Admin + `countries.manage` |
| `POST /api/countries/{id}/deactivate` | Admin + `countries.manage` |
| `GET /api/accounts` | Authenticated + scope filtered |
| `POST /api/accounts` | Admin + `accounts.manage` |
| `PUT /api/accounts/{id}` | Admin + `accounts.manage` |
| `POST /api/accounts/{id}/deactivate` | Admin + `accounts.manage` |
| `GET /api/campaigns` | Authenticated + scope filtered |
| `POST /api/campaigns` | Admin + `campaigns.manage` |
| `PUT /api/campaigns/{id}` | Admin + `campaigns.manage` |
| `POST /api/campaigns/{id}/deactivate` | Admin + `campaigns.manage` |

---

## 19.4 Customer Endpoints

| Endpoint | Required Access |
|---|---|
| `POST /api/customers` | `customers.create` + scope |
| `GET /api/customers` | `customers.view` + scope |
| `GET /api/customers/{id}` | `customers.view` + scope |
| `PUT /api/customers/{id}` | `customers.update` + scope |
| `GET /api/customers/{id}/tickets` | `customers.history.view` + scope |

---

## 19.5 Ticket Endpoints

| Endpoint | Required Access |
|---|---|
| `POST /api/tickets` | `tickets.create` + scope |
| `GET /api/tickets` | `tickets.view` + scope |
| `GET /api/tickets/{id}` | `tickets.view` + scope |
| `PUT /api/tickets/{id}` | `tickets.update` + scope |
| `POST /api/tickets/{id}/assign` | `tickets.assign` + scope |
| `PUT /api/tickets/{id}/status` | `tickets.update_status` + scope |
| `POST /api/tickets/{id}/comments` | `tickets.comment` + scope |
| `GET /api/tickets/{id}/comments` | `tickets.view` + scope |
| `POST /api/tickets/{id}/escalate` | `tickets.escalate` + scope |
| `POST /api/tickets/{id}/resolve` | `tickets.resolve` + scope |
| `POST /api/tickets/{id}/close` | `tickets.close` + scope |
| `GET /api/tickets/{id}/history` | `tickets.history.view` + scope |
| `GET /api/tickets/{id}/eligible-agents` | `tickets.assign` + scope |

---

## 19.6 SLA, Dashboard, Reports, and Audit Endpoints

| Endpoint | Required Access |
|---|---|
| `GET /api/sla/dashboard` | `sla.view` + scope |
| `GET /api/sla/at-risk` | `sla.view` + scope |
| `GET /api/sla/breached` | `sla.view` + scope |
| `GET /api/sla/policies` | `sla.policies.view` |
| `POST /api/sla/policies` | Admin + `sla.policies.manage` |
| `GET /api/dashboard/operations` | `dashboard.view` + scope |
| `GET /api/dashboard/agent-workload` | `dashboard.view` + scope |
| `GET /api/dashboard/supervisor-workload` | `dashboard.view` + scope |
| `GET /api/reports/tickets` | `reports.view` + scope |
| `GET /api/reports/tickets/export` | `reports.export` + scope |
| `GET /api/reports/sla` | `reports.view` + scope |
| `GET /api/reports/sla/export` | `reports.export` + scope |
| `GET /api/audit-logs` | `audit.view` + scope or `audit.admin_view` |
| `GET /api/audit-logs/entity/{entityType}/{entityId}` | `audit.view` + scope |

---

# 20. Security Testing Expectations

Security behavior should be covered by tests.

Recommended test categories:

```text
Authentication tests
Role authorization tests
Permission authorization tests
Scope filtering tests
Cross-scope denial tests
Viewer read-only tests
Deactivated user tests
Audit creation tests
Sensitive data exposure tests
Endpoint protection tests
```

---

## 20.1 Authentication Test Cases

```text
Valid user can log in.
Invalid credentials are rejected.
Deactivated user cannot log in.
Expired token is rejected.
Protected endpoint rejects missing token.
Protected endpoint rejects invalid token.
```

---

## 20.2 Authorization Test Cases

```text
Viewer cannot create tickets.
Viewer cannot update tickets.
Agent cannot assign tickets.
Agent cannot manage users.
Supervisor cannot manage roles.
Operations Manager cannot create users.
Admin can manage users.
Technical Owner is not treated as a runtime business role.
```

---

## 20.3 Scope Test Cases

```text
Agent cannot view tickets outside assigned campaign.
Supervisor cannot assign tickets outside assigned account or campaign.
Manager cannot view data outside assigned region.
Viewer cannot view audit records outside assigned scope.
Ticket list applies scope filters.
Dashboard applies scope filters.
Reports apply scope filters.
Customer history applies scope filters.
```

---

## 20.4 Audit Test Cases

```text
Creating a user creates an audit record.
Assigning a role creates an audit record.
Assigning scope creates an audit record.
Creating a ticket creates an audit record.
Assigning a ticket creates an audit record.
Updating ticket status creates an audit record.
Adding a comment creates an audit record.
Escalating a ticket creates an audit record.
Resolving a ticket creates an audit record.
Closing a ticket creates an audit record.
Deactivating a user creates an audit record.
```

---

# 21. Future Security Considerations

The following security improvements may be added in later phases:

```text
Refresh tokens
Token revocation
Password reset flow
Login audit dashboard
Multi-factor authentication
Enterprise SSO
Azure AD / Microsoft Entra ID integration
Rate limiting
Account lockout policy
Security event monitoring
Advanced audit export controls
Field-level permissions
Data masking for sensitive customer fields
Attachment virus scanning
Secret management with Azure Key Vault
Application Insights security telemetry
```

These are not required for the MVP unless explicitly added to scope.

---

# 22. Out of Scope for MVP

The following security features are outside the initial MVP scope:

```text
Production enterprise SSO
Multi-factor authentication
Customer portal authentication
External customer roles
Native mobile authentication
Advanced data masking
Field-level authorization
Complex approval workflows
Full IAM platform replacement
Advanced SIEM integration
```

---

# 23. Related Documents

| Document | Relationship |
|---|---|
| `docs/06-requirements.md` | Defines authentication, authorization, user role, and audit requirements. |
| `docs/07-use-cases.md` | Defines workflows that require security and permission enforcement. |
| `docs/08-business-process-flows.md` | Defines business processes that require authorization and audit behavior. |
| `docs/09-business-rules.md` | Defines security, role, scope, and audit business rules. |
| `docs/10-domain-model.md` | Defines User, Role, Permission, Scope, Ticket, SLA, Escalation, and Audit Trail concepts. |
| `docs/11-architecture-overview.md` | Defines JWT, RBAC, backend authorization, audit logging, and security architecture direction. |
| `docs/12-c4-architecture.md` | Shows authentication and authorization as backend responsibilities. |
| `docs/13-uml-diagrams.md` | Shows login, ticket creation, and escalation flows where authorization applies. |
| `docs/14-database-design.md` | Defines users, roles, permissions, scopes, audit logs, and related persistence structures. |
| `docs/15-api-design.md` | Defines endpoint-level authorization expectations. |

---

# 24. Document Summary

This document defines the initial security and permissions model for OpsSphere.

OpsSphere uses JWT authentication, role-based access control, permission-based authorization, and scope-based visibility to protect operational data.

The system supports five runtime business roles:

```text
Admin
Operations Manager
Supervisor
Agent
Viewer
```

Customers do not directly access the initial system, and Technical Owner is not a runtime business role.

The backend must enforce all authorization rules independently from frontend visibility. The frontend may hide unavailable features, but the API and application layer remain the source of truth.

Audit logging is required for critical administrative and operational actions, including user changes, role changes, scope changes, structure changes, ticket lifecycle events, escalation events, SLA changes, and selected exports.

This document completes the initial architecture and technical design documentation set for issue #5.
