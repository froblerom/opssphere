# Stakeholder Map

## Purpose

This document identifies the main stakeholders involved in OpsSphere, their operational needs, their pain points, and how they interact with the system.

Stakeholders are important because they help define the system requirements, user roles, permissions, workflows, dashboards, audit needs, and operational boundaries of the platform.

OpsSphere is designed for a multinational BPO/contact center environment where operational work is organized by regions, countries, accounts, campaigns, supervisors, and agents.

## Stakeholder Overview

OpsSphere includes the following main stakeholder groups:

- System Administrator / Admin
- Operations Manager
- Supervisor
- Support Agent
- Viewer / Read-only Reviewer
- Customer
- Technical Owner

## Stakeholder Matrix

| Stakeholder | Primary Role | Scope | Access Type |
|---|---|---|---|
| 🛠️ Admin | Configure platform structure | Global | Full read/write |
| 📊 Operations Manager | Oversee performance | Regional | Read + oversight |
| 👥 Supervisor | Manage agents and tickets | Account / Campaign | Read/write within scope |
| 🎧 Support Agent | Resolve tickets | Assigned tickets | Limited write |
| 👁️ Viewer | Audit and review | Configurable | Read-only |
| 🙍 Customer | Receive support | N/A (linked to tickets) | No direct access |
| 💻 Technical Owner | Build and maintain | Entire platform | Implementation |

## Stakeholder Details

### System Administrator / Admin

**Description**

The System Administrator is responsible for configuring and maintaining the operational structure of OpsSphere.

Admins are not primarily responsible for resolving tickets. Their main responsibility is to ensure that the platform accurately reflects the real organizational structure of the BPO/contact center operation.

**Main Responsibilities**

- Create and manage regions.
- Create and manage countries.
- Create and manage accounts.
- Create and manage campaigns.
- Create and manage managers.
- Create and manage supervisors.
- Create and manage agents.
- Create and manage viewer users.
- Manage roles and permissions.
- Maintain operational catalogs.
- Deactivate users who no longer belong to the operation.
- Keep the platform structure aligned with the business operation.

**Needs**

- A reliable way to maintain organizational structure.
- Controlled access to administrative features.
- Validation rules to avoid inconsistent setup.
- Audit history for important administrative changes.
- Clear visibility of which users belong to which region, country, account, campaign, or role.

**Pain Points**

- Operational structures can change frequently.
- Users may be assigned to the wrong account, campaign, or role.
- Regions, countries, accounts, and campaigns can become inconsistent if maintained manually.
- Incorrect permissions can create security or visibility problems.
- Lack of audit history makes administrative changes difficult to trace.

**System Interaction**

Admins interact with OpsSphere through administrative modules such as:

- Region management.
- Country management.
- Account management.
- Campaign management.
- User management.
- Role management.
- Permission management.
- Operational catalog management.
- Audit history review.

**Example Scenario**

An Admin creates a new account called NovaBank, adds the Credit Card Support and Fraud Review Support campaigns, assigns a supervisor to the account, creates agent users, and assigns them to the appropriate campaign.

### Operations Manager

**Description**

The Operations Manager oversees performance across one or more regions.

Managers are responsible for reviewing operational performance, identifying workload issues, monitoring SLA health, supervising supervisors, and ensuring that accounts and campaigns are operating correctly within their assigned scope.

**Main Responsibilities**

- Monitor performance across assigned regions.
- Review ticket volume by country, account, and campaign.
- Monitor SLA compliance.
- Identify overloaded supervisors, teams, or campaigns.
- Review escalation trends.
- Track unresolved and overdue ticket aging.
- Support operational decision-making.
- Ensure supervisors are managing their assigned teams effectively.

**Needs**

- Visibility across assigned regions.
- Ability to filter data by country, account, campaign, supervisor, agent, priority, status, and SLA state.
- Operational dashboards for daily monitoring.
- Reliable data for decision-making.
- Read access to audit history and ticket lifecycle information.
- Structured data that can support external reporting tools such as Power BI.

**Pain Points**

- Limited visibility across multiple countries or accounts.
- Difficulty identifying where workload is accumulating.
- SLA issues may be detected too late.
- Manual consolidation of operational information can be slow or inaccurate.
- Lack of traceability makes it harder to understand why performance issues occurred.

**System Interaction**

Operations Managers interact with OpsSphere through:

- Regional dashboards.
- Country-level filters.
- Account and campaign views.
- Supervisor performance views.
- SLA monitoring views.
- Escalation views.
- Ticket aging views.
- Audit history.
- Read-only operational data exports or reporting-ready views.

**Example Scenario**

A manager responsible for Latin America reviews the dashboard and identifies that the NovaBank Credit Card Support campaign in Mexico has a high number of overdue tickets and repeated SLA risk. The manager reviews the supervisor workload and escalation history to understand the issue.

