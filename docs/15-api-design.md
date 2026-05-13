# API Design

## Document Information

| Field | Value |
|---|---|
| Project | OpsSphere |
| Document | API Design |
| File | `docs/15-api-design.md` |
| Version | 1.0 |
| Status | Draft |
| Project Type | Enterprise Support Operations Platform |
| Backend | .NET 8 / ASP.NET Core Web API |
| Architecture | Clean Architecture / CQRS with MediatR |
| Related Issue | #5 |

---

## 1. Purpose

This document defines the initial API design for OpsSphere.

The API design describes the HTTP endpoints that will be exposed by the ASP.NET Core Web API backend before implementation begins.

The purpose of this document is to define:

- API conventions.
- Resource structure.
- Authentication endpoints.
- User and role management endpoints.
- Operational structure endpoints.
- Customer endpoints.
- Ticket lifecycle endpoints.
- Comment endpoints.
- Escalation endpoints.
- SLA endpoints.
- Dashboard endpoints.
- Report endpoints.
- Audit history endpoints.
- Request and response examples.
- Authorization expectations.
- Error response format.

This document connects directly to:

- Requirements.
- Use cases.
- Business rules.
- Domain model.
- Database design.
- Security and permissions.
- ASP.NET Core controller design.
- CQRS command and query handlers.

---

## 2. API Design Goals

The API should support the following goals:

1. Provide a clear backend contract for the Angular frontend.
2. Keep endpoints resource-oriented and predictable.
3. Support authentication with JWT.
4. Enforce role-based and scope-based authorization.
5. Support the ticket lifecycle from creation to closure.
6. Support SLA visibility and operational dashboards.
7. Support audit history for critical business actions.
8. Keep controllers thin by delegating business logic to application commands and queries.
9. Return consistent response shapes.
10. Return clear validation and authorization errors.

---

## 3. API Base Path

Business API endpoints should use the following base path:

```text
/api
```

Example:

```http
GET /api/tickets
```

Health check endpoints are operational endpoints and intentionally do not use the `/api` base path.

---

## 4. API Style

OpsSphere will use REST-style HTTP endpoints.

The API should use resource-oriented routes such as:

```http
GET /api/tickets
POST /api/tickets
GET /api/tickets/{id}
PUT /api/tickets/{id}/status
POST /api/tickets/{id}/comments
POST /api/tickets/{id}/escalate
```

The API does not need to be perfectly REST-pure. Business workflow actions such as `assign`, `escalate`, `resolve`, and `close` may use action-style subroutes because they represent domain workflows, not simple field updates.

---

## 5. HTTP Methods

| Method | Purpose |
|---|---|
| GET | Retrieve data. |
| POST | Create a resource or execute a workflow action. |
| PUT | Replace or update a known resource or state. |
| PATCH | Partially update a resource when needed. |
| DELETE | Soft delete or deactivate where allowed. |

For MVP, destructive delete operations should be avoided for important operational records.

Prefer:

```text
Deactivate
Soft delete
Status update
```

over physical deletion.

---

## 6. Common Headers

## 6.1 Request Headers

Protected endpoints should require:

```http
Authorization: Bearer {jwt_token}
Content-Type: application/json
Accept: application/json
```

Optional tracing header:

```http
X-Correlation-Id: {correlation_id}
```

---

## 6.2 Response Headers

The API may return:

```http
Content-Type: application/json
X-Correlation-Id: {correlation_id}
```

---

## 7. Authentication and Authorization

## 7.1 Authentication Strategy

OpsSphere will use JWT authentication.

Flow:

```text
User submits credentials.
API validates credentials.
API verifies active user status.
API loads role and scope information.
API returns JWT and user profile.
Frontend stores token.
Frontend sends token on protected requests.
```

---

## 7.2 Authorization Strategy

Authorization will use:

```text
Role
+ Permission
+ Operational Scope
```

Examples:

```text
Admin can manage users and organizational structure.
Operations Manager can view dashboards and reports within assigned regions.
Supervisor can manage tickets within assigned accounts or campaigns.
Agent can work only on tickets within assigned operational scope.
Viewer can only access read-only operational information.
```

Frontend visibility rules improve UX, but backend authorization is the source of truth.

---

## 8. Common Response Shapes

## 8.1 Single Resource Response

```json
{
  "data": {
    "id": "00000000-0000-0000-0000-000000000000"
  }
}
```

---

## 8.2 List Response

```json
{
  "data": [],
  "page": 1,
  "pageSize": 25,
  "totalCount": 0,
  "totalPages": 0
}
```

---

## 8.3 Command Response

```json
{
  "data": {
    "id": "00000000-0000-0000-0000-000000000000",
    "message": "Operation completed successfully."
  }
}
```

---

## 8.4 Error Response

The API should return a consistent error shape.

```json
{
  "error": {
    "code": "validation_error",
    "message": "The request contains validation errors.",
    "details": [
      {
        "field": "priority",
        "message": "Priority is required."
      }
    ],
    "correlationId": "8e3f0f90-7e56-4fd8-938a-8422f7fcd123"
  }
}
```

---

## 9. HTTP Status Codes

