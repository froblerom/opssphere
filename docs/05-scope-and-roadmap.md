# Scope and Roadmap

## Purpose

This document defines the planned scope and roadmap for OpsSphere.

The goal is to prevent the project from becoming too broad too early. OpsSphere should be developed incrementally, starting with the core operational workflows that prove the business value of the platform.

The roadmap is divided into:

- MVP Scope
- Phase 2
- Phase 3
- Future Considerations
- Out of Scope
- Assumptions
- Release Milestones

## Product Scope Summary

OpsSphere is an enterprise-grade operations platform for multinational BPO/contact center environments.

The platform focuses on managing support execution across:

- Regions
- Countries
- Accounts
- Campaigns
- Managers
- Supervisors
- Agents
- Viewers
- Tickets
- SLAs
- Audit history
- Role-based access
- Operational dashboards
- Structured data for external reporting tools

OpsSphere is not intended to become a full business intelligence platform, workforce management system, telephony platform, CRM replacement, or AI automation suite in its initial version.

## Scope Principles

The project will follow these scope principles:

- Build the operational foundation first.
- Prioritize business workflows over visual complexity.
- Keep the MVP focused on ticket execution, ownership, SLA visibility, and auditability.
- Avoid adding advanced reporting, AI, telephony, or automation too early.
- Use fictional sample data only.
- Design the system as enterprise-ready, but deliver it incrementally.
- Keep external reporting tools such as Power BI outside the core application scope.
- Make each phase valuable on its own.

## MVP Scope

The MVP focuses on the minimum set of capabilities required to demonstrate a realistic enterprise support operations platform.

The MVP should prove that OpsSphere can represent the operational structure of a multinational BPO/contact center and support the core ticket lifecycle with role-based access, SLA tracking, and audit history.

## MVP Goals

The MVP must demonstrate:

- A realistic operational hierarchy.
- Secure user authentication.
- Role-based access control.
- Basic administration of operational structure.
- Ticket lifecycle management.
- Ticket ownership visibility.
- Basic SLA tracking.
- Internal collaboration through comments.
- Audit history for important changes.
- Basic operational dashboards.
- Clean backend architecture.
- Testable workflows.

## MVP Functional Scope

### Authentication and Access

The MVP includes:

- User login.
- JWT authentication.
- Role-based authorization.
- Access control for Admin, Manager, Supervisor, Agent, and Viewer.
- Basic user session handling.

### User and Role Management

The MVP includes basic management of:

- Admin users.
- Managers.
- Supervisors.
- Agents.
- Viewers.
- Roles.
- Permissions.

The goal is not to build a complex identity management platform, but to support the operational roles required by OpsSphere.

### Operational Structure Management

The MVP includes administration of the core organizational structure:

- Regions.
- Countries.
- Accounts.
- Campaigns.
- Manager assignments.
- Supervisor assignments.
- Agent assignments.
- Viewer access scope.

Administrators should be able to configure enough structure to represent a realistic operation.

### Customer Management

The MVP includes basic customer management:

- Create customer.
- View customer.
- Update customer.
- Link customer to tickets.
- View basic customer ticket history.

Customer management is included only to support ticket context. It is not intended to become a full CRM in the MVP.

### Ticket Management

The MVP includes the core ticket lifecycle:

- Create ticket.
- View ticket.
- Update ticket.
- Assign ticket.
- Change ticket status.
- Change ticket priority.
- Add internal comments.
- Escalate ticket.
- Resolve ticket.
- Close ticket.
- View ticket history.

### Ticket Operational Context

Each ticket should be associated with operational context such as:

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

This allows tickets to be filtered, tracked, and audited based on the real business structure.

### Ticket Status Workflow

The MVP includes a basic ticket status workflow.

Initial statuses:

- Open
- Assigned
- In Progress
- Waiting for Customer
- Escalated
- Resolved
- Closed

The workflow should prevent invalid transitions where appropriate.

### Internal Comments

The MVP includes internal ticket comments.