### Supervisor

**Description**

The Supervisor manages the operational execution of one account or one or more campaigns within an account.

Supervisors are responsible for monitoring agent workload, ticket progress, SLA status, escalations, internal comments, and agent performance.

**Main Responsibilities**

- Monitor tickets assigned to agents.
- Review workload by agent.
- Track SLA risk and overdue tickets.
- Review escalated tickets.
- Reassign tickets when needed.
- Monitor ticket priority and status.
- Add or review internal comments.
- Support agents in resolving operational issues.
- Ensure tickets progress through the expected lifecycle.

**Needs**

- Visibility into all tickets under assigned account or campaign.
- Ability to see agent workload.
- Ability to identify overdue tickets.
- Ability to identify tickets close to SLA breach.
- Ability to review escalation reasons.
- Ability to reassign or escalate tickets depending on permissions.
- Audit history to understand ticket changes.

**Pain Points**

- Difficult to know which agents are overloaded.
- Tickets can become stuck without visibility.
- SLA breaches may occur without early warning.
- Escalations may lack context.
- Ticket ownership can become unclear after reassignment.
- Internal comments may be scattered across tools.

**System Interaction**

Supervisors interact with OpsSphere through:

- Ticket queue views.
- Agent workload views.
- SLA status views.
- Escalation queues.
- Ticket detail pages.
- Internal comments.
- Assignment actions.
- Status update review.
- Audit history.

**Example Scenario**

A supervisor assigned to NovaBank sees that an agent has multiple high-priority tickets close to SLA breach. The supervisor reviews the ticket details, reassigns one ticket to another agent, adds an internal comment, and monitors the SLA state.

### Support Agent

**Description**

The Support Agent handles tickets within an assigned account or campaign.

Agents are responsible for creating, updating, commenting on, escalating, resolving, and closing tickets according to their permissions and operational scope.

**Main Responsibilities**

- Create tickets.
- Update ticket details.
- Change ticket status.
- Add internal comments.
- Escalate tickets when needed.
- Resolve tickets.
- Close tickets when allowed.
- Maintain accurate ticket information.
- Work within assigned account or campaign scope.

**Needs**

- Clear visibility of assigned tickets.
- Clear ticket priority and SLA target.
- Easy access to ticket history.
- Ability to add internal comments.
- Ability to escalate tickets when needed.
- Ability to update ticket status.
- Simple workflows for ticket handling.
- Clear assignment ownership.

**Pain Points**

- Unclear ticket ownership.
- Too many tickets without prioritization.
- Difficulty knowing which tickets are close to SLA breach.
- Missing context from previous updates.
- Repetitive manual updates.
- Lack of clarity on escalation requirements.

**System Interaction**

Agents interact with OpsSphere through:

- Assigned ticket queue.
- Ticket creation form.
- Ticket detail page.
- Status update actions.
- Internal comments.
- Escalation action.
- Resolution and closure actions.
- SLA indicators.

**Example Scenario**

An agent assigned to Streamly Creator Support receives a ticket related to account verification. The agent reviews the SLA target, adds an internal comment, updates the status to In Progress, and later resolves the ticket.

### Viewer / Read-only Reviewer

**Description**

The Viewer is a read-only stakeholder who needs visibility into operational data without modifying records.

Viewers may support auditing, reporting, analysis, management review, quality assurance, or operational oversight.

Advanced reporting is outside the core scope of OpsSphere and is expected to be handled by tools such as Power BI. However, viewers may still need access to filtered operational data inside the platform.

**Main Responsibilities**

- Review operational information.
- View dashboards.
- Inspect ticket status and history.
- Review audit trails.
- Validate operational data.
- Support reporting or analysis activities.
- Access information without modifying the system.

**Needs**

- Read-only access.
- Filtered views by region, country, account, campaign, supervisor, agent, status, priority, and SLA state.
- Access to ticket history.
- Access to audit history.
- Reliable structured data.
- Controlled visibility based on assigned scope.

**Pain Points**

- Reporting users may need operational visibility but should not modify data.
- Data may be difficult to trust if ticket history is incomplete.
- Manual reporting depends on consistent operational data.
- Lack of read-only access can create unnecessary dependency on supervisors or admins.

**System Interaction**

Viewers interact with OpsSphere through:

- Read-only dashboards.
- Read-only ticket views.
- Filters.
- Audit history views.
- Exportable or reporting-ready data views.

**Example Scenario**

A viewer assigned to Latin America reviews ticket volume and SLA status for NovaBank and Streamly but cannot create, edit, assign, escalate, or close tickets.

### Customer

**Description**

The Customer is the person or entity associated with a support request.

In the initial scope of OpsSphere, customers do not directly access the system. Instead, customer information is linked to tickets so agents and supervisors can understand the context of each support case.

