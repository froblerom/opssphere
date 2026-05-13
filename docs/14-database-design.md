# Database Design

## Document Information

| Field | Value |
|---|---|
| Project | OpsSphere |
| Document | Database Design |
| File | `docs/14-database-design.md` |
| Version | 1.0 |
| Status | Draft |
| Project Type | Enterprise Support Operations Platform |
| Database Engine | SQL Server |
| ORM | Entity Framework Core |
| Related Issue | #5 |

---

## 1. Purpose

This document defines the initial database design for OpsSphere.

The database design is derived from the domain model, use cases, business rules, process flows, and architecture documentation.

The purpose of this document is to define:

- Initial ERD reference.
- Core tables.
- Main relationships.
- Primary keys.
- Foreign keys.
- Important fields.
- Initial indexes.
- Audit strategy.
- Soft delete strategy.
- Reporting-ready data considerations.

This document is not a final migration script. It is a technical design baseline that will guide Entity Framework Core entity modeling, SQL Server schema design, and future implementation work.

---

## 2. Design Goals

The database design must support the following goals:

1. Represent the BPO/contact center operational hierarchy.
2. Support users, roles, permissions, and operational scopes.
3. Support ticket lifecycle management.
4. Support customer-linked tickets.
5. Support assignment, comments, escalation, resolution, and closure.
6. Support SLA tracking.
7. Preserve audit history for critical business actions.
8. Support dashboard filtering and reporting-ready data.
9. Preserve historical traceability when users or organizational records are deactivated.
10. Use soft delete or deactivation where historical records depend on existing data.

---

## 3. Database Technology

OpsSphere will use:

```text
SQL Server
```

Entity Framework Core will be used for:

```text
Entity mapping
Migrations
Change tracking
Query execution
Relationship configuration
Seed data where appropriate
```

The database should be designed with relational integrity in mind.

---

# 4. ERD

## 4.1 Diagram Reference

![OpsSphere ERD](diagrams/database/opssphere-erd.png)

> Diagram placeholder: export the ERD as `opssphere-erd.png` and store it in `docs/diagrams/database/`.

---

## 4.2 Diagram Folder Structure

Database diagrams should be stored under:

```text
docs/
  diagrams/
    database/
      opssphere-erd.png
```

The Markdown image path should be:

```text
diagrams/database/opssphere-erd.png
```

---

# 5. Database Areas

The initial database is organized into the following areas:

| Area | Purpose |
|---|---|
| Identity and Access | Users, roles, permissions, and operational scopes. |
| Organization Structure | Regions, countries, accounts, campaigns, and assignments. |
| Customer Management | Customer records and customer-ticket linkage. |
| Ticket Management | Tickets, assignment history, comments, escalations, status history, and resolution. |
| SLA Management | SLA policies and ticket SLA state. |
| Audit | Immutable audit log for critical business actions. |
| Reporting Support | Indexed operational fields and reporting-ready views. |

---

# 6. Naming Conventions

## 6.1 Table Naming

Use plural table names:

```text
Users
Roles
Permissions
Tickets
Customers
AuditLogs
```

## 6.2 Primary Key Naming

Use:

```text
Id
```

as the primary key column for each table.

Recommended type:

```text
uniqueidentifier
```

or:

```text
bigint identity
```

For the initial design, use:

```text
uniqueidentifier
```

because it works well with distributed creation patterns and Entity Framework Core.

---

## 6.3 Foreign Key Naming

Use the format:

```text
<EntityName>Id
```

Examples:

```text
UserId
RoleId
TicketId
AccountId
CampaignId
CustomerId
AssignedAgentId
```

---

## 6.4 Timestamp Naming

Use:

```text
CreatedAt
UpdatedAt
DeletedAt
DeactivatedAt
ResolvedAt
ClosedAt
```

Timestamps should be stored in UTC.

---

# 7. Common Columns

Most core tables should include the following common columns:

| Column | Type | Description |
|---|---|---|
| Id | uniqueidentifier | Primary key. |
| CreatedAt | datetime2 | Record creation timestamp in UTC. |
| CreatedByUserId | uniqueidentifier null | User who created the record, when applicable. |
| UpdatedAt | datetime2 null | Last update timestamp in UTC. |
| UpdatedByUserId | uniqueidentifier null | User who last updated the record, when applicable. |
| IsActive | bit | Indicates whether the record is active. |
| IsDeleted | bit | Indicates soft deletion when applicable. |
| DeletedAt | datetime2 null | Soft deletion timestamp. |
| DeletedByUserId | uniqueidentifier null | User who soft-deleted the record. |

Not every table requires all common columns. Audit and history tables may use a more specialized structure.

---

# 8. Identity and Access Tables

## 8.1 Users