| Status Code | Meaning |
|---:|---|
| 200 | Successful read or update. |
| 201 | Resource created. |
| 204 | Successful action with no response body. |
| 400 | Invalid request or business rule violation. |
| 401 | Unauthenticated request. |
| 403 | Authenticated but not authorized. |
| 404 | Resource not found. |
| 409 | Conflict with current resource state. |
| 422 | Validation error. |
| 500 | Unexpected server error. |

---

# 10. Authentication API

## 10.1 Login

Authenticates a user and returns a JWT.

```http
POST /api/auth/login
```

Required authorization:

```text
Anonymous
```

Request body:

```json
{
  "email": "agent@example.com",
  "password": "Password123!"
}
```

Response:

```json
{
  "data": {
    "accessToken": "jwt_token_here",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "user": {
      "id": "4a73c3d8-4e7a-4fd7-9d68-ff0e042e6001",
      "email": "agent@example.com",
      "displayName": "Ana Lopez",
      "roles": [
        "Agent"
      ],
      "scopes": [
        {
          "scopeType": "Campaign",
          "campaignId": "70c5f763-94d7-4a3a-b16d-57a0ac5d9f2a"
        }
      ]
    }
  }
}
```

Business rules:

```text
Only active users may authenticate.
Invalid credentials must be rejected.
Deactivated users must not receive tokens.
Generic login errors should avoid exposing sensitive details.
```

---

## 10.2 Get Current User Profile

Returns the authenticated user's profile, roles, permissions, and scopes.

```http
GET /api/auth/me
```

Required authorization:

```text
Authenticated user
```

Response:

```json
{
  "data": {
    "id": "4a73c3d8-4e7a-4fd7-9d68-ff0e042e6001",
    "email": "agent@example.com",
    "displayName": "Ana Lopez",
    "roles": [
      "Agent"
    ],
    "permissions": [
      "tickets.create",
      "tickets.update_status",
      "tickets.comment",
      "tickets.escalate",
      "tickets.resolve"
    ],
    "scopes": [
      {
        "scopeType": "Campaign",
        "campaignId": "70c5f763-94d7-4a3a-b16d-57a0ac5d9f2a"
      }
    ]
  }
}
```

---

## 10.3 Logout

For JWT-based authentication, logout may be handled on the frontend by removing the token.

If refresh tokens are added later, server-side logout may be implemented.

```http
POST /api/auth/logout
```

Required authorization:

```text
Authenticated user
```

MVP behavior:

```text
Frontend removes token.
No server-side token revocation required for MVP.
```

---

# 11. User Management API

User management endpoints are primarily for Admin users.

## 11.1 Create User

```http
POST /api/users
```

Required authorization:

```text
Admin
users.manage
```

Request body:

```json
{
  "email": "supervisor@example.com",
  "firstName": "Laura",
  "lastName": "Torres",
  "displayName": "Laura Torres",
  "temporaryPassword": "TempPassword123!",
  "roleIds": [
    "d7088530-f1c1-4f35-9992-efcb89189f11"
  ],
  "scopes": [
    {
      "scopeType": "Account",
      "accountId": "00b3cf55-86b9-4b68-b8d8-7fc4e7bb2042"
    }
  ]
}
```

Response:

```json
{
  "data": {
    "id": "6a7df18b-3f54-4121-8372-68228be1147f",
    "message": "User created successfully."
  }
}
```

Audit:

```text
UserCreated
RoleAssigned
ScopeAssigned
```

---

## 11.2 Get Users

```http
GET /api/users
```

Required authorization:

```text
Admin
```

Query parameters:

```text
role
isActive
search
page
pageSize
```

Example:

```http
GET /api/users?role=Agent&isActive=true&page=1&pageSize=25
```

Response:

```json
{
  "data": [
    {
      "id": "6a7df18b-3f54-4121-8372-68228be1147f",
      "email": "supervisor@example.com",
      "displayName": "Laura Torres",
      "roles": [
        "Supervisor"
      ],
      "isActive": true
    }
  ],
  "page": 1,
  "pageSize": 25,
  "totalCount": 1,
  "totalPages": 1
}
```

---

## 11.3 Get User by Id

```http
GET /api/users/{id}
```

Required authorization:

```text
Admin
```

---

## 11.4 Update User

```http
PUT /api/users/{id}
```

Required authorization:

```text
Admin
users.manage
```

Request body:

```json
{
  "firstName": "Laura",
  "lastName": "Torres",
  "displayName": "Laura Torres",
  "isActive": true
}
```

Audit:

```text
UserUpdated
```

---

## 11.5 Assign User Roles

```http
PUT /api/users/{id}/roles
```

Required authorization:

```text
Admin
roles.manage
```

Request body:

```json
{
  "roleIds": [
    "d7088530-f1c1-4f35-9992-efcb89189f11"
  ]
}
```

Audit:

```text
RoleAssigned
RoleRemoved
```

---

## 11.6 Assign User Scope

```http
PUT /api/users/{id}/scopes
```

Required authorization:

```text
Admin
organization.manage
```

Request body:

```json
{
  "scopes": [
    {
      "scopeType": "Region",
      "regionId": "f26eaf77-6751-4130-9db2-30ec8d4740b0"
    }
  ]
}
```

Audit:

```text
ScopeAssigned
ScopeUpdated
```

---

## 11.7 Deactivate User

```http
POST /api/users/{id}/deactivate
```

Required authorization:

```text
Admin
users.manage
```

Request body:

```json
{
  "reason": "User no longer belongs to the operation."
}
```

