# Software Requirements Specification

## Document Information

| Field | Value |
|---|---|
| Project | OpsSphere |
| Document | Software Requirements Specification |
| File | docs/06-requirements.md |
| Formal PDF | docs/formal-specification/OpsSphere-Software-Requirements-Specification.pdf |
| Version | 1.0 |
| Status | Draft |
| Project Type | Enterprise Support Operations Platform |
| Business Context | Multinational BPO / Contact Center Operations |

## 1. Introduction

OpsSphere is an enterprise-grade operations platform designed for multinational BPO/contact center environments.

The platform supports operational execution across regions, countries, accounts, campaigns, managers, supervisors, agents, viewers, customers, tickets, SLAs, audit history, dashboards, and role-based access control.

OpsSphere is not intended to be a generic ticketing system. It is designed as an enterprise operations platform that connects business workflows, organizational structure, ticket ownership, SLA visibility, auditability, and structured operational data.

This document defines the functional requirements, non-functional requirements, business rules, acceptance criteria, and glossary for the initial version of OpsSphere.

## 2. Objective

The objective of this document is to define what OpsSphere must do and the quality conditions it must satisfy.

This document serves as the requirements baseline for:

- Use case definition.
- Business process modeling.
- Domain modeling.
- Database design.
- API design.
- Security and permissions design.
- Frontend workflow design.
- Testing strategy.
- MVP implementation.
- Formal PDF specification.

## 3. Product Scope

OpsSphere focuses on the operational management layer of a BPO/contact center organization.

The initial scope includes:

- Authentication.
- Role-based access control.
- User and role management.
- Region management.
- Country management.
- Account management.
- Campaign management.
- Manager management.
- Supervisor management.
- Agent management.
- Viewer management.
- Customer management.
- Ticket lifecycle management.
- Ticket assignment.
- Ticket status workflow.
- Ticket priority.
- SLA tracking.
- Internal comments.
- Escalation visibility.
- Audit history.
- Basic operational dashboards.
- Filtered operational views.
- Structured operational data for external reporting tools.

The initial scope does not include a full business intelligence platform, advanced Power BI dashboard creation, telephony integration, workforce management, payroll, HR, AI ticket classification, customer portal, mobile application, or omnichannel messaging integrations.

## 4. Business Context

OpsSphere is modeled around a multinational BPO/contact center organization.

The organization operates through the following structure:

```text
Multinational BPO / Contact Center
  → Region
    → Country
      → Account
        → Campaign
          → Supervisor
            → Agent
```

Administrators maintain the structure of the platform:

**Administrator**
- Manages Regions
- Manages Countries
- Manages Accounts
- Manages Campaigns
- Manages Managers
- Manages Supervisors
- Manages Agents
- Manages Viewers
- Manages Roles
- Manages Permissions

OpsSphere must support this operational structure so tickets, SLAs, dashboards, permissions, and audit history can be understood in the correct business context.

## 5. Stakeholders

The main stakeholders are:

| Stakeholder | Description |
|---|---|
| Admin | Configures and maintains the platform structure |
| Operations Manager | Oversees performance across one or more regions |
| Supervisor | Manages agents and tickets within assigned accounts or campaigns |
| Agent | Handles tickets within assigned operational scope |
| Viewer | Reviews operational information in read-only mode |
| Customer | Person or entity associated with a support case |
| Technical Owner | Designs, builds, tests, and maintains the platform |

## 6. Definitions

| Term | Definition |
|---|---|
| Region | Large geographic operating area, such as Latin America or North America |
| Country | National operation within a region |
| Account | Client account served by the BPO/contact center |
| Campaign | Operational service or support function inside an account |
| Manager | User responsible for overseeing one or more regions |
| Supervisor | User responsible for overseeing agents and tickets within an account or campaign |
| Agent | User responsible for handling tickets |
| Viewer | Read-only user who accesses operational information |
| Customer | Person or entity associated with a support request |
| Ticket | Operational support case handled by agents and supervisors |
| SLA | Service Level Agreement target associated with a ticket |
| SLA State | Current SLA condition of a ticket, such as Within SLA, At Risk, or Breached |
| Escalation | Action used when a ticket requires supervisor or higher-level attention |
| Audit History | Record of important changes made in the system |
| Operational Dashboard | Basic visibility screen for ticket, SLA, workload, and operational status |
| RBAC | Role-Based Access Control |
| Scope | Operational visibility boundary assigned to a user |
| Power BI | External business intelligence tool used for advanced reporting outside the core application |

## 7. User Roles

OpsSphere must support the following user roles:

| Role | Description |
|---|---|
| Admin | Full administrative access to configure structure, users, roles, and permissions |
| Operations Manager | Regional oversight access |
| Supervisor | Account or campaign-level operational access |
| Agent | Ticket handling access within assigned scope |
| Viewer | Read-only access to operational data |
| Technical Owner | Implementation role, not a business runtime role |
| Customer | Linked to tickets but does not directly access the initial system |

