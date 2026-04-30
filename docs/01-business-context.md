# Business Context

## Business Domain

OpsSphere is modeled around a multinational BPO/contact center organization that provides operational support services for multiple client accounts across different regions, countries, campaigns, supervisors, and agents.The platform represents a real-world support operation where agents handle tickets within assigned accounts and campaigns, supervisors manage the operational execution of specific accounts, and regional managers oversee performance across one or more regions.OpsSphere is designed to centralize the operational workflow behind ticket management, SLA tracking, internal collaboration, audit history, accountability, and operational visibility.The system also includes an administrative layer responsible for maintaining the operational structure of the platform. Administrators manage the creation and configuration of regions, countries, accounts, campaigns, managers, supervisors, agents, roles, and permissions.## Business Context SummaryIn this business context, the organization operates as a multinational call center with several geographic regions. Each region contains one or more countries. Within those countries, the company serves different client accounts. Each account can have one or more campaigns, and each campaign groups a specific operational workload, process, or support function.Agents are assigned to accounts and campaigns. Supervisors are assigned to accounts and are responsible for overseeing the agents working under those accounts. Regional managers are responsible for monitoring the overall operation of one or more regions and supervising the supervisors within their scope.Administrators are responsible for configuring and maintaining the organizational structure inside OpsSphere. They create new regions, countries, accounts, campaigns, managers, supervisors, and agents so the system can accurately represent the company’s operational structure.The system also includes viewer users who require read-only access to operational information. These users are mainly related to visibility, auditing, analytics, or reporting contexts. Advanced business intelligence and executive reporting are considered outside the core scope of OpsSphere and would typically be handled by external tools such as Power BI.

## Operating Model

OpsSphere follows this operational structure:

> Multinational BPO / Contact Center → Region → Country → Account → Campaign → Supervisor → Agent

Administrators manage and maintain this structure inside the system.

> Administrator  → Manages Regions  → Manages Countries  → Manages Accounts  → Manages Campaigns  → Manages Managers  → Manages Supervisors  → Manages Agents  → Manages Roles and Permissions

In practice, some managerial roles may span more than one region, and some supervisors may oversee multiple campaigns within the same account depending on the operational structure.

Organizational Levels
---------------------

### Region

A region represents a large geographic operating area managed by one or more operations managers.

Example regions:

> Latin America / North America / EMEA / APAC

For the initial business context, the main focus is the Latin America region.

Example:

> Region: Latin America
> Countries:
> - Mexico
> - Colombia
> - Costa Rica
> - El Salvador

### Country

A country represents a national operation within a region.

Countries are useful for operational segmentation, access control, SLA analysis, workload visibility, and regional performance monitoring.

Example:

>Country: Mexico
>Region: Latin America

### Account

An account represents a client account served by the BPO/contact center.

An account is the business relationship or operational unit that groups support activity for a specific client. Agents and supervisors are commonly assigned around accounts, and operational performance is often reviewed at the account level.

Example accounts:

> - NovaBank
> - Streamly
> - Shopora
> - AeroLink

These names are fictional and are used only to simulate realistic enterprise support operations.

### Campaign

A campaign represents a specific operational service, support function, or workload inside an account.

An account can have multiple campaigns.

Example:

> Account: NovaBank
> Campaigns:
> - Credit Card Support
> - Fraud Review Support
> - Account Access Support

Another example:

> Account: Streamly
> Campaigns:
> - Creator Support
> - Content Moderation Appeals
> - Monetization Support

Campaigns may have their own ticket categories, priorities, SLA expectations, escalation paths, and operational metrics.

### Manager

A manager oversees the performance of one or more regions.

Some managers may be responsible for a single region, while others may manage multiple regions. They supervise supervisors, review operational metrics, identify workload issues, monitor SLA performance, and ensure that accounts and campaigns are operating correctly.

Example:

> Manager: Sofia Ramirez  
> Scope:
> - Latin America

Another example:

> Manager: Daniel Carter  
> Scope:
> - North America
> - Latin America

### Supervisor

A supervisor is responsible for managing the agents assigned to an account or campaign.

Supervisors monitor workload, ticket status, SLA compliance, escalations, internal comments, and agent performance within their assigned operational scope.

In this model, supervisors are usually assigned to an account and supervise the agents working under that account. Depending on the business structure, a supervisor may also oversee one or more campaigns within that account.

Example:

> Supervisor: Laura Torres  
> Account: NovaBank  
> Campaigns:
> - Credit Card Support
> - Fraud Review Support

### Agent

An agent is responsible for handling tickets within their assigned account and campaign.

Agents can create, update, comment on, escalate, and resolve tickets according to their permissions and operational scope.

In the initial model, agents are generally assigned to one main account or campaign.

Example:

> Agent: Ana López  
> Account: NovaBank  
> Campaign: Credit Card Support  
> Supervisor: Laura Torres

### Administrator