Audit:

```text
UserDeactivated
```

Business rules:

```text
Deactivated users cannot authenticate.
Deactivation must preserve historical tickets, comments, assignments, and audit records.
```

---

# 12. Role and Permission API

## 12.1 Get Roles

```http
GET /api/roles
```

Required authorization:

```text
Admin
```

---

## 12.2 Get Permissions

```http
GET /api/permissions
```

Required authorization:

```text
Admin
```

---

## 12.3 Update Role Permissions

```http
PUT /api/roles/{id}/permissions
```

Required authorization:

```text
Admin
roles.manage
```

Request body:

```json
{
  "permissionIds": [
    "4b91f308-fef4-4da4-b70c-900b4987caa0"
  ]
}
```

Audit:

```text
RolePermissionsUpdated
```

---

# 13. Operational Structure API

Operational structure endpoints are primarily for Admin users.

---

## 13.1 Regions

### Create Region

```http
POST /api/regions
```

Required authorization:

```text
Admin
organization.manage
```

Request body:

```json
{
  "name": "Latin America",
  "code": "LATAM"
}
```

Audit:

```text
RegionCreated
```

---

### Get Regions

```http
GET /api/regions
```

Required authorization:

```text
Authenticated user
```

Query parameters:

```text
isActive
search
page
pageSize
```

---

### Get Region by Id

```http
GET /api/regions/{id}
```

Required authorization:

```text
Authenticated user
```

---

### Update Region

```http
PUT /api/regions/{id}
```

Required authorization:

```text
Admin
organization.manage
```

Audit:

```text
RegionUpdated
```

---

### Deactivate Region

```http
POST /api/regions/{id}/deactivate
```

Required authorization:

```text
Admin
organization.manage
```

Audit:

```text
RegionDeactivated
```

---

## 13.2 Countries

### Create Country

```http
POST /api/countries
```

Required authorization:

```text
Admin
organization.manage
```

Request body:

```json
{
  "regionId": "f26eaf77-6751-4130-9db2-30ec8d4740b0",
  "name": "Mexico",
  "code": "MX"
}
```

Audit:

```text
CountryCreated
```

---

### Get Countries

```http
GET /api/countries
```

Required authorization:

```text
Authenticated user
```

Query parameters:

```text
regionId
isActive
search
page
pageSize
```

---

### Update Country

```http
PUT /api/countries/{id}
```

Required authorization:

```text
Admin
organization.manage
```

Audit:

```text
CountryUpdated
```

---

### Deactivate Country

```http
POST /api/countries/{id}/deactivate
```

Required authorization:

```text
Admin
organization.manage
```

Audit:

```text
CountryDeactivated
```

---

## 13.3 Accounts

### Create Account

```http
POST /api/accounts
```

Required authorization:

```text
Admin
organization.manage
```

Request body:

```json
{
  "countryId": "a39b80ef-e4ca-4909-bc2f-d72682d47a51",
  "name": "NovaBank",
  "code": "NOVABANK",
  "description": "Fictional financial services account."
}
```

Audit:

```text
AccountCreated
```

---

### Get Accounts

```http
GET /api/accounts
```

Required authorization:

```text
Authenticated user
```

Query parameters:

```text
countryId
regionId
isActive
search
page
pageSize
```

Scope behavior:

```text
Results should be filtered by user role and operational scope.
```

---

### Get Account by Id

```http
GET /api/accounts/{id}
```

Required authorization:

```text
Authenticated user
```

Scope behavior:

```text
User must have access to the account or its parent scope.
```

---

### Update Account

```http
PUT /api/accounts/{id}
```

Required authorization:

```text
Admin
organization.manage
```

Audit:

```text
AccountUpdated
```

---

### Deactivate Account

```http
POST /api/accounts/{id}/deactivate
```

Required authorization:

```text
Admin
organization.manage
```

Audit:

```text
AccountDeactivated
```

---

## 13.4 Campaigns

### Create Campaign

```http
POST /api/campaigns
```

Required authorization:

```text
Admin
organization.manage
```

Request body:

```json
{
  "accountId": "00b3cf55-86b9-4b68-b8d8-7fc4e7bb2042",
  "countryId": "a39b80ef-e4ca-4909-bc2f-d72682d47a51",
  "name": "Credit Card Support",
  "code": "CC-SUPPORT",
  "description": "Credit card support campaign."
}
```

Audit:

```text
CampaignCreated
```

---

### Get Campaigns

```http
GET /api/campaigns
```

Required authorization:

```text
Authenticated user
```

Query parameters:

```text
accountId
countryId
regionId
isActive
search
page
pageSize
```

Scope behavior:

```text
Results should be filtered by user role and operational scope.
```

---

### Get Campaign by Id

```http
GET /api/campaigns/{id}
```

Required authorization:

```text
Authenticated user
```

---

### Update Campaign

```http
PUT /api/campaigns/{id}
```

Required authorization:

```text
Admin
organization.manage
```

Audit:

```text
CampaignUpdated
```

---

### Deactivate Campaign

```http
POST /api/campaigns/{id}/deactivate
```

Required authorization:

```text
Admin
organization.manage
```

Audit:

```text
CampaignDeactivated
```

---

# 14. Customer API

## 14.1 Create Customer

```http
POST /api/customers
```

Required authorization:

```text
Admin
Supervisor
Agent
customers.manage or tickets.create
```

Request body:

```json
{
  "accountId": "00b3cf55-86b9-4b68-b8d8-7fc4e7bb2042",
  "firstName": "Maria",
  "lastName": "Gonzalez",
  "email": "maria.gonzalez@example.com",
  "phoneNumber": "+52 8180000000",
  "externalReference": "CUST-001"
}
```

Response:

```json
{
  "data": {
    "id": "fdb83553-99c7-4d4e-aa1c-a0617fdf4f4e",
    "message": "Customer created successfully."
  }
}
```

Audit:

```text
CustomerCreated
```

---

## 14.2 Get Customers

```http
GET /api/customers
```

Required authorization:

```text
Authenticated user with customer visibility
```

Query parameters:

```text
accountId
search
email
externalReference
isActive
page
pageSize
```

Scope behavior:

```text
Customer records must be filtered by user role and operational scope.
```

---

## 14.3 Get Customer by Id

```http
GET /api/customers/{id}
```

Required authorization:

```text
Authenticated user with customer visibility
```

Scope behavior:

```text
User must have access to the customer's account scope.
```

---

## 14.4 Update Customer

```http
PUT /api/customers/{id}
```

Required authorization:

```text
Admin
Supervisor
Agent with scope
```

Audit:

```text
CustomerUpdated
```

---

## 14.5 Get Customer Ticket History

```http
GET /api/customers/{id}/tickets
```

Required authorization:

```text
Authenticated user with customer visibility
```

Query parameters:

```text
status
priority
slaState
page
pageSize
```

Scope behavior:

```text
Only tickets within the user's authorized scope should be returned.
```

---

# 15. Ticket API

Ticket endpoints support the main ticket lifecycle.

---

## 15.1 Create Ticket

```http
POST /api/tickets
```

Required authorization:

```text
Agent
Supervisor
tickets.create
```

Request body:

```json
{
  "customerId": "fdb83553-99c7-4d4e-aa1c-a0617fdf4f4e",
  "accountId": "00b3cf55-86b9-4b68-b8d8-7fc4e7bb2042",
  "campaignId": "70c5f763-94d7-4a3a-b16d-57a0ac5d9f2a",
  "category": "Billing Dispute",
  "priority": "High",
  "subject": "Incorrect credit card charge",
  "description": "Customer reports an incorrect credit card charge."
}
```

Response:

```json
{
  "data": {
    "id": "e633d243-1448-4011-af81-9de2e32274ec",
    "ticketNumber": "OPS-000145",
    "status": "Open",
    "priority": "High",
    "slaState": "Within SLA",
    "message": "Ticket created successfully."
  }
}
```

Business rules:

```text
Ticket must be linked to a customer.
Ticket must be linked to an account.
Ticket must be linked to a campaign.
Ticket must include category, priority, subject, and description.
User must have permission within the selected operational scope.
Ticket creation must initialize SLA tracking.
Ticket creation must create an audit record.
```

Audit:

```text
TicketCreated
SlaStateChanged when applicable
```

---

## 15.2 Get Tickets

```http
GET /api/tickets
```

Required authorization:

```text
Authenticated user with ticket visibility
```

Query parameters:

```text
regionId
countryId
accountId
campaignId
customerId
assignedAgentUserId
supervisorUserId
status
priority
slaState
isEscalated
createdFrom
createdTo
search
page
pageSize
sortBy
sortDirection
```

Example:

```http
GET /api/tickets?accountId=00b3cf55-86b9-4b68-b8d8-7fc4e7bb2042&status=Open&page=1&pageSize=25
```

Scope behavior:

```text
Returned tickets must be filtered by role and operational scope.
```

Response:

```json
{
  "data": [
    {
      "id": "e633d243-1448-4011-af81-9de2e32274ec",
      "ticketNumber": "OPS-000145",
      "customerName": "Maria Gonzalez",
      "accountName": "NovaBank",
      "campaignName": "Credit Card Support",
      "assignedAgentName": "Ana Lopez",
      "supervisorName": "Laura Torres",
      "priority": "High",
      "status": "Open",
      "slaState": "Within SLA",
      "slaDueAt": "2026-05-13T18:00:00Z",
      "isEscalated": false,
      "createdAt": "2026-05-12T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 25,
  "totalCount": 1,
  "totalPages": 1
}
```

---

## 15.3 Get Ticket by Id

```http
GET /api/tickets/{id}
```

Required authorization:

```text
Authenticated user with ticket visibility
```

Scope behavior:

```text
User must have access to the ticket's operational context.
```

Response:

```json
{
  "data": {
    "id": "e633d243-1448-4011-af81-9de2e32274ec",
    "ticketNumber": "OPS-000145",
    "customer": {
      "id": "fdb83553-99c7-4d4e-aa1c-a0617fdf4f4e",
      "name": "Maria Gonzalez"
    },
    "account": {
      "id": "00b3cf55-86b9-4b68-b8d8-7fc4e7bb2042",
      "name": "NovaBank"
    },
    "campaign": {
      "id": "70c5f763-94d7-4a3a-b16d-57a0ac5d9f2a",
      "name": "Credit Card Support"
    },
    "assignedAgent": {
      "id": "4a73c3d8-4e7a-4fd7-9d68-ff0e042e6001",
      "name": "Ana Lopez"
    },
    "supervisor": {
      "id": "6a7df18b-3f54-4121-8372-68228be1147f",
      "name": "Laura Torres"
    },
    "category": "Billing Dispute",
    "priority": "High",
    "status": "Open",
    "subject": "Incorrect credit card charge",
    "description": "Customer reports an incorrect credit card charge.",
    "slaState": "Within SLA",
    "slaDueAt": "2026-05-13T18:00:00Z",
    "isEscalated": false,
    "createdAt": "2026-05-12T10:00:00Z",
    "updatedAt": null
  }
}
```