## 8. Functional Requirements

### 8.1 Authentication and Authorization Requirements

- **FR-AUTH-001: User Login**: The system shall allow registered users to authenticate using valid credentials.
- **FR-AUTH-002: JWT Authentication**: The system shall use JWT-based authentication for API access.
- **FR-AUTH-003: Protected Resources**: The system shall prevent unauthenticated users from accessing protected application features.
- **FR-AUTH-004: Role-Based Access Control**: The system shall enforce role-based access control for Admin, Operations Manager, Supervisor, Agent, and Viewer roles.
- **FR-AUTH-005: Scope-Based Visibility**: The system shall restrict user visibility based on assigned operational scope.
- **FR-AUTH-006: Unauthorized Access Handling**: The system shall return an appropriate authorization error when users attempt to access resources outside their role or scope.
- **FR-AUTH-007: Session Expiration**: The system shall expire user sessions according to configured token expiration rules.

### 8.2 User and Role Management Requirements

- **FR-USER-001: Manage Users**: The system shall allow Admin users to create, view, update, deactivate, and manage users.
- **FR-USER-002: Manage Roles**: The system shall allow Admin users to assign roles to users.
- **FR-USER-008: Deactivate Users**: The system shall allow Admin users to deactivate users who no longer belong to the operation.
- **FR-USER-010: User Audit History**: The system shall record important user management changes in audit history.

### 8.3 Operational Structure Requirements

- **FR-ORG-001: Manage Regions**: The system shall allow Admin users to create, view, update, and deactivate regions.
- **FR-ORG-002: Manage Countries**: The system shall allow Admin users to create, view, update, and deactivate countries.
- **FR-ORG-003: Assign Countries to Regions**: The system shall allow Admin users to assign countries to regions.
- **FR-ORG-004: Manage Accounts**: The system shall allow Admin users to create, view, update, and deactivate accounts.
- **FR-ORG-005: Manage Campaigns**: The system shall allow Admin users to create, view, update, and deactivate campaigns.
- **FR-ORG-014: Operational Structure Audit History**: The system shall record important changes to regions, countries, accounts, campaigns, assignments, roles, and permissions in audit history.

### 8.5 Ticket Management Requirements

- **FR-TICKET-001: Create Ticket**: The system shall allow authorized users to create tickets.
- **FR-TICKET-002: Ticket Required Fields**: The system shall require the following information when creating a ticket: Customer, Account, Campaign, Category, Priority, Description, Created by user.
- **FR-TICKET-008: Change Ticket Status**: The system shall allow authorized users to change ticket status according to valid workflow transitions.
- **FR-TICKET-011: Resolve Ticket**: The system shall allow authorized users to mark tickets as resolved.
- **FR-TICKET-012: Close Ticket**: The system shall allow authorized users to close resolved tickets.

### 8.8 SLA Requirements

- **FR-SLA-001: Assign SLA Target**: The system shall assign an SLA target to each ticket.
- **FR-SLA-002: SLA State**: The system shall calculate or store an SLA state (Within SLA, At Risk, Breached, Completed).

### 8.10 Audit History Requirements

- **FR-AUDIT-008: Audit Event Metadata**: Each audit event shall include Actor, Timestamp, Entity type, Entity identifier, Action performed, Previous value, and New value.

## 9. Non-Functional Requirements

### 9.1 Security Requirements
- **NFR-SEC-001**: The system shall require authentication for protected features.
- **NFR-SEC-005**: The system shall store passwords securely using appropriate hashing mechanisms.

### 9.3 Maintainability Requirements
- **NFR-MAINT-001**: The backend shall follow Clean Architecture principles.

### 9.7 Testability Requirements
- **NFR-TEST-001**: The system shall include unit tests for important business logic.

## 10. Business Rules

### 10.1 Organizational Structure Rules
- **BR-ORG-001**: A country must belong to one region.
- **BR-ORG-003**: A campaign must belong to one account.

### 10.2 Ticket Rules
- **BR-TICKET-008**: A ticket must be resolved before it can be closed.

### 10.3 SLA Rules
- **BR-SLA-006**: The initial SLA model should remain simple and avoid advanced business-hour calendar logic.

## 11. Acceptance Criteria

### 11.1 Authentication Acceptance Criteria
- Users can log in with valid credentials.
- Deactivated users cannot access the system.

## 12. Out of Scope

- Full business intelligence platform.
- AI-assisted ticket classification.
- Mobile application.

## 14. Glossary

| Term | Meaning |
|---|---|
| MVP | Minimum Viable Product |
| RBAC | Role-Based Access Control |

## 15. Formal PDF Version

This Markdown document is the living requirements document for GitHub.