Stores internal users who can authenticate and access OpsSphere.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| Email | nvarchar(256) | Yes | User email used for login. |
| PasswordHash | nvarchar(max) | Yes | Secure password hash. |
| FirstName | nvarchar(100) | Yes | User first name. |
| LastName | nvarchar(100) | Yes | User last name. |
| DisplayName | nvarchar(200) | Yes | Display name in the UI. |
| IsActive | bit | Yes | Indicates whether the user can authenticate. |
| LastLoginAt | datetime2 null | No | Last successful login timestamp. |
| CreatedAt | datetime2 | Yes | Creation timestamp. |
| UpdatedAt | datetime2 null | No | Last update timestamp. |
| DeactivatedAt | datetime2 null | No | Deactivation timestamp. |

Recommended constraints:

```text
PK_Users(Id)
UQ_Users_Email
```

Recommended indexes:

```text
IX_Users_Email
IX_Users_IsActive
```

Soft delete strategy:

```text
Users should normally be deactivated, not physically deleted.
```

Design note:

```text
The domain model mentions RoleId as a conceptual attribute.
The database design uses UserRoles to support one or more roles per user.
For the MVP, the application may enforce one primary role per user even if the schema supports multiple roles.
```

---

## 8.2 Roles

Stores system roles.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| Name | nvarchar(100) | Yes | Role name. |
| Description | nvarchar(500) null | No | Role description. |
| IsSystemRole | bit | Yes | Indicates built-in system role. |
| IsActive | bit | Yes | Indicates whether role is active. |
| CreatedAt | datetime2 | Yes | Creation timestamp. |

Initial roles:

```text
Admin
OperationsManager
Supervisor
Agent
Viewer
```

Recommended constraints:

```text
PK_Roles(Id)
UQ_Roles_Name
```

---

## 8.3 Permissions

Stores granular permissions.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| Code | nvarchar(150) | Yes | Permission code. |
| Name | nvarchar(150) | Yes | Human-readable permission name. |
| Description | nvarchar(500) null | No | Permission description. |
| IsActive | bit | Yes | Indicates whether permission is active. |

Example permission codes:

```text
users.manage
roles.manage
organization.manage
tickets.create
tickets.assign
tickets.update_status
tickets.comment
tickets.escalate
tickets.resolve
tickets.close
sla.view
dashboard.view
reports.view
audit.view
```

Recommended constraints:

```text
PK_Permissions(Id)
UQ_Permissions_Code
```

---

## 8.4 UserRoles

Many-to-many relationship between users and roles.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| UserId | uniqueidentifier | Yes | FK to Users. |
| RoleId | uniqueidentifier | Yes | FK to Roles. |
| CreatedAt | datetime2 | Yes | Assignment timestamp. |
| CreatedByUserId | uniqueidentifier null | No | Admin who assigned the role. |

Recommended constraints:

```text
PK_UserRoles(UserId, RoleId)
FK_UserRoles_Users_UserId
FK_UserRoles_Roles_RoleId
```

Recommended indexes:

```text
IX_UserRoles_RoleId
```

---

## 8.5 RolePermissions

Many-to-many relationship between roles and permissions.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| RoleId | uniqueidentifier | Yes | FK to Roles. |
| PermissionId | uniqueidentifier | Yes | FK to Permissions. |
| CreatedAt | datetime2 | Yes | Assignment timestamp. |

Recommended constraints:

```text
PK_RolePermissions(RoleId, PermissionId)
FK_RolePermissions_Roles_RoleId
FK_RolePermissions_Permissions_PermissionId
```

Recommended indexes:

```text
IX_RolePermissions_PermissionId
```

---

# 9. Organization Structure Tables

## 9.1 Regions

Stores large geographic operating areas.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| Name | nvarchar(150) | Yes | Region name. |
| Code | nvarchar(50) | Yes | Region code. |
| IsActive | bit | Yes | Active status. |
| CreatedAt | datetime2 | Yes | Creation timestamp. |
| UpdatedAt | datetime2 null | No | Last update timestamp. |

Recommended constraints:

```text
PK_Regions(Id)
UQ_Regions_Code
```

---

## 9.2 Countries

Stores countries under regions.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| RegionId | uniqueidentifier | Yes | FK to Regions. |
| Name | nvarchar(150) | Yes | Country name. |
| Code | nvarchar(20) | Yes | Country code. |
| IsActive | bit | Yes | Active status. |
| CreatedAt | datetime2 | Yes | Creation timestamp. |
| UpdatedAt | datetime2 null | No | Last update timestamp. |

Recommended constraints:

```text
PK_Countries(Id)
FK_Countries_Regions_RegionId
UQ_Countries_Code
```

Recommended indexes:

```text
IX_Countries_RegionId
IX_Countries_IsActive
```

---

## 9.3 Accounts

Stores client accounts served by the operation.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| CountryId | uniqueidentifier | Yes | FK to Countries. |
| Name | nvarchar(200) | Yes | Account name. |
| Code | nvarchar(50) | Yes | Account code. |
| Description | nvarchar(500) null | No | Account description. |
| IsActive | bit | Yes | Active status. |
| CreatedAt | datetime2 | Yes | Creation timestamp. |
| UpdatedAt | datetime2 null | No | Last update timestamp. |

Recommended constraints:

```text
PK_Accounts(Id)
FK_Accounts_Countries_CountryId
UQ_Accounts_Code
```

Recommended indexes:

```text
IX_Accounts_CountryId
IX_Accounts_IsActive
```

---

## 9.4 Campaigns

Stores operational workloads inside accounts.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| AccountId | uniqueidentifier | Yes | FK to Accounts. |
| CountryId | uniqueidentifier | Yes | FK to Countries. |
| Name | nvarchar(200) | Yes | Campaign name. |
| Code | nvarchar(50) | Yes | Campaign code. |
| Description | nvarchar(500) null | No | Campaign description. |
| IsActive | bit | Yes | Active status. |
| CreatedAt | datetime2 | Yes | Creation timestamp. |
| UpdatedAt | datetime2 null | No | Last update timestamp. |

Recommended constraints:

```text
PK_Campaigns(Id)
FK_Campaigns_Accounts_AccountId
FK_Campaigns_Countries_CountryId
UQ_Campaigns_Code
```

Recommended indexes:

```text
IX_Campaigns_AccountId
IX_Campaigns_CountryId
IX_Campaigns_IsActive
```

Design note:

```text
CountryId is stored on Campaigns to preserve the operating country context for the campaign,
even though country can also be reached through Account → Country.
```

---

# 10. User Scope and Assignment Tables

## 10.1 UserScopes

Defines the operational scope assigned to a user.

This table supports manager, supervisor, agent, and viewer scope restrictions.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| UserId | uniqueidentifier | Yes | FK to Users. |
| ScopeType | nvarchar(50) | Yes | Region, Country, Account, or Campaign. |
| RegionId | uniqueidentifier null | No | FK to Regions. |
| CountryId | uniqueidentifier null | No | FK to Countries. |
| AccountId | uniqueidentifier null | No | FK to Accounts. |
| CampaignId | uniqueidentifier null | No | FK to Campaigns. |
| IsActive | bit | Yes | Active scope flag. |
| CreatedAt | datetime2 | Yes | Assignment timestamp. |
| CreatedByUserId | uniqueidentifier null | No | Admin who assigned the scope. |

Recommended constraints:

```text
PK_UserScopes(Id)
FK_UserScopes_Users_UserId
FK_UserScopes_Regions_RegionId
FK_UserScopes_Countries_CountryId
FK_UserScopes_Accounts_AccountId
FK_UserScopes_Campaigns_CampaignId
```

Recommended indexes:

```text
IX_UserScopes_UserId
IX_UserScopes_ScopeType
IX_UserScopes_RegionId
IX_UserScopes_CountryId
IX_UserScopes_AccountId
IX_UserScopes_CampaignId
IX_UserScopes_IsActive
```

Validation rule:

```text
Exactly one scope target should be populated according to ScopeType.
```

Example:

```text
ScopeType = Region
RegionId = not null
CountryId = null
AccountId = null
CampaignId = null
```

---

## 10.2 SupervisorAssignments

Stores supervisor assignment to accounts or campaigns.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| SupervisorUserId | uniqueidentifier | Yes | FK to Users. |
| AccountId | uniqueidentifier | Yes | FK to Accounts. |
| CampaignId | uniqueidentifier null | No | Optional FK to Campaigns. |
| IsActive | bit | Yes | Active assignment flag. |
| CreatedAt | datetime2 | Yes | Assignment timestamp. |
| CreatedByUserId | uniqueidentifier null | No | Admin who created assignment. |

Recommended constraints:

```text
PK_SupervisorAssignments(Id)
FK_SupervisorAssignments_Users_SupervisorUserId
FK_SupervisorAssignments_Accounts_AccountId
FK_SupervisorAssignments_Campaigns_CampaignId
```

Recommended indexes:

```text
IX_SupervisorAssignments_SupervisorUserId
IX_SupervisorAssignments_AccountId
IX_SupervisorAssignments_CampaignId
IX_SupervisorAssignments_IsActive
```

---

## 10.3 AgentAssignments

Stores agent assignment to accounts or campaigns.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| AgentUserId | uniqueidentifier | Yes | FK to Users. |
| AccountId | uniqueidentifier | Yes | FK to Accounts. |
| CampaignId | uniqueidentifier null | No | Optional FK to Campaigns. |
| SupervisorUserId | uniqueidentifier null | No | FK to Users. |
| IsActive | bit | Yes | Active assignment flag. |
| CreatedAt | datetime2 | Yes | Assignment timestamp. |
| CreatedByUserId | uniqueidentifier null | No | Admin who created assignment. |

Recommended constraints:

```text
PK_AgentAssignments(Id)
FK_AgentAssignments_Users_AgentUserId
FK_AgentAssignments_Accounts_AccountId
FK_AgentAssignments_Campaigns_CampaignId
FK_AgentAssignments_Users_SupervisorUserId
```

Recommended indexes:

```text
IX_AgentAssignments_AgentUserId
IX_AgentAssignments_SupervisorUserId
IX_AgentAssignments_AccountId
IX_AgentAssignments_CampaignId
IX_AgentAssignments_IsActive
```