---

## 15.4 Update Ticket

```http
PUT /api/tickets/{id}
```

Required authorization:

```text
Supervisor
Agent with scope
tickets.update
```

Request body:

```json
{
  "category": "Billing Dispute",
  "priority": "High",
  "subject": "Incorrect credit card charge",
  "description": "Updated ticket description."
}
```

Business rules:

```text
Closed tickets cannot be modified unless reopening is explicitly allowed.
Priority changes must be authorized.
Important changes must be audited.
```

Audit:

```text
TicketUpdated
TicketPriorityChanged when priority changes
```

---

## 15.5 Assign Ticket

```http
POST /api/tickets/{id}/assign
```

Required authorization:

```text
Supervisor
tickets.assign
```

Request body:

```json
{
  "assignedAgentUserId": "4a73c3d8-4e7a-4fd7-9d68-ff0e042e6001",
  "assignmentReason": "Agent has capacity and belongs to the correct campaign."
}
```

Response:

```json
{
  "data": {
    "id": "e633d243-1448-4011-af81-9de2e32274ec",
    "status": "Assigned",
    "assignedAgentUserId": "4a73c3d8-4e7a-4fd7-9d68-ff0e042e6001",
    "message": "Ticket assigned successfully."
  }
}
```

Business rules:

```text
Ticket can only be assigned to an eligible active agent.
Agent must belong to the correct account or campaign scope.
Supervisor can only assign tickets within assigned scope.
Closed tickets cannot be assigned unless reopening is allowed.
Assignment must be recorded in ticket history and audit history.
```

Audit:

```text
TicketAssigned
TicketReassigned
TicketStatusChanged when status changes to Assigned
```

---

## 15.6 Update Ticket Status

```http
PUT /api/tickets/{id}/status
```

Required authorization:

```text
Agent
Supervisor
tickets.update_status
```

Request body:

```json
{
  "status": "In Progress",
  "reason": "Agent started working on the ticket."
}
```

Business rules:

```text
Status transition must be valid.
Users can only update tickets within operational scope.
Closed ticket restrictions must be enforced.
Status changes must be recorded in ticket history and audit history.
```

Audit:

```text
TicketStatusChanged
```

---

## 15.7 Add Ticket Comment

```http
POST /api/tickets/{id}/comments
```

Required authorization:

```text
Agent
Supervisor
OperationsManager
tickets.comment
```

Request body:

```json
{
  "body": "Customer provided additional transaction details."
}
```

Response:

```json
{
  "data": {
    "id": "1c0bb817-f87e-4a71-bd35-3556d39d768a",
    "ticketId": "e633d243-1448-4011-af81-9de2e32274ec",
    "body": "Customer provided additional transaction details.",
    "authorName": "Ana Lopez",
    "createdAt": "2026-05-12T12:30:00Z"
  }
}
```

Business rules:

```text
Internal comments must not be empty.
Internal comments must not be exposed to customers.
Comment visibility must respect role and scope.
Comment creation must be audited.
```

Audit:

```text
TicketCommentAdded
```

---

## 15.8 Get Ticket Comments

```http
GET /api/tickets/{id}/comments
```

Required authorization:

```text
Authenticated internal user with ticket visibility
```

Scope behavior:

```text
Only authorized internal users may view internal comments.
```

---

## 15.9 Escalate Ticket

```http
POST /api/tickets/{id}/escalate
```

Required authorization:

```text
Agent
Supervisor
tickets.escalate
```

Request body:

```json
{
  "escalationReason": "Customer impact is high and SLA breach risk is increasing."
}
```

Response:

```json
{
  "data": {
    "id": "e633d243-1448-4011-af81-9de2e32274ec",
    "status": "Escalated",
    "isEscalated": true,
    "message": "Ticket escalated successfully."
  }
}
```

Business rules:

```text
Escalation reason is required.
Closed tickets cannot be escalated.
User must have permission and scope.
Escalated ticket must be visible to authorized supervisors and managers.
Escalation must be recorded in ticket history and audit history.
```

Audit:

```text
TicketEscalated
TicketStatusChanged when status changes to Escalated
```

---

## 15.10 Resolve Ticket

```http
POST /api/tickets/{id}/resolve
```

Required authorization:

```text
Agent
Supervisor
tickets.resolve
```

Request body:

```json
{
  "resolutionSummary": "Charge was reviewed and corrected.",
  "resolutionCode": "BillingCorrected"
}
```

Response:

```json
{
  "data": {
    "id": "e633d243-1448-4011-af81-9de2e32274ec",
    "status": "Resolved",
    "message": "Ticket resolved successfully."
  }
}
```

Business rules:

```text
Ticket must be in a status that allows resolution.
User must have permission and scope.
Resolution event must preserve final SLA outcome.
Resolution must be recorded in ticket history and audit history.
```

