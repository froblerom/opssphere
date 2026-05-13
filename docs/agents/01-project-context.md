# OpsSphere Project Context

## Project Identity

OpsSphere is an enterprise-grade operations platform for multinational BPO/contact center environments.

It supports operational execution across:

- Regions
- Countries
- Accounts
- Campaigns
- Managers
- Supervisors
- Agents
- Viewers
- Customers
- Tickets
- SLAs
- Escalations
- Internal comments
- Audit history
- Dashboards
- Reports
- Role-based access
- Scope-based visibility

OpsSphere is not intended to be a generic ticketing system.

It is designed as an enterprise operations platform that connects:

```text
Business workflows
Organizational structure
Ticket ownership
SLA visibility
Escalation handling
Supervisor oversight
Auditability
Reporting-ready data
Modern software architecture
```

---

## Portfolio Purpose

OpsSphere is a senior-level full stack .NET portfolio project.

It must demonstrate:

- Business analysis
- Requirements analysis
- Process modeling
- Domain modeling
- Clean Architecture
- ASP.NET Core Web API
- Angular frontend
- SQL Server database design
- JWT authentication
- Role-based authorization
- Scope-based access control
- Audit logging
- Automated testing
- Dockerized local development
- CI/CD readiness
- Azure-ready deployment practices
- Operational observability and support readiness

---

## Target Business Environment

OpsSphere is modeled around a multinational BPO/contact center organization.

The business structure is:

```text
Multinational BPO / Contact Center
  → Region
    → Country
      → Account
        → Campaign
          → Supervisor
            → Agent
```

Tickets are created, assigned, updated, escalated, resolved, closed, audited, and reported within this operational context.

---

## Runtime Roles

OpsSphere supports the following runtime business roles:

| Role | Purpose |
|---|---|
| Admin | Manages users, roles, permissions, operational structure, and configuration. |
| Operations Manager | Oversees operational performance across assigned regions. |
| Supervisor | Manages tickets, agents, assignments, escalations, and SLA risk within assigned scope. |
| Agent | Handles tickets within assigned account, campaign, or ticket scope. |
| Viewer | Reviews tickets, dashboards, reports, and audit history in read-only mode. |

The following are not runtime business roles in the MVP:

| Actor | Reason |
|---|---|
| Customer | Customers are linked to tickets but do not directly access OpsSphere. |
| Technical Owner | Technical Owner is an implementation/project role, not a business runtime role. |

---

## MVP Scope

The MVP includes:

- Authentication
- JWT-based API access
- Role-based authorization
- Permission-based access
- Scope-based visibility
- User and role management
- Region management
- Country management
- Account management
- Campaign management
- Manager assignment
- Supervisor assignment
- Agent assignment
- Viewer scope assignment
- Customer management
- Ticket creation
- Ticket assignment
- Ticket status workflow
- Ticket priority
- Basic SLA tracking
- Internal comments
- Escalation
- Resolution
- Closure
- Audit history
- Basic operational dashboards
- Filtered operational views
- Structured operational data for external reporting tools
- Optional CSV exports or reporting-ready views

---

## MVP Non-Goals

The MVP does not include:

- Full business intelligence platform
- Advanced Power BI dashboard creation
- Payroll
- Workforce management scheduling
- Call recording
- Telephony integration
- Omnichannel messaging integrations
- Customer billing
- HR management
- AI-based ticket classification
- Predictive SLA modeling
- Advanced automation workflows
- Mobile application
- Real-time chat system
- External customer portal
- Production-grade enterprise SSO
- Multi-tenant billing model

---

## Core Business Capabilities

OpsSphere must support:

```text
User authenticates
  → User operates within role, permission, and scope
  → Ticket is created in operational context
  → Ticket is assigned to eligible agent
  → Ticket moves through valid workflow states
  → SLA state is visible
  → Internal comments support collaboration
  → Escalation makes risk visible to supervisors
  → Resolution preserves outcome
  → Closure finalizes the ticket
  → Audit history preserves traceability
  → Dashboards and reports provide operational visibility
```

---

## Core Success Criteria

OpsSphere is successful when:

- Users can authenticate into the system.
- Users access only the features allowed by their role.
- Users access only data within their operational scope.
- Admins can configure the operational structure.
- Tickets can be created, assigned, updated, escalated, resolved, and closed.
- Tickets can be filtered by region, country, account, campaign, supervisor, agent, status, priority, and SLA state.
- SLA state can be tracked for tickets.
- Internal comments can be added to tickets.
- Important ticket changes are recorded in audit history.
- Basic dashboards provide operational visibility.
- Backend follows Clean Architecture principles.
- Business logic is not implemented directly inside controllers.
- System persists data in SQL Server.
- Automated tests cover core workflows.
- Local development can be containerized with Docker.
- CI/CD-ready structure exists through GitHub Actions.
- The solution is Azure-ready.

---

## Scope Control Principle

Agents must protect the MVP from scope creep.

Use this rule:

```text
If a feature is not required for authentication, authorization, operational structure, ticket workflow, SLA visibility, auditability, dashboards, reporting-ready data, testing, DevOps, or supportability, it probably does not belong in the MVP.
```

---

## Product Positioning

OpsSphere should be presented as:

```text
An enterprise support operations platform for multinational BPO/contact center environments.
```

Not as:

```text
A generic CRUD app.
A generic ticketing clone.
A Power BI replacement.
A CRM replacement.
A telephony platform.
An AI automation suite.
```