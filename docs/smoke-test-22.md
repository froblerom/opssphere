# Sprint 22 Manual Smoke Test - Angular MVP Workflow Integration

> **Note:** Automated browser-level E2E tests are deferred to a future sprint. For MVP validation, this manual smoke checklist remains the authoritative E2E smoke reference. The API and integration layer are covered by the automated integration tests in `tests/OpsSphere.IntegrationTests`.

## Prerequisites
- [ ] Backend API is running locally.
- [ ] Angular frontend is running locally.
- [ ] Database migrations have been applied.
- [ ] Local demo seed data has been loaded.
- [ ] Only fictional seed/demo data is used.

## Seeded Personas
- [ ] Admin: `admin@opssphere.local` / `OpsSphere123!`
- [ ] Operations Manager: `manager.latam@opssphere.local` / `OpsSphere123!`
- [ ] Supervisor: `supervisor.novabank@opssphere.local` / `OpsSphere123!`
- [ ] Agent: `agent.novabank@opssphere.local` / `OpsSphere123!`
- [ ] Viewer: `viewer.latam@opssphere.local` / `OpsSphere123!`

## Startup Assumptions
- [ ] Backend is reachable from the frontend environment API base URL.
- [ ] Frontend environment configuration points to the local backend API.
- [ ] Browser developer tools can be used to confirm protected API calls include an `Authorization: Bearer ...` header after login.

## Smoke Test Checklist

### Authentication
- [ ] Admin can log in.
- [ ] Operations Manager can log in.
- [ ] Supervisor can log in.
- [ ] Agent can log in.
- [ ] Viewer can log in.
- [ ] Logout clears the session and returns the user to login.
- [ ] Protected routes redirect unauthenticated users to login.
- [ ] Protected API calls include the bearer token after login.

### Admin Workflow
- [ ] Admin can access user management.
- [ ] Admin can view roles and permissions.
- [ ] Admin can view and update scopes where supported.
- [ ] Admin can access organization management.
- [ ] Admin can open Customers from the main navigation.

### Supervisor Workflow
- [ ] Supervisor can view scoped NovaBank tickets.
- [ ] Supervisor can assign or reassign tickets where permitted.
- [ ] Supervisor can update ticket status where permitted.
- [ ] Supervisor can update ticket priority where permitted.
- [ ] Supervisor can view SLA state and ticket audit/history.
- [ ] Supervisor can use dashboard and ticket Region, Country, Account, and Campaign filters without typing raw UUIDs.

### Agent Workflow
- [ ] Agent can create a ticket within assigned scope.
- [ ] Agent can update ticket status where permitted.
- [ ] Agent can add an internal comment.
- [ ] Agent can escalate a ticket.
- [ ] Agent can resolve a ticket.
- [ ] Agent can close a ticket when permitted.
- [ ] Agent can complete the create -> assign -> status update -> comment -> escalate -> resolve -> close flow with fictional data.

### Viewer Workflow
- [ ] Viewer can access read-only scoped dashboard, ticket, customer, and audit views.
- [ ] Viewer cannot see write actions in the UI.
- [ ] Direct write attempts return forbidden from the API.
- [ ] Unauthorized route access displays the access denied page instead of exposing permission internals.

### Dashboard / SLA / Audit
- [ ] Dashboard widgets load with non-zero seeded metrics.
- [ ] Dashboard drill-ins navigate to `/tickets` with filtered query parameters.
- [ ] Ticket list applies dashboard drill-in filters.
- [ ] SLA badges are visible on ticket list/detail views.
- [ ] Seeded tickets demonstrate Within SLA, At Risk, Breached, and Completed SLA states.
- [ ] Audit log is visible to authorized users.
- [ ] Entity history panels load where available.

### Customers
- [ ] Customers navigation entry is visible to users with `customers.view`.
- [ ] Customer list loads scoped fictional customers.
- [ ] Customer detail loads the ticket history panel.
- [ ] Customer create and edit forms validate required fields and email format.

### Error and Access States
- [ ] 401 responses during an active session clear the token and redirect to login.
- [ ] 403 responses from direct API attempts remain enforced by the backend.
- [ ] Authenticated users without required route permissions see `/access-denied`.
- [ ] API errors display safe messages, not raw exceptions.
- [ ] Empty states are readable.
- [ ] Loading states appear during requests.

### MVP Scope Confirmation
- [ ] No full reports module is required for this smoke test.
- [ ] No export UI, Power BI integration, report builder, or advanced analytics is required.
- [ ] No external customer portal, notification center, attachment workflow, AI classification, predictive SLA, or enterprise SSO is required.