Audit:

```text
TicketResolved
TicketStatusChanged
SlaStateChanged when applicable
```

---

## 15.11 Close Ticket

```http
POST /api/tickets/{id}/close
```

Required authorization:

```text
Agent
Supervisor
tickets.close
```

Request body:

```json
{
  "closureNote": "Ticket confirmed and closed."
}
```

Business rules:

```text
Ticket must be resolved before it can be closed.
Closed ticket must become restricted from normal modification.
Closure must preserve final SLA outcome.
Closure must be recorded in ticket history and audit history.
```

Audit:

```text
TicketClosed
TicketStatusChanged
```

---

## 15.12 Get Ticket History

```http
GET /api/tickets/{id}/history
```

Required authorization:

```text
Authenticated internal user with ticket visibility
```

Returns:

```text
Status history
Assignment history
Escalation history
Resolution summary
Audit-related lifecycle events
```

---

# 16. SLA API

## 16.1 Get SLA Dashboard

```http
GET /api/sla/dashboard
```

Required authorization:

```text
OperationsManager
Supervisor
Agent
Viewer
sla.view
dashboard.view
```

Query parameters:

```text
regionId
countryId
accountId
campaignId
supervisorUserId
agentUserId
priority
status
createdFrom
createdTo
```

Response:

```json
{
  "data": {
    "openTickets": 42,
    "overdueTickets": 7,
    "atRiskTickets": 12,
    "withinSlaTickets": 23,
    "completedTickets": 15,
    "ticketsBySlaState": [
      {
        "slaState": "Within SLA",
        "count": 23
      },
      {
        "slaState": "At Risk",
        "count": 12
      },
      {
        "slaState": "Breached",
        "count": 7
      }
    ]
  }
}
```

Scope behavior:

```text
Dashboard data must be filtered by user role and operational scope.
```

---

## 16.2 Get At-Risk Tickets

```http
GET /api/sla/at-risk
```

Required authorization:

```text
Authenticated user with SLA visibility
```

---

## 16.3 Get Breached Tickets

```http
GET /api/sla/breached
```

Required authorization:

```text
Authenticated user with SLA visibility
```

---

## 16.4 Get SLA Policies

```http
GET /api/sla/policies
```

Required authorization:

```text
Admin
Supervisor
OperationsManager
```

---

## 16.5 Create SLA Policy

```http
POST /api/sla/policies
```

Required authorization:

```text
Admin
organization.manage
```

Request body:

```json
{
  "accountId": "00b3cf55-86b9-4b68-b8d8-7fc4e7bb2042",
  "campaignId": "70c5f763-94d7-4a3a-b16d-57a0ac5d9f2a",
  "priority": "High",
  "targetHours": 8,
  "atRiskThresholdPercent": 80
}
```

Audit:

```text
SlaPolicyCreated
```

---

# 17. Dashboard API

## 17.1 Get Operational Dashboard

```http
GET /api/dashboard/operations
```

Required authorization:

```text
OperationsManager
Supervisor
Agent
Viewer
dashboard.view
```

Query parameters:

```text
regionId
countryId
accountId
campaignId
supervisorUserId
agentUserId
createdFrom
createdTo
```

Response:

```json
{
  "data": {
    "openTickets": 42,
    "closedTickets": 18,
    "overdueTickets": 7,
    "escalatedTickets": 5,
    "ticketsByStatus": [
      {
        "status": "Open",
        "count": 10
      },
      {
        "status": "In Progress",
        "count": 20
      }
    ],
    "ticketsByPriority": [
      {
        "priority": "High",
        "count": 9
      }
    ]
  }
}
```

Scope behavior:

```text
Dashboard metrics must respect user role and operational scope.
```

---

## 17.2 Get Agent Workload

```http
GET /api/dashboard/agent-workload
```

Required authorization:

```text
OperationsManager
Supervisor
dashboard.view
```

Query parameters:

```text
accountId
campaignId
supervisorUserId
```

---

## 17.3 Get Supervisor Workload

```http
GET /api/dashboard/supervisor-workload
```

Required authorization:

```text
OperationsManager
dashboard.view
```

Query parameters:

```text
regionId
countryId
accountId
```

---

# 18. Reports API

Reports are basic operational exports and filtered data endpoints.

OpsSphere does not replace Power BI.

---

## 18.1 Get Ticket Report

```http
GET /api/reports/tickets
```

Required authorization:

```text
OperationsManager
Supervisor
Viewer
reports.view
```

Query parameters:

```text
regionId
countryId
accountId
campaignId
status
priority
slaState
createdFrom
createdTo
page
pageSize
```

---

## 18.2 Export Ticket Report CSV

```http
GET /api/reports/tickets/export
```

Required authorization:

```text
OperationsManager
Supervisor
Viewer
reports.view
```

Response content type:

```http
text/csv
```

Business rule:

```text
Exports must respect role and operational scope.
```

Audit:

```text
ReportExported when export auditing is enabled
```

---

## 18.3 Get SLA Report

```http
GET /api/reports/sla
```

Required authorization:

```text
OperationsManager
Supervisor
Viewer
reports.view
```

---

## 18.4 Export SLA Report CSV

```http
GET /api/reports/sla/export
```

Required authorization:

```text
OperationsManager
Supervisor
Viewer
reports.view
```

Response content type:

```http
text/csv
```

---

# 19. Audit API

## 19.1 Get Audit Logs

```http
GET /api/audit-logs
```

Required authorization:

```text
Admin
OperationsManager
Supervisor
Viewer
audit.view
```

Query parameters:

```text
actorUserId
entityType
entityId
action
createdFrom
createdTo
page
pageSize
```

Scope behavior:

```text
Audit logs must be filtered by role and operational scope when related to operational data.
Admin users may view administrative audit records according to platform permissions.
```

Response:

```json
{
  "data": [
    {
      "id": "2a2aa2a9-9098-4d28-87d2-1303681c1e2e",
      "actorUserId": "4a73c3d8-4e7a-4fd7-9d68-ff0e042e6001",
      "actorName": "Ana Lopez",
      "action": "TicketStatusChanged",
      "entityType": "Ticket",
      "entityId": "e633d243-1448-4011-af81-9de2e32274ec",
      "previousValue": "Open",
      "newValue": "In Progress",
      "createdAt": "2026-05-12T12:45:00Z"
    }
  ],
  "page": 1,
  "pageSize": 25,
  "totalCount": 1,
  "totalPages": 1
}
```

---

## 19.2 Get Audit Logs for Entity

```http
GET /api/audit-logs/entity/{entityType}/{entityId}
```

Required authorization:

```text
Authenticated user with audit visibility
```

Example:

```http
GET /api/audit-logs/entity/Ticket/e633d243-1448-4011-af81-9de2e32274ec
```

---

# 20. Assignment Lookup API

These endpoints support frontend dropdowns and assignment workflows.

---

## 20.1 Get Eligible Agents for Ticket

```http
GET /api/tickets/{id}/eligible-agents
```

Required authorization:

```text
Supervisor
tickets.assign
```

Business rules:

```text
Only active agents within the ticket's account or campaign scope should be returned.
```

---

## 20.2 Get Agents by Campaign

```http
GET /api/campaigns/{id}/agents
```

Required authorization:

```text
Admin
Supervisor
OperationsManager
```

Scope behavior:

```text
Result must respect user role and scope.
```

---

## 20.3 Get Supervisors by Account

```http
GET /api/accounts/{id}/supervisors
```

Required authorization:

```text
Admin
OperationsManager
```

---

# 21. Health Endpoints

Health checks are operational readiness and liveness endpoints, not business API resources.

They intentionally do not use the `/api` base path.

## 21.1 Health Check

```http
GET /health
```

Optional detailed endpoint:

```http
GET /health/details
```

Required authorization:

```text
Anonymous or internal monitoring
```

Response:

```json
{
  "status": "Healthy",
  "timestamp": "2026-05-12T12:00:00Z"
}
```

---

# 22. Endpoint Summary

## 22.1 Authentication

```http
POST /api/auth/login
GET /api/auth/me
POST /api/auth/logout
```

---

## 22.2 Users, Roles, and Permissions

```http
POST /api/users
GET /api/users
GET /api/users/{id}
PUT /api/users/{id}
PUT /api/users/{id}/roles
PUT /api/users/{id}/scopes
POST /api/users/{id}/deactivate

GET /api/roles
GET /api/permissions
PUT /api/roles/{id}/permissions
```

---

## 22.3 Operational Structure

```http
POST /api/regions
GET /api/regions
GET /api/regions/{id}
PUT /api/regions/{id}
POST /api/regions/{id}/deactivate

POST /api/countries
GET /api/countries
PUT /api/countries/{id}
POST /api/countries/{id}/deactivate

POST /api/accounts
GET /api/accounts
GET /api/accounts/{id}
PUT /api/accounts/{id}
POST /api/accounts/{id}/deactivate

POST /api/campaigns
GET /api/campaigns
GET /api/campaigns/{id}
PUT /api/campaigns/{id}
POST /api/campaigns/{id}/deactivate
```

---

## 22.4 Customers

```http
POST /api/customers
GET /api/customers
GET /api/customers/{id}
PUT /api/customers/{id}
GET /api/customers/{id}/tickets
```

---

## 22.5 Tickets

```http
POST /api/tickets
GET /api/tickets
GET /api/tickets/{id}
PUT /api/tickets/{id}
POST /api/tickets/{id}/assign
PUT /api/tickets/{id}/status
POST /api/tickets/{id}/comments
GET /api/tickets/{id}/comments
POST /api/tickets/{id}/escalate
POST /api/tickets/{id}/resolve
POST /api/tickets/{id}/close
GET /api/tickets/{id}/history
GET /api/tickets/{id}/eligible-agents
```

---

## 22.6 SLA

```http
GET /api/sla/dashboard
GET /api/sla/at-risk
GET /api/sla/breached
GET /api/sla/policies
POST /api/sla/policies
```

---

## 22.7 Dashboards

```http
GET /api/dashboard/operations
GET /api/dashboard/agent-workload
GET /api/dashboard/supervisor-workload
```

---

## 22.8 Reports

```http
GET /api/reports/tickets
GET /api/reports/tickets/export
GET /api/reports/sla
GET /api/reports/sla/export
```

---

## 22.9 Audit

```http
GET /api/audit-logs
GET /api/audit-logs/entity/{entityType}/{entityId}
```

---

## 22.10 Health

```http
GET /health
GET /health/details
```

---

# 23. Controller Mapping