An administrator is responsible for configuring and maintaining the operational structure of OpsSphere.

Administrators are not primarily responsible for resolving tickets. Their main responsibility is to keep the platform correctly configured so managers, supervisors, agents, and viewers can operate within the right organizational structure.

Administrators can manage:

- Regions
- Countries
- Accounts
- Campaigns
- Managers
- Supervisors
- Agents
- Viewer users
- Roles
- Permissions
- Operational catalogs

Example administrative actions:

- Create a new region.
- Create a new country under a region.
- Create a new account.
- Create a new campaign under an account.
- Assign a manager to one or more regions.
- Assign a supervisor to an account.
- Assign agents to an account or campaign.
- Create viewer users for read-only visibility.
- Update roles and permissions.
- Deactivate users who no longer belong to the operation.

This role gives operational governance to the platform and ensures that OpsSphere reflects the real structure of the organization.

### Viewer

A viewer is a read-only user who can access operational information for visibility, auditing, analytics, or reporting purposes.

In a real enterprise environment, advanced reporting is often handled by tools such as Power BI. For that reason, OpsSphere focuses on capturing, structuring, and exposing reliable operational data rather than replacing a dedicated business intelligence platform.

The viewer role exists to support controlled read-only visibility into operational data.

Example:

> Viewer: Regional Reporting User  
> Access Level: Read-only  
> Scope:
> - Region: Latin America
> - Accounts: NovaBank, Streamly

### Example Operational Structure

> Region: Latin America  
> Country: Mexico  
> Account: NovaBank  
> Campaign: Credit Card Support  
> Supervisor: Laura Torres  
> Agents:
> - Ana López
> - Roberto Díaz
> - Miguel Hernández

> Region: Latin America  
> Country: Colombia  
> Account: Streamly  
> Campaign: Creator Support  
> Supervisor: Diego Herrera  
> Agents:
> - Camila Rojas
> - Andrés Pérez
> - Valentina Gómez

> Region: Latin America  
> Country: Costa Rica  
> Account: Streamly  
> Campaign: Content Moderation Appeals  
> Supervisor: Mariana Solís  
> Agents:
> - José Vargas
> - Daniela Castro

### Example Ticket

> Ticket: OPS-000145  
> Region: Latin America  
> Country: Mexico  
> Account: NovaBank  
> Campaign: Credit Card Support  
> Category: Billing Dispute  
> Priority: High  
> Status: In Progress  
> Assigned Agent: Ana López  
> Supervisor: Laura Torres  
> SLA Target: 8 business hours  
> Customer Impact: Customer reports an incorrect credit card charge

This ticket represents a realistic operational case where the system needs to track not only the issue itself, but also the account, campaign, assigned agent, supervisor, SLA target, priority, status, and operational ownership.

### Business Problems Addressed

OpsSphere addresses several operational problems common in multinational BPO/contact center environments:

- Difficulty tracking ticket ownership across accounts and campaigns.
- Limited visibility into agent workload and supervisor scope.
- SLA tracking complexity across different accounts and campaign types.
- Lack of centralized audit history for ticket changes and escalations.
- Operational fragmentation across regions and countries.
- Difficulty understanding which teams, accounts, or campaigns are accumulating unresolved or overdue work.
- Limited traceability when tickets change status, owner, priority, or escalation level.
- Difficulty maintaining the operational structure of regions, countries, accounts, campaigns, and users in a centralized system.

### Operational Scope

OpsSphere focuses on the operational management layer of the support process.

The platform supports:

- Region management.
- Country management.
- Account management.
- Campaign management.
- Manager management.
- Supervisor management.
- Agent management.
- Viewer user management.
- Role and permission management.
- Agent and supervisor assignments.
- Ticket lifecycle management.
- SLA tracking.
- Internal comments.
- Escalation visibility.
- Audit history.
- Operational dashboards.
- Read-only visibility for viewers.
- Structured operational data that can later support business intelligence tools.

### Reporting Context

OpsSphere includes basic operational dashboards and filtered data views for day-to-day visibility.

However, advanced executive reporting, historical analytics, and business intelligence dashboards are considered outside the main application scope. In a real enterprise environment, those capabilities would typically be handled by Power BI or a similar reporting platform.

OpsSphere should therefore focus on producing clean, structured, auditable operational data that can be exported or integrated into external reporting tools.

### Business Meaning

This business context allows OpsSphere to demonstrate realistic enterprise workflows such as:

- Region-based operational visibility.
- Country-level segmentation.
- Account-based ownership.
- Campaign-based ticket assignment.
- Supervisor oversight.
- Agent workload tracking.
- SLA monitoring.
- Escalation management.
- Auditability.
- Administrative control of operational structure.
- Role-based access to operational data.
- Operational data readiness for reporting and analytics.

OpsSphere is therefore positioned as an enterprise operations platform for managing support execution, not as a generic ticketing application or a full business intelligence solution.