# ADR-004: Use Angular for the Frontend

## Status

Accepted

## Context

OpsSphere needs a frontend application for internal operational users such as Admins, Operations Managers, Supervisors, Agents, and Viewers.

The frontend must support enterprise-style workflows, including:

```text
- Login
- Role-aware navigation
- Dashboards
- Ticket queues
- Ticket detail views
- Ticket creation forms
- Customer management
- User management
- Operational structure management
- SLA views
- Escalation views
- Audit history views
- Reports or exports
```

The frontend must work with an ASP.NET Core Web API backend and consume protected HTTP endpoints using JWT authentication.

The user interface must also support role-based and scope-aware behavior. For example, Viewers should not see write actions, Agents should only see ticket actions within their scope, and Admins should have access to administrative modules.

Because OpsSphere is intended to demonstrate enterprise full stack development, the frontend technology should be mature, structured, strongly typed, and commonly used in professional .NET environments.

## Decision

OpsSphere will use Angular as the primary frontend framework.

The frontend will be built with:

```text
- Angular
- TypeScript
- RxJS
- Reactive Forms
- Angular Router
- Route guards
- HTTP interceptors
- Angular Material or Tailwind for UI structure
```

Angular will communicate with the backend through REST API calls exposed by the ASP.NET Core Web API.

The frontend will use JWT authentication for protected API calls.

Frontend role and scope checks may be used to improve user experience, but backend authorization remains the source of truth.

## Rationale

Angular is a good fit for OpsSphere because the application has enterprise-style workflows, multiple user roles, protected routes, forms, dashboards, and structured modules.

OpsSphere is not a small landing page or simple static interface. It needs a frontend that can scale across multiple operational areas:

```text
- Authentication
- Tickets
- Customers
- Users
- Organization
- SLA
- Dashboards
- Reports
- Audit
```

Angular provides a strong project structure for organizing these features into modules, routes, guards, services, components, forms, and shared UI elements.

TypeScript also supports maintainability by making frontend models, API contracts, and component logic easier to reason about.

Angular is also common in enterprise .NET environments, which supports the portfolio goal of demonstrating senior-level full stack capability with .NET, Angular, SQL Server, and Azure-ready architecture.

## Consequences

### Positive Consequences

- Angular provides a structured frontend architecture.
- TypeScript improves maintainability and type safety.
- Angular Router supports protected routes and role-aware navigation.
- HTTP interceptors can attach JWT tokens to API requests.
- Reactive Forms support complex form validation for tickets, users, customers, and operational structure.
- Angular services provide a clean place to centralize API communication.
- Angular works well with ASP.NET Core Web API.
- The technology choice aligns with common enterprise .NET job requirements.
- The frontend can be organized by business features instead of becoming a single large UI layer.

### Negative Consequences

- Angular has more initial complexity than simpler frontend libraries.
- The project requires knowledge of Angular modules, routing, services, observables, forms, and dependency injection.
- RxJS can introduce complexity if overused.
- Angular may be heavier than necessary for a very small prototype.
- UI development may take longer at the beginning because structure and patterns must be established.
- Poor frontend organization could still lead to duplicated services, inconsistent state, or large components.

## Alternatives Considered

### React

React is a popular frontend library and could be used to build OpsSphere.

Rejected for this project because Angular better matches the enterprise .NET positioning of OpsSphere and provides a more opinionated structure out of the box.

React remains a valid option for other projects, especially when more flexibility or a lighter frontend architecture is preferred.

### Blazor

Blazor would allow the frontend to be built using C# and .NET.

Rejected for the initial version because OpsSphere is intended to demonstrate a full stack profile commonly requested in the market: ASP.NET Core Web API plus Angular.

Blazor may be considered in future projects, but Angular better supports the current portfolio objective.

### Razor Pages or MVC Views

Server-rendered ASP.NET Core MVC or Razor Pages could simplify some backend and frontend integration.

Rejected because OpsSphere is designed as a modern SPA plus API application. The frontend and backend should be separated so the API can serve Angular and future clients or integrations.

### Vue

Vue is simpler than Angular and can be effective for frontend applications.

Rejected because Angular is more aligned with the enterprise structure and .NET full stack positioning of OpsSphere.

Vue remains a valid alternative for smaller applications or teams that prefer a lighter framework.

## Implementation Notes

The frontend should follow a feature-oriented structure.

Recommended structure:

```text
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

The frontend should include:

```text
- Auth service for login and token handling
- HTTP interceptor for attaching JWT tokens
- Route guards for protected routes
- Role-aware navigation
- Shared API models
- Reusable UI components
- Reactive forms for create and update workflows
- Feature services for API communication
- Error handling for authorization and validation errors
```

The frontend must not be the only place where authorization is enforced.

This rule must always apply:

```text
Frontend visibility improves user experience.
Backend authorization protects the system.
```

Examples:

```text
- The frontend may hide the "Assign Ticket" button from Agents.
- The backend must still reject unauthorized assignment requests.
- The frontend may hide user management routes from non-Admins.
- The backend must still reject user management API calls from non-Admins.
- The frontend may filter navigation based on role.
- The backend must still filter data based on role and operational scope.
```

## Decision Scope

This ADR defines Angular as the primary frontend framework for OpsSphere.

It does not define the final UI design system, component library, state management strategy, visual theme, or detailed screen design.

Those details should be handled in frontend design and implementation tasks.