The initial ASP.NET Core Web API controllers may be organized as:

```text
AuthController
UsersController
RolesController
PermissionsController
RegionsController
CountriesController
AccountsController
CampaignsController
CustomersController
TicketsController
SlaController
DashboardController
ReportsController
AuditLogsController
HealthController
```

Controllers should remain thin.

Controllers should delegate behavior to:

```text
Commands
Queries
Handlers
Validators
Authorization policies
Application services
Domain entities
Domain services
```

---

# 24. CQRS Mapping

## 24.1 Example Commands

```text
LoginCommand
CreateUserCommand
AssignUserRolesCommand
AssignUserScopesCommand
DeactivateUserCommand
CreateRegionCommand
CreateCountryCommand
CreateAccountCommand
CreateCampaignCommand
CreateCustomerCommand
CreateTicketCommand
AssignTicketCommand
UpdateTicketStatusCommand
AddTicketCommentCommand
EscalateTicketCommand
ResolveTicketCommand
CloseTicketCommand
CreateSlaPolicyCommand
```

---

## 24.2 Example Queries

```text
GetCurrentUserQuery
GetUsersQuery
GetRegionsQuery
GetAccountsQuery
GetCampaignsQuery
GetCustomersQuery
GetCustomerTicketsQuery
GetTicketsQuery
GetTicketByIdQuery
GetTicketCommentsQuery
GetTicketHistoryQuery
GetEligibleAgentsQuery
GetSlaDashboardQuery
GetOperationalDashboardQuery
GetTicketReportQuery
GetAuditLogsQuery
```

---

# 25. API Validation Rules

API validation should enforce:

```text
Required fields
Valid enum values
Valid identifiers
Valid role and scope combinations
Valid ticket status transitions
Valid assignment eligibility
Valid escalation reason
Valid resolution state
Valid closure state
Valid pagination parameters
```

Examples:

```text
Ticket priority must be Low, Normal, High, or Critical.
Ticket status must be one of the supported workflow statuses.
Escalation reason is required.
Ticket must be resolved before closure.
User must be authorized for selected account or campaign.
```

---

# 26. Security Rules

The API must enforce:

```text
Authentication before authorization.
Protected endpoints require JWT.
Backend authorization independent from frontend visibility.
Role-based access.
Scope-based filtering.
Least privilege.
No unauthorized data exposure.
Generic login failure messages.
No password or token logging.
Audit records for critical actions.
```

---

# 27. Pagination and Filtering

List endpoints should support pagination.

Common query parameters:

```text
page
pageSize
sortBy
sortDirection
search
```

Recommended defaults:

```text
page = 1
pageSize = 25
maxPageSize = 100
```

Invalid pagination values should return validation errors.

---

# 28. Versioning Strategy

The MVP may start without explicit API versioning.

Recommended initial base path:

```text
/api
```

Future versioned path if needed:

```text
/api/v1
```

Versioning can be introduced before public or external API consumers depend on the endpoints.

---

# 29. OpenAPI / Swagger

The ASP.NET Core Web API should expose OpenAPI documentation during development.

Recommended development endpoint:

```http
/swagger
```

Swagger should document:

```text
Request bodies
Response bodies
Status codes
Authentication requirements
Validation errors
Endpoint descriptions
```

Swagger should not expose sensitive production-only details.

---

# 30. Out of Scope for Initial API

The following API areas are out of scope for the MVP:

```text
External customer portal APIs
Telephony APIs
Omnichannel messaging APIs
AI ticket classification APIs
Predictive SLA APIs
Payroll APIs
Workforce management APIs
Production enterprise SSO APIs
Billing APIs
Advanced Power BI embedded APIs
Complex workflow automation APIs
Native mobile APIs
```

---

# 31. Related Documents

| Document | Relationship |
|---|---|
| `docs/06-requirements.md` | Defines functional requirements implemented by API endpoints. |
| `docs/07-use-cases.md` | Defines user workflows supported by API actions. |
| `docs/08-business-process-flows.md` | Defines process behavior behind ticket, SLA, escalation, and user access endpoints. |
| `docs/09-business-rules.md` | Defines validation, authorization, workflow, and audit rules enforced by the API. |
| `docs/10-domain-model.md` | Defines domain concepts exposed through API resources. |
| `docs/11-architecture-overview.md` | Defines ASP.NET Core Web API, Clean Architecture, CQRS, and MediatR direction. |
| `docs/12-c4-architecture.md` | Shows the API as a central container and backend component. |
| `docs/13-uml-diagrams.md` | Defines sequence diagrams for login, ticket creation, and escalation. |
| `docs/14-database-design.md` | Defines tables and relationships read and written by API endpoints. |
| `docs/16-security-and-permissions.md` | Defines authentication, authorization, roles, permissions, and audit-sensitive actions. |

---

# 32. Document Summary

This document defines the initial API design for OpsSphere.

The API is designed for an ASP.NET Core Web API backend using Clean Architecture and CQRS with MediatR.

The endpoints support authentication, user management, roles, permissions, operational structure, customers, ticket lifecycle, internal comments, escalations, SLA visibility, dashboards, reports, audit history, assignment lookups, and health checks.

The API design prioritizes clear resource boundaries, role-based authorization, scope-based filtering, auditability, predictable request and response shapes, and implementation readiness for the Angular frontend.