Internal comments allow agents and supervisors to collaborate without relying on external tools such as chat or email.

Internal comments should be linked to:

- Ticket.
- Author.
- Timestamp.
- Comment content.

### Basic SLA Tracking

The MVP includes basic SLA tracking.

The system should support:

- SLA target per ticket.
- SLA status.
- Tickets within SLA.
- Tickets close to SLA breach.
- Overdue tickets.
- SLA visibility by account, campaign, agent, supervisor, status, and priority.

The MVP does not require advanced SLA calendars, pause rules, business-hour calculations, or predictive SLA modeling.

### Audit History

The MVP includes audit history for important operational changes.

Audit events should include:

- Ticket created.
- Ticket assigned.
- Status changed.
- Priority changed.
- Agent changed.
- Ticket escalated.
- Ticket resolved.
- Ticket closed.
- Internal comment added.
- User created.
- Role changed.
- Account created.
- Campaign created.
- Permission changed.

Each audit event should capture:

- Who performed the action.
- When it happened.
- What changed.
- Previous value when applicable.
- New value when applicable.

### Basic Operational Dashboards

The MVP includes basic operational dashboards for day-to-day visibility.

Initial dashboard metrics:

- Open tickets.
- Overdue tickets.
- Tickets by status.
- Tickets by priority.
- Tickets by account.
- Tickets by campaign.
- Tickets by agent.
- Tickets by supervisor.
- SLA status overview.

The MVP dashboard should focus on operational visibility, not advanced analytics.

### Filtering and Search

The MVP includes filtered views for tickets and operational data.

Initial filters:

- Region.
- Country.
- Account.
- Campaign.
- Supervisor.
- Agent.
- Status.
- Priority.
- SLA state.
- Date created.

### Structured Data for Reporting

The MVP should produce clean, structured operational data that can support external reporting tools.

The system may include basic CSV export or reporting-ready views, but it should not attempt to replace Power BI.

## MVP Technical Scope

The MVP includes the following technical foundation:

- .NET 10 backend.
- ASP.NET Core Web API.
- Entity Framework Core.
- SQL Server.
- Clean Architecture.
- CQRS with MediatR.
- JWT authentication.
- Role-based authorization.
- Angular frontend.
- TypeScript.
- Reactive Forms.
- Angular Material or Tailwind.
- xUnit tests.
- Integration tests for core workflows.
- Docker-based local development.
- GitHub Actions-ready structure.
- Azure-ready deployment design.

## MVP Non-Goals

The MVP will not include:

- Advanced Power BI dashboards.
- AI-assisted ticket classification.
- Predictive SLA risk.
- Telephony integration.
- Call recording.
- Omnichannel integrations.
- Workforce scheduling.
- Payroll.
- HR management.
- External customer portal.
- Real-time chat.
- Mobile app.
- Complex notification engine.
- Enterprise SSO.
- Multi-tenant billing.

## Phase 2 Scope

Phase 2 expands the operational capabilities after the MVP is stable.

The focus of Phase 2 is to improve supervisor workflows, operational monitoring, notifications, attachments, and escalation visibility.

## Phase 2 Goals

Phase 2 should improve:

- Supervisor oversight.
- Escalation management.
- Operational dashboards.
- Notification visibility.
- Ticket context.
- Attachment handling.
- Data export quality.
- Audit and governance coverage.

## Phase 2 Functional Scope

### Enhanced Dashboards

Phase 2 includes improved dashboards for:

- Managers.
- Supervisors.
- Agents.
- Viewers.

Possible dashboard improvements:

- Workload by region.
- Workload by country.
- Workload by account.
- Workload by campaign.
- SLA compliance by account.
- SLA compliance by campaign.
- Ticket aging by supervisor.
- Agent workload distribution.
- Escalation trends.
- Overdue ticket trends.

### Escalation Queues

Phase 2 includes dedicated escalation queues.

Escalation queues should allow supervisors and managers to identify:

- Escalated tickets.
- Escalation reasons.
- Assigned supervisor.
- Escalation age.
- SLA state.
- Resolution status.

### Notification Center

Phase 2 may include an in-app notification center.

Possible notifications:

- Ticket assigned.
- Ticket escalated.
- Ticket close to SLA breach.
- Ticket overdue.
- Ticket reassigned.
- Comment added.
- Priority changed.
- Ticket resolved.

The first version of notifications should be simple and internal to the app.

### Attachments

Phase 2 may include ticket attachments.

Attachment examples:

- Screenshots.
- Supporting documents.
- Customer-provided files.
- Internal evidence files.

Attachments should be linked to tickets and follow access control rules.

### Advanced Audit Views

Phase 2 may include improved audit history views.

Possible improvements:

- Filter audit events by user.
- Filter audit events by ticket.
- Filter audit events by entity type.
- Filter audit events by date.
- Export audit history.
- View administrative changes separately from ticket changes.

### Improved User and Permission Management

Phase 2 may include a more complete permissions matrix.

Possible improvements:

- Scope-based permissions.
- Region-level manager access.
- Account-level supervisor access.
- Campaign-level agent assignment.
- Read-only scoped viewer access.
- Permission audit history.

### Export and Reporting-Ready Views

Phase 2 may include stronger export features.

Possible exports:

- Ticket list export.
- SLA status export.
- Audit history export.
- Agent workload export.
- Campaign workload export.

These exports should support external analysis in tools such as Power BI.

## Phase 2 Technical Scope

Phase 2 may include:

- Improved frontend state management.
- More complete integration test coverage.
- Background jobs for SLA checks.
- Better logging with Serilog.
- Health checks.
- Basic Application Insights readiness.
- Improved Docker Compose setup.
- More complete CI pipeline.

## Phase 3 Scope

Phase 3 focuses on cloud readiness, production-oriented architecture, advanced operational intelligence, and future enterprise capabilities.

Phase 3 should only begin after the MVP and Phase 2 workflows are stable.

## Phase 3 Goals

Phase 3 should improve:

- Cloud deployment readiness.
- Production observability.
- Advanced operational insights.
- Integration readiness.
- Future AI capabilities.
- Scalability and maintainability.

## Phase 3 Functional Scope

### Azure Deployment

Phase 3 includes deployment to Azure or Azure-ready infrastructure.

Target Azure services may include:

- Azure App Service.
- Azure SQL.
- Azure Key Vault.
- Application Insights.
- Azure Storage for attachments if attachments are implemented.
- GitHub Actions deployment pipeline.

### Production Observability

Phase 3 includes stronger observability.

Possible features:

- Structured logs.
- Request tracing.
- Error tracking.
- Health checks.
- Application metrics.
- Audit metrics.
- SLA monitoring jobs.
- Operational telemetry.

### Advanced SLA Capabilities

Phase 3 may include more advanced SLA rules.

Possible improvements:

- SLA rules by account.
- SLA rules by campaign.
- SLA rules by priority.
- SLA rules by category.
- Business hours support.
- SLA pause and resume rules.
- Escalation based on SLA risk.

### AI-Assisted Ticket Classification

Phase 3 may include AI-assisted classification as an optional enhancement.

Possible AI features:

- Suggested ticket category.
- Suggested priority.
- Suggested campaign.
- Similar ticket suggestions.
- Suggested escalation risk.

AI features should not be part of the MVP.

### Predictive SLA Risk

Phase 3 may include predictive SLA risk analysis.

Possible capabilities:

- Identify tickets likely to breach SLA.
- Identify campaigns with recurring SLA risk.
- Identify overloaded supervisors or agents.
- Detect abnormal workload patterns.

### External Integrations

Phase 3 may include selected integrations.

Possible integrations:

- Power BI dataset or export integration.
- Email notification service.
- Identity provider integration.
- External storage for attachments.
- Webhooks for operational events.

## Phase 3 Technical Scope

Phase 3 may include:

- Azure deployment pipeline.
- Secrets management with Azure Key Vault.
- Application Insights integration.
- More complete CI/CD.
- Performance testing.
- Security hardening.
- Production-ready configuration management.
- Integration architecture.

## Future Considerations

The following capabilities may be considered in future versions but are not required for the initial roadmap:

- External customer portal.
- Real-time chat.
- Mobile application.
- Omnichannel integration.
- Telephony integration.
- Workforce management integration.
- Advanced approval workflows.
- Multi-tenant commercial SaaS model.
- Advanced role customization.
- Custom report builder.
- AI agent assistant.
- Knowledge base integration.
- RAG assistant integration.
- Customer satisfaction tracking.
- Quality assurance review workflows.

## Out of Scope

The following items are outside the current planned scope unless explicitly introduced in a future roadmap revision:

- Full business intelligence platform.
- Advanced Power BI dashboard design.
- Payroll management.
- Workforce management scheduling.
- HR management.
- Call recording.
- Telephony infrastructure.
- Omnichannel messaging platform.
- Customer billing.
- Real-time chat system.
- External customer self-service portal.
- Production-grade enterprise SSO.
- Multi-tenant billing model.
- Complex workflow automation engine.
- Native mobile application.
- AI-based decision automation.
- Replacement for CRM systems.
- Replacement for Power BI.
- Replacement for workforce management tools.

## Assumptions

The roadmap is based on the following assumptions:

- OpsSphere represents a multinational BPO/contact center environment.
- The business structure includes regions, countries, accounts, campaigns, managers, supervisors, agents, viewers, roles, and permissions.
- Admins are responsible for configuring and maintaining the operational structure.
- Managers oversee one or more regions.
- Supervisors oversee accounts and campaigns within their assigned scope.
- Agents work within assigned accounts or campaigns.
- Viewers require read-only access to operational information.
- Customers do not directly access the initial system.
- Advanced reporting is handled outside OpsSphere by tools such as Power BI.
- The MVP should prioritize operational execution and auditability over advanced analytics.
- The system should be designed with maintainability and testability from the beginning.
- All sample operational data will be fictional.
- The project will be developed incrementally as a portfolio project using realistic enterprise software practices.

## Release Milestones

## Milestone 0: Documentation Foundation

### Goal

Establish the business and planning foundation before implementation.

### Deliverables

- Executive Summary.
- Business Context.
- Business Case.
- Project Charter.
- Stakeholder Map.
- Scope and Roadmap.
- Software Requirements Specification.
- Use Cases.
- Business Process Flows.
- Business Rules.
- Domain Model.
- Architecture Overview.

### Exit Criteria

- Business context is clear.
- MVP scope is defined.
- Stakeholders are identified.
- Out-of-scope items are documented.
- Initial requirements can be written.
- The project is ready for domain and architecture design.

## Milestone 1: Technical Foundation

### Goal

Create the base technical structure of the application.

### Deliverables

- GitHub repository structure.
- Backend solution structure.
- Clean Architecture layers.
- Angular application structure.
- SQL Server configuration.
- Docker Compose setup.
- Initial CI pipeline.
- Basic health check endpoint.
- Basic logging setup.

### Exit Criteria

- Backend project builds successfully.
- Frontend project builds successfully.
- Local environment can run with documented setup steps.
- Database connection is configured.
- Basic automated pipeline is available.

## Milestone 2: Identity and Access Foundation

### Goal

Implement authentication and role-based access control.

### Deliverables

- User login.
- JWT authentication.
- Role model.
- Basic permissions.
- Admin role.
- Manager role.
- Supervisor role.
- Agent role.
- Viewer role.
- Authorization policies.

### Exit Criteria

- Users can authenticate.
- Roles can be assigned.
- Protected endpoints require authentication.
- Users can access only authorized features.
- Basic access control tests exist.

## Milestone 3: Operational Structure Management

### Goal

Implement administration of the core business structure.

### Deliverables