**Main Responsibilities**

Customers are not active system users in the initial scope.

From the system perspective, customer-related information is used to:

- Link tickets to a customer.
- Track customer issue history.
- Provide context to agents and supervisors.
- Support ticket categorization and operational tracking.

**Needs**

- Timely resolution of support issues.
- Clear ownership of support cases.
- Consistent handling of requests.
- Proper escalation when required.
- Accurate record of support interactions.

**Pain Points**

- Delayed resolution.
- Repeated explanations of the same issue.
- Inconsistent support handling.
- Lack of follow-up.
- Poor visibility into issue progress.

**System Interaction**

Customers do not directly interact with OpsSphere in the initial version.

Customer data is managed internally by agents, supervisors, and authorized users.

**Example Scenario**

A customer reports an incorrect credit card charge. An agent creates a ticket linked to that customer under the NovaBank Credit Card Support campaign.

### Technical Owner

**Description**

The Technical Owner is responsible for designing, implementing, testing, and maintaining OpsSphere.

This stakeholder ensures that the system is technically aligned with the business needs, architecture goals, security requirements, maintainability expectations, and portfolio objectives.

**Main Responsibilities**

- Design the system architecture.
- Implement backend APIs.
- Implement frontend workflows.
- Design the database model.
- Implement authentication and authorization.
- Implement role-based access control.
- Create automated tests.
- Configure Docker-based local development.
- Configure CI/CD workflows.
- Maintain technical documentation.
- Ensure the system is Azure-ready.

**Needs**

- Clear business context.
- Clear stakeholder responsibilities.
- Clear requirements.
- Clear use cases.
- Defined business rules.
- Architecture boundaries.
- Testable workflows.
- Stable domain model.
- Defined success criteria.

**Pain Points**

- Ambiguous business rules.
- Undefined role permissions.
- Unclear operational hierarchy.
- Requirements changing without documentation.
- Overly broad scope.
- Business logic leaking into UI or controllers.
- Lack of testable acceptance criteria.

**System Interaction**

The Technical Owner interacts with the project through:

- Source code.
- Architecture documentation.
- Requirements.
- Use cases.
- Business rules.
- Database design.
- API design.
- Testing strategy.
- DevOps pipeline.
- Deployment configuration.

**Example Scenario**

The Technical Owner implements the ticket assignment workflow using Clean Architecture, ensuring that agents can only receive tickets within their assigned account or campaign and that the assignment change is recorded in audit history.

## Stakeholder Relationships

OpsSphere reflects the following stakeholder relationships:

```text
Administrator
  → Configures regions, countries, accounts, campaigns, users, roles, and permissions

Operations Manager
  → Oversees one or more regions
  → Supervises supervisors
  → Reviews operational performance

Supervisor
  → Assigned to one account or one or more campaigns
  → Oversees agents
  → Monitors workload, SLA, escalations, and ticket progress

Support Agent
  → Assigned to an account or campaign
  → Handles tickets within operational scope

Viewer
  → Has read-only access to operational data

Customer
  → Associated with tickets
  → Does not directly access the initial system

Technical Owner
  → Builds and maintains the platform
```

## Access and Visibility Summary

| Role | Manage Structure | Manage Users | Manage Tickets | View Dashboards | View Audit History | Modify Data |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| Admin | ✅ | ✅ | Limited / Optional | ✅ | ✅ | ✅ |
| Operations Manager | ❌ | ❌ | Limited / Oversight | ✅ | ✅ | Limited |
| Supervisor | ❌ | ❌ | Yes, within scope | ✅ | Yes, within scope | Yes, within scope |
| Agent | ❌ | ❌ | Yes, assigned scope | Limited | Limited | Yes, assigned tickets |
| Viewer | ❌ | ❌ | ❌ | Read-only | Read-only | ❌ |
| Customer | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Technical Owner | No business ownership | No business ownership | No business ownership | No business ownership | No business ownership | Implementation only |

## Stakeholder-Driven Requirements

The needs of these stakeholders will influence the requirements of OpsSphere.

Examples:

- Admin needs define organizational structure management requirements.
- Manager needs define regional visibility and dashboard requirements.
- Supervisor needs define workload, escalation, and SLA monitoring requirements.
- Agent needs define ticket lifecycle and collaboration requirements.
- Viewer needs define read-only access and reporting-ready data requirements.
- Customer needs define customer information and ticket context requirements.
- Technical Owner needs define maintainability, architecture, testing, and deployment requirements.

## Conclusion

The stakeholder map confirms that OpsSphere is not only a ticket management application.

It is an enterprise operations platform where each stakeholder has a clear business purpose, operational responsibility, visibility scope, and interaction model.

This document will guide the definition of requirements, use cases, permissions, business rules, process flows, and architecture decisions.