---

## 10.4 ManagerAssignments

Stores manager assignment to one or more regions.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| ManagerUserId | uniqueidentifier | Yes | FK to Users. |
| RegionId | uniqueidentifier | Yes | FK to Regions. |
| IsActive | bit | Yes | Active assignment flag. |
| CreatedAt | datetime2 | Yes | Assignment timestamp. |
| CreatedByUserId | uniqueidentifier null | No | Admin who created assignment. |

Recommended constraints:

```text
PK_ManagerAssignments(Id)
FK_ManagerAssignments_Users_ManagerUserId
FK_ManagerAssignments_Regions_RegionId
```

Recommended indexes:

```text
IX_ManagerAssignments_ManagerUserId
IX_ManagerAssignments_RegionId
IX_ManagerAssignments_IsActive
```

---

# 11. Customer Management Tables

## 11.1 Customers

Stores customer information used for ticket context.

Customers do not directly authenticate into OpsSphere in the initial version.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| AccountId | uniqueidentifier | Yes | FK to Accounts. |
| FirstName | nvarchar(100) | Yes | Customer first name. |
| LastName | nvarchar(100) | Yes | Customer last name. |
| Email | nvarchar(256) null | No | Customer email. |
| PhoneNumber | nvarchar(50) null | No | Customer phone number. |
| ExternalReference | nvarchar(100) null | No | Optional external customer reference. |
| IsActive | bit | Yes | Active status. |
| CreatedAt | datetime2 | Yes | Creation timestamp. |
| UpdatedAt | datetime2 null | No | Last update timestamp. |

Recommended constraints:

```text
PK_Customers(Id)
FK_Customers_Accounts_AccountId
```

Recommended indexes:

```text
IX_Customers_AccountId
IX_Customers_Email
IX_Customers_ExternalReference
IX_Customers_IsActive
```

Soft delete strategy:

```text
Customers linked to tickets should not be physically deleted.
They should be deactivated or soft-deleted while preserving ticket history.
```

---

# 12. Ticket Management Tables

## 12.1 Tickets

Stores the main operational support case.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| TicketNumber | nvarchar(50) | Yes | Human-readable unique ticket number. |
| CustomerId | uniqueidentifier | Yes | FK to Customers. |
| RegionId | uniqueidentifier | Yes | Denormalized operational context for reporting. |
| CountryId | uniqueidentifier | Yes | Denormalized operational context for reporting. |
| AccountId | uniqueidentifier | Yes | FK to Accounts. |
| CampaignId | uniqueidentifier | Yes | FK to Campaigns. |
| CreatedByUserId | uniqueidentifier | Yes | FK to Users. |
| AssignedAgentUserId | uniqueidentifier null | No | FK to Users. |
| SupervisorUserId | uniqueidentifier null | No | FK to Users. |
| Category | nvarchar(100) | Yes | Ticket category. |
| Priority | nvarchar(50) | Yes | Low, Normal, High, or Critical. |
| Status | nvarchar(50) | Yes | Open, Assigned, In Progress, Waiting for Customer, Escalated, Resolved, Closed. |
| Subject | nvarchar(200) | Yes | Short ticket subject. |
| Description | nvarchar(max) | Yes | Ticket description. |
| SlaState | nvarchar(50) | Yes | Within SLA, At Risk, Breached, or Completed. |
| SlaDueAt | datetime2 null | No | SLA deadline. |
| ResolvedAt | datetime2 null | No | Resolution timestamp. |
| ClosedAt | datetime2 null | No | Closure timestamp. |
| IsEscalated | bit | Yes | Indicates whether ticket is currently escalated. |
| CreatedAt | datetime2 | Yes | Ticket creation timestamp. |
| UpdatedAt | datetime2 null | No | Last update timestamp. |
| IsDeleted | bit | Yes | Soft delete flag if needed. |

Recommended constraints:

```text
PK_Tickets(Id)
UQ_Tickets_TicketNumber
FK_Tickets_Customers_CustomerId
FK_Tickets_Regions_RegionId
FK_Tickets_Countries_CountryId
FK_Tickets_Accounts_AccountId
FK_Tickets_Campaigns_CampaignId
FK_Tickets_Users_CreatedByUserId
FK_Tickets_Users_AssignedAgentUserId
FK_Tickets_Users_SupervisorUserId
```

Recommended indexes:

```text
IX_Tickets_TicketNumber
IX_Tickets_CustomerId
IX_Tickets_RegionId
IX_Tickets_CountryId
IX_Tickets_AccountId
IX_Tickets_CampaignId
IX_Tickets_AssignedAgentUserId
IX_Tickets_SupervisorUserId
IX_Tickets_Status
IX_Tickets_Priority
IX_Tickets_SlaState
IX_Tickets_SlaDueAt
IX_Tickets_IsEscalated
IX_Tickets_CreatedAt
IX_Tickets_AccountId_CampaignId_Status
IX_Tickets_AssignedAgentUserId_Status
IX_Tickets_SupervisorUserId_Status
IX_Tickets_SlaState_SlaDueAt
```