- Region management.
- Country management.
- Account management.
- Campaign management.
- Manager management.
- Supervisor management.
- Agent management.
- Viewer management.
- Assignment rules.
- Basic administrative audit history.

### Exit Criteria

- Admins can create and manage the core operational structure.
- Managers can be assigned to regions.
- Supervisors can be assigned to accounts or campaigns.
- Agents can be assigned to accounts or campaigns.
- Viewers can be given read-only scope.
- Administrative changes are validated and auditable.

## Milestone 4: Ticket Lifecycle

### Goal

Implement the core ticket management workflow.

### Deliverables

- Customer management.
- Ticket creation.
- Ticket detail view.
- Ticket assignment.
- Ticket status workflow.
- Ticket priority.
- Internal comments.
- Ticket escalation.
- Ticket resolution.
- Ticket closure.
- Ticket history.

### Exit Criteria

- Agents can create and manage tickets within scope.
- Supervisors can monitor and manage tickets within scope.
- Ticket lifecycle changes are recorded.
- Ticket ownership is visible.
- Internal comments are supported.
- Escalation workflow is available.

## Milestone 5: SLA and Audit

### Goal

Implement basic SLA tracking and audit history.

### Deliverables

- SLA target tracking.
- SLA state calculation.
- Overdue ticket detection.
- Tickets close to SLA breach.
- Audit history for ticket changes.
- Audit history for important administrative changes.
- SLA filters.

### Exit Criteria

- Tickets display SLA state.
- Users can filter tickets by SLA state.
- Important changes are recorded in audit history.
- Supervisors can identify overdue tickets.
- Managers can review SLA visibility within their scope.

## Milestone 6: Operational Dashboards and Filters

### Goal

Provide basic day-to-day operational visibility.

### Deliverables

- Dashboard summary cards.
- Ticket filters.
- Workload views.
- SLA overview.
- Tickets by status.
- Tickets by priority.
- Tickets by account.
- Tickets by campaign.
- Tickets by agent.
- Tickets by supervisor.
- Basic CSV export or reporting-ready data view.

### Exit Criteria

- Managers can review assigned regional performance.
- Supervisors can review workload and SLA state.
- Agents can view assigned work.
- Viewers can access read-only dashboards.
- Data can support external reporting tools.

## Milestone 7: MVP Stabilization

### Goal

Stabilize the MVP for portfolio presentation.

### Deliverables

- End-to-end workflow validation.
- Integration tests for core workflows.
- Frontend polish.
- Seed data.
- README documentation.
- Screenshots.
- Architecture documentation updates.
- Demo scenario.
- Known limitations section.

### Exit Criteria

- MVP demo can be executed from start to finish.
- Sample data demonstrates a realistic BPO/contact center operation.
- Core workflows are stable.
- Documentation explains business value and technical architecture.
- The project can be presented as an enterprise operations platform.

## Roadmap Summary

| Phase | Focus | Main Outcome |
|---|---|---|
| MVP | Core operational platform | Prove ticket lifecycle, structure, SLA, audit, and role-based access |
| Phase 2 | Operational maturity | Improve dashboards, escalations, notifications, attachments, and exports |
| Phase 3 | Enterprise readiness | Add Azure deployment, observability, advanced SLA, and selected integrations |
| Future | Advanced capabilities | Consider AI, customer portal, telephony, workforce integration, and RAG support |

## Conclusion

The Scope and Roadmap document defines how OpsSphere will be delivered incrementally.

The MVP focuses on the operational foundation: authentication, role-based access, organizational structure, tickets, SLA tracking, internal comments, audit history, dashboards, and structured data.

Phase 2 improves operational maturity with better dashboards, escalation queues, notifications, attachments, and export capabilities.

Phase 3 focuses on Azure readiness, observability, advanced SLA capabilities, selected integrations, and optional AI-assisted features.

This roadmap keeps OpsSphere focused, realistic, and aligned with enterprise software practices while preventing the project from becoming too large too early.
