# OpsSphere Frontend Context

## Purpose

This document provides compressed frontend context for agents working inside OpsSphere.

Use this file when the task touches:

- Angular
- TypeScript
- Routing
- Forms
- Guards
- HTTP services
- JWT handling
- Role-aware UI
- Scope-aware UI
- Frontend testing

---

## Frontend Stack

OpsSphere frontend uses:

```text
Angular
TypeScript
RxJS
Reactive Forms
Angular Router
Route Guards
HTTP Interceptors
Angular Material or Tailwind
```

The frontend communicates with the backend through REST API calls exposed by the ASP.NET Core Web API.

---

## Frontend Role

The frontend provides the user experience for:

```text
Login
Dashboard
Ticket queues
Ticket detail
Ticket creation
Customer management
User management
Operational structure management
SLA views
Escalation views
Audit history views
Reports or exports
```

---

## Recommended Structure

```text
frontend/
  src/
    app/
      core/
        auth/
        guards/
        interceptors/
        services/

      shared/
        components/
        pipes/
        models/

      features/
        login/
        dashboard/
        tickets/
        customers/
        users/
        organization/
        sla/
        audit/
        reports/
```

---

## Security Boundary Rule

Frontend role and scope checks improve UX.

They are not the security boundary.

```text
Frontend may hide unavailable actions.
Frontend may protect routes for usability.
Frontend may avoid showing irrelevant data.

But backend authorization is the source of truth.
```

Never assume a hidden button means the action is secure.

---

## Authentication Frontend Responsibilities

Frontend should support:

```text
Login form
Token storage according to selected security strategy
Attach JWT token to protected API requests
Redirect unauthenticated users to login
Handle expired token responses
Fetch current user profile
Track roles, permissions, and scopes for UI behavior
Logout by removing token in MVP
```

Expected API calls:

```http
POST /api/auth/login
GET /api/auth/me
POST /api/auth/logout
```

MVP logout behavior:

```text
Frontend removes token.
Protected requests without token are rejected by backend.
```

---

## HTTP Interceptor Responsibilities

HTTP interceptor may:

```text
Attach Authorization: Bearer {token}
Attach X-Correlation-Id when needed
Handle 401 Unauthorized
Handle 403 Forbidden
Route expired session to login
Preserve correlation ID from responses when useful
```

---

## Route Guard Responsibilities

Route guards may:

```text
Block unauthenticated routes.
Check role requirements.
Check permission requirements.
Redirect unauthorized users.
Prevent viewers from entering write modules.
```

Route guards must not replace backend authorization.

---

## Role-Aware UX

Frontend should show actions according to role and permission.

Examples:

```text
Admin:
  Show user management and organization management.

Operations Manager:
  Show regional dashboards, reports, SLA trends, audit views.

Supervisor:
  Show ticket queues, assignment actions, escalation queues, SLA dashboards.

Agent:
  Show assigned tickets, create ticket, update status, comment, escalate, resolve.

Viewer:
  Show dashboards, reports, audit history, ticket read-only views.
  Hide create/update/assign/escalate/resolve/close actions.
```

---

## Scope-Aware UX

Frontend should pass filters and display user-relevant data.

Examples:

```text
Manager:
  Region-level data.

Supervisor:
  Account or campaign-level data.

Agent:
  Assigned tickets or assigned campaign data.

Viewer:
  Read-only scoped data.
```

Backend must still enforce scope.

---

## Forms

Use Reactive Forms for:

```text
Login
Ticket creation
Ticket status update
Ticket assignment
Internal comments
Escalation reason
Resolution details
Customer management
User management
Operational structure management
```

Form validation should reflect backend rules but not replace them.

Important form validations:

```text
Required fields
Valid email
Valid role
Valid scope
Required ticket customer
Required account
Required campaign
Required priority
Required category
Required description
Required escalation reason
Non-empty internal comment
Required resolution before closure
```

---

## Error Handling

Frontend should:

```text
Display clear validation messages.
Redirect unauthenticated users to login.
Show access denied messages for forbidden actions.
Show safe generic messages for unexpected errors.
Preserve and display correlation ID when useful.
Avoid exposing raw technical exceptions.
```

Expected error categories:

```text
400 validation or business rule error
401 unauthenticated
403 forbidden
404 not found
409 conflict
500 unexpected error
```

---

## Dashboard UX

Initial dashboard metrics may include:

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

Dashboards must respect backend-filtered data.

---

## Frontend Testing Priorities

Test:

```text
Login behavior
Route guards
HTTP interceptor token attachment
401/403 handling
Role-aware navigation
Viewer read-only behavior
Ticket creation validation
Escalation reason validation
Comment validation
Dashboard filter behavior
Service API calls
```

---

## Frontend Non-Goals

Do not add these unless explicitly requested:

```text
Real-time chat
Mobile app
Customer portal
Advanced BI authoring
Telephony controls
Omnichannel inbox
AI ticket classification UI
Complex notification center for MVP
```