Design note:

```text
RegionId and CountryId are stored directly on Tickets for reporting and historical context preservation,
even though they can be derived through Campaign → Account → Country → Region.
```

This helps preserve the original operational context if the organizational structure changes later.

---

## 12.2 TicketComments

Stores internal comments linked to tickets.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| TicketId | uniqueidentifier | Yes | FK to Tickets. |
| AuthorUserId | uniqueidentifier | Yes | FK to Users. |
| Body | nvarchar(max) | Yes | Internal comment text. |
| CreatedAt | datetime2 | Yes | Comment timestamp. |
| IsDeleted | bit | Yes | Soft delete flag if moderation is needed. |
| DeletedAt | datetime2 null | No | Soft deletion timestamp. |

Recommended constraints:

```text
PK_TicketComments(Id)
FK_TicketComments_Tickets_TicketId
FK_TicketComments_Users_AuthorUserId
```

Recommended indexes:

```text
IX_TicketComments_TicketId
IX_TicketComments_AuthorUserId
IX_TicketComments_CreatedAt
```

Business rule:

```text
Internal comments must not be exposed to customers in the initial version.
```

---

## 12.3 TicketAssignments

Stores assignment and reassignment history.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| TicketId | uniqueidentifier | Yes | FK to Tickets. |
| PreviousAgentUserId | uniqueidentifier null | No | Previous assigned agent. |
| NewAgentUserId | uniqueidentifier | Yes | New assigned agent. |
| AssignedByUserId | uniqueidentifier | Yes | User who performed assignment. |
| AssignmentReason | nvarchar(500) null | No | Optional reason. |
| CreatedAt | datetime2 | Yes | Assignment timestamp. |

Recommended constraints:

```text
PK_TicketAssignments(Id)
FK_TicketAssignments_Tickets_TicketId
FK_TicketAssignments_Users_PreviousAgentUserId
FK_TicketAssignments_Users_NewAgentUserId
FK_TicketAssignments_Users_AssignedByUserId
```

Recommended indexes:

```text
IX_TicketAssignments_TicketId
IX_TicketAssignments_NewAgentUserId
IX_TicketAssignments_AssignedByUserId
IX_TicketAssignments_CreatedAt
```

---

## 12.4 TicketStatusHistory

Stores ticket status transitions.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| TicketId | uniqueidentifier | Yes | FK to Tickets. |
| PreviousStatus | nvarchar(50) null | No | Previous status. |
| NewStatus | nvarchar(50) | Yes | New status. |
| ChangedByUserId | uniqueidentifier | Yes | User who changed status. |
| ChangeReason | nvarchar(500) null | No | Optional reason. |
| CreatedAt | datetime2 | Yes | Status change timestamp. |

Recommended constraints:

```text
PK_TicketStatusHistory(Id)
FK_TicketStatusHistory_Tickets_TicketId
FK_TicketStatusHistory_Users_ChangedByUserId
```

Recommended indexes:

```text
IX_TicketStatusHistory_TicketId
IX_TicketStatusHistory_NewStatus
IX_TicketStatusHistory_ChangedByUserId
IX_TicketStatusHistory_CreatedAt
```

---

## 12.5 TicketEscalations

Stores escalation records.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| TicketId | uniqueidentifier | Yes | FK to Tickets. |
| EscalatedByUserId | uniqueidentifier | Yes | User who escalated the ticket. |
| EscalationReason | nvarchar(1000) | Yes | Reason for escalation. |
| ReviewedByUserId | uniqueidentifier null | No | Supervisor or manager who reviewed escalation. |
| ReviewNotes | nvarchar(1000) null | No | Optional review notes. |
| ResolvedAt | datetime2 null | No | Escalation resolution timestamp. |
| CreatedAt | datetime2 | Yes | Escalation timestamp. |
| IsActive | bit | Yes | Indicates whether escalation is still active. |

Recommended constraints:

```text
PK_TicketEscalations(Id)
FK_TicketEscalations_Tickets_TicketId
FK_TicketEscalations_Users_EscalatedByUserId
FK_TicketEscalations_Users_ReviewedByUserId
```

Recommended indexes:

```text
IX_TicketEscalations_TicketId
IX_TicketEscalations_EscalatedByUserId
IX_TicketEscalations_ReviewedByUserId
IX_TicketEscalations_IsActive
IX_TicketEscalations_CreatedAt
```

---

## 12.6 TicketResolutions

Stores resolution details.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| TicketId | uniqueidentifier | Yes | FK to Tickets. |
| ResolvedByUserId | uniqueidentifier | Yes | User who resolved the ticket. |
| ResolutionSummary | nvarchar(max) | Yes | Resolution details. |
| ResolutionCode | nvarchar(100) null | No | Optional resolution classification. |
| FinalSlaState | nvarchar(50) | Yes | Final SLA outcome. |
| CreatedAt | datetime2 | Yes | Resolution timestamp. |

Recommended constraints:

```text
PK_TicketResolutions(Id)
FK_TicketResolutions_Tickets_TicketId
FK_TicketResolutions_Users_ResolvedByUserId
UQ_TicketResolutions_TicketId
```

Recommended indexes:

```text
IX_TicketResolutions_ResolvedByUserId
IX_TicketResolutions_FinalSlaState
IX_TicketResolutions_CreatedAt
```

Design rule:

```text
A ticket should have at most one active resolution record.
```

---

# 13. SLA Management Tables

## 13.1 SlaPolicies

Stores SLA target rules.

The MVP uses a simple SLA model.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| AccountId | uniqueidentifier null | No | Optional FK to Accounts. |
| CampaignId | uniqueidentifier null | No | Optional FK to Campaigns. |
| Priority | nvarchar(50) | Yes | Ticket priority. |
| TargetHours | int | Yes | SLA target duration in hours. |
| AtRiskThresholdPercent | int | Yes | Percentage of target time when ticket becomes at risk. |
| IsActive | bit | Yes | Active policy flag. |
| CreatedAt | datetime2 | Yes | Creation timestamp. |
| UpdatedAt | datetime2 null | No | Last update timestamp. |

Recommended constraints:

```text
PK_SlaPolicies(Id)
FK_SlaPolicies_Accounts_AccountId
FK_SlaPolicies_Campaigns_CampaignId
```

Recommended indexes:

```text
IX_SlaPolicies_AccountId
IX_SlaPolicies_CampaignId
IX_SlaPolicies_Priority
IX_SlaPolicies_IsActive
IX_SlaPolicies_AccountId_CampaignId_Priority
```

MVP rule:

```text
Start with priority-based SLA policies.
Account and campaign-specific SLA policies may be used when needed.
```

---

## 13.2 TicketSlaStates

Stores detailed SLA tracking per ticket.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| TicketId | uniqueidentifier | Yes | FK to Tickets. |
| SlaPolicyId | uniqueidentifier null | No | FK to SlaPolicies. |
| StartedAt | datetime2 | Yes | SLA start timestamp. |
| DueAt | datetime2 | Yes | SLA deadline. |
| State | nvarchar(50) | Yes | Within SLA, At Risk, Breached, Completed. |
| LastEvaluatedAt | datetime2 null | No | Last SLA evaluation timestamp. |
| CompletedAt | datetime2 null | No | Completion timestamp. |
| FinalState | nvarchar(50) null | No | Final SLA result after resolution or closure. |

Recommended constraints:

```text
PK_TicketSlaStates(Id)
FK_TicketSlaStates_Tickets_TicketId
FK_TicketSlaStates_SlaPolicies_SlaPolicyId
UQ_TicketSlaStates_TicketId
```

Recommended indexes:

```text
IX_TicketSlaStates_TicketId
IX_TicketSlaStates_State
IX_TicketSlaStates_DueAt
IX_TicketSlaStates_State_DueAt
IX_TicketSlaStates_LastEvaluatedAt
```

Design note:

```text
Tickets.SlaState and Tickets.SlaDueAt may duplicate current SLA state for faster dashboard filtering.
TicketSlaStates keeps the detailed SLA tracking record.
```

---

# 14. Audit Tables

## 14.1 AuditLogs

Stores immutable audit records for critical business actions.

| Column | Type | Required | Description |
|---|---:|:---:|---|
| Id | uniqueidentifier | Yes | Primary key. |
| ActorUserId | uniqueidentifier null | No | User who performed the action. Null allowed for system actions. |
| Action | nvarchar(150) | Yes | Action performed. |
| EntityType | nvarchar(150) | Yes | Affected entity type. |
| EntityId | uniqueidentifier | Yes | Affected entity identifier. |
| PreviousValue | nvarchar(max) null | No | Previous value or JSON snapshot. |
| NewValue | nvarchar(max) null | No | New value or JSON snapshot. |
| IpAddress | nvarchar(100) null | No | Request IP address when available. |
| UserAgent | nvarchar(500) null | No | Request user agent when available. |
| CorrelationId | nvarchar(100) null | No | Request correlation identifier. |
| CreatedAt | datetime2 | Yes | Audit timestamp. |

Recommended constraints:

```text
PK_AuditLogs(Id)
FK_AuditLogs_Users_ActorUserId
```

Recommended indexes:

```text
IX_AuditLogs_ActorUserId
IX_AuditLogs_Action
IX_AuditLogs_EntityType_EntityId
IX_AuditLogs_CreatedAt
IX_AuditLogs_CorrelationId
```

Audit immutability rule:

```text
AuditLogs should not be updated or deleted by normal business workflows.
```

---

## 14.2 Audit-Sensitive Actions

The following actions should create audit records:

```text
UserCreated
UserUpdated
UserDeactivated
RoleAssigned
RoleRemoved
ScopeAssigned
ScopeUpdated
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
CustomerCreated
CustomerUpdated
TicketCreated
TicketAssigned
TicketReassigned
TicketStatusChanged
TicketPriorityChanged
TicketCommentAdded
TicketEscalated
TicketResolved
TicketClosed
SlaStateChanged
```

---

# 15. Reporting and Dashboard Support

## 15.1 Reporting-Ready Ticket Fields

The `Tickets` table intentionally stores the following reporting-ready fields:

```text
RegionId
CountryId
AccountId
CampaignId
AssignedAgentUserId
SupervisorUserId
CustomerId
Status
Priority
SlaState
SlaDueAt
CreatedAt
UpdatedAt
ResolvedAt
ClosedAt
IsEscalated
```

This supports filters such as:

```text
Tickets by region
Tickets by country
Tickets by account
Tickets by campaign
Tickets by supervisor
Tickets by agent
Tickets by status
Tickets by priority
Tickets by SLA state
Overdue tickets
Open tickets
Escalated tickets
Ticket aging
```

---

## 15.2 Suggested Reporting Views

The following database views may be added later:

```text
vw_TicketOperationalSummary
vw_SlaDashboardSummary
vw_AgentWorkloadSummary
vw_SupervisorWorkloadSummary
vw_AuditTrailSummary
```

These views are optional for MVP but useful for dashboards and exports.

---

# 16. Soft Delete and Deactivation Strategy

## 16.1 Deactivation Preferred for Operational Structure

The following records should normally be deactivated instead of physically deleted:

```text
Users
Roles
Regions
Countries
Accounts
Campaigns
UserScopes
ManagerAssignments
SupervisorAssignments
AgentAssignments
Customers
SlaPolicies
```

Reason:

```text
Historical tickets, assignments, audit logs, and reports may depend on these records.
```

---

## 16.2 Soft Delete Candidates

Soft delete may apply to:

```text
Customers
TicketComments
Tickets
```

However, ticket deletion should be restricted.

Recommended rule:

```text
Tickets should almost never be physically deleted.
If removal is required, use IsDeleted and preserve audit history.
```

---

## 16.3 Tables That Should Not Be Soft Deleted

The following tables should preserve history and should not be modified by normal delete flows:

```text
AuditLogs
TicketAssignments
TicketStatusHistory
TicketEscalations
TicketResolutions
TicketSlaStates
```

---

# 17. Initial Relationship Summary

```text
Region 1 ── * Country
Country 1 ── * Account
Country 1 ── * Campaign
Account 1 ── * Campaign
Account 1 ── * Customer
Customer 1 ── * Ticket
Account 1 ── * Ticket
Campaign 1 ── * Ticket

User * ── * Role
Role * ── * Permission
User 1 ── * UserScope

User 1 ── * ManagerAssignment as Manager
Region 1 ── * ManagerAssignment

User 1 ── * SupervisorAssignment as Supervisor
Account 1 ── * SupervisorAssignment
Campaign 1 ── * SupervisorAssignment

User 1 ── * AgentAssignment as Agent
Account 1 ── * AgentAssignment
Campaign 1 ── * AgentAssignment

User 1 ── * Ticket as CreatedByUser
User 1 ── * Ticket as AssignedAgent
User 1 ── * Ticket as Supervisor

Ticket 1 ── * TicketComment
Ticket 1 ── * TicketAssignment
Ticket 1 ── * TicketStatusHistory
Ticket 1 ── * TicketEscalation
Ticket 1 ── 0..1 TicketResolution
Ticket 1 ── 0..1 TicketSlaState

SlaPolicy 1 ── * TicketSlaState
User 1 ── * AuditLog as Actor
```

---

# 18. Initial Index Strategy

## 18.1 Authentication and Access Indexes

```text
IX_Users_Email
IX_Users_IsActive
IX_UserRoles_RoleId
IX_UserScopes_UserId
IX_UserScopes_ScopeType
IX_UserScopes_AccountId
IX_UserScopes_CampaignId
```

---

## 18.2 Organization Indexes

```text
IX_Countries_RegionId
IX_Accounts_CountryId
IX_Campaigns_AccountId
IX_Campaigns_CountryId
IX_ManagerAssignments_ManagerUserId
IX_ManagerAssignments_RegionId
IX_SupervisorAssignments_SupervisorUserId
IX_SupervisorAssignments_AccountId
IX_SupervisorAssignments_CampaignId
IX_AgentAssignments_AgentUserId
IX_AgentAssignments_SupervisorUserId
IX_AgentAssignments_AccountId
IX_AgentAssignments_CampaignId
```

---

## 18.3 Ticket Indexes

```text
IX_Tickets_TicketNumber
IX_Tickets_CustomerId
IX_Tickets_RegionId
IX_Tickets_CountryId
IX_Tickets_AccountId
IX_Tickets_CampaignId
IX_Tickets_AssignedAgentUserId
IX_Tickets_SupervisorUserId
IX_Tickets_Status
IX_Tickets_Priority
IX_Tickets_SlaState
IX_Tickets_SlaDueAt
IX_Tickets_IsEscalated
IX_Tickets_CreatedAt
IX_Tickets_AccountId_CampaignId_Status
IX_Tickets_AssignedAgentUserId_Status
IX_Tickets_SupervisorUserId_Status
IX_Tickets_SlaState_SlaDueAt
```

---

## 18.4 History and Audit Indexes

```text
IX_TicketComments_TicketId
IX_TicketAssignments_TicketId
IX_TicketStatusHistory_TicketId
IX_TicketEscalations_TicketId
IX_TicketEscalations_IsActive
IX_TicketResolutions_FinalSlaState
IX_TicketSlaStates_State_DueAt
IX_AuditLogs_ActorUserId
IX_AuditLogs_EntityType_EntityId
IX_AuditLogs_CreatedAt
IX_AuditLogs_CorrelationId
```

---

# 19. Data Integrity Rules

The database should enforce structural consistency through:

```text
Primary keys
Foreign keys
Unique constraints
Required columns
Check constraints where practical
Indexes for common filters
```

Recommended check constraints:

```text
CK_Tickets_Status
CK_Tickets_Priority
CK_Tickets_SlaState
CK_UserScopes_ScopeType
CK_TicketSlaStates_State
CK_SlaPolicies_TargetHours_Positive
CK_SlaPolicies_AtRiskThresholdPercent_Range
```

Example values:

```text
Ticket Status:
- Open
- Assigned
- In Progress
- Waiting for Customer
- Escalated
- Resolved
- Closed

Ticket Priority:
- Low
- Normal
- High
- Critical

SLA State:
- Within SLA
- At Risk
- Breached
- Completed
```

---

# 20. Entity Framework Core Implementation Notes

## 20.1 Recommended EF Core Mapping Strategy

Use explicit entity configurations:

```text
Infrastructure/
  Persistence/
    Configurations/
      UserConfiguration.cs
      RoleConfiguration.cs
      TicketConfiguration.cs
      CustomerConfiguration.cs
      AuditLogConfiguration.cs
```

---

## 20.2 Avoid Domain Pollution

Domain entities should not depend directly on EF Core infrastructure.

Avoid placing persistence-specific behavior inside domain rules.

---

## 20.3 Migrations

Migrations should be generated after the initial entity model is implemented.

Recommended migration naming:

```text
InitialCreate
AddTicketManagement
AddSlaTracking
AddAuditLogs
AddUserScopes
```

---

# 21. ERD Creation Guidance

The ERD should include the main tables and relationships.

Minimum ERD content:

```text
Users
Roles
Permissions
UserRoles
RolePermissions
UserScopes
Regions
Countries
Accounts
Campaigns
ManagerAssignments
SupervisorAssignments
AgentAssignments
Customers
Tickets
TicketComments
TicketAssignments
TicketStatusHistory
TicketEscalations
TicketResolutions
SlaPolicies
TicketSlaStates
AuditLogs
```

Recommended visual grouping:

```text
Identity and Access
Organization Structure
Customer Management
Ticket Management
SLA Management
Audit
```

Recommended ERD rule:

```text
Keep the ERD readable.
If the full ERD becomes too dense, create smaller diagrams later for ticket model, security model, and organization model.
```

---

# 22. Future Database Considerations

The following database areas may be added in future phases:

```text
Notifications
Attachments
SavedFilters
ReportExports
PowerBiExportJobs
IntegrationEvents
Webhooks
RefreshTokens
LoginAuditEvents
PasswordResetTokens
```

These are not required for the initial database design.

---

# 23. Related Documents

| Document | Relationship |
|---|---|
| `docs/06-requirements.md` | Defines functional and non-functional requirements that the database must support. |
| `docs/07-use-cases.md` | Defines workflows that require database persistence. |
| `docs/08-business-process-flows.md` | Defines process behavior and audit points. |
| `docs/09-business-rules.md` | Defines constraints, workflow rules, audit rules, and scope rules. |
| `docs/10-domain-model.md` | Defines domain concepts represented in the database model. |
| `docs/11-architecture-overview.md` | Defines SQL Server and EF Core as part of the technical architecture. |
| `docs/12-c4-architecture.md` | Shows SQL Server as the primary data store in the C4 diagrams. |
| `docs/13-uml-diagrams.md` | Provides class/domain diagrams that inform table design. |
| `docs/15-api-design.md` | Defines endpoints that read and write database records. |
| `docs/16-security-and-permissions.md` | Defines authorization, role, permission, and audit requirements. |

---

# 24. Document Summary

This document defines the initial database design for OpsSphere.

The design uses SQL Server and Entity Framework Core to support a structured enterprise operations platform.

The initial schema covers identity, roles, permissions, operational scope, organization structure, customers, tickets, comments, assignments, status history, escalations, resolutions, SLA tracking, audit logs, dashboard filtering, and reporting-ready data.

The design favors historical traceability through audit logging, status history, assignment history, SLA records, and deactivation instead of physical deletion for records that are referenced by historical operational data.

The ERD should be exported as:

```text
docs/diagrams/database/opssphere-erd.png
```