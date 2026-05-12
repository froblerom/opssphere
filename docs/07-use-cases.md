# Use Cases

## Document Information

| Field | Value |
|---|---|
| Project | OpsSphere |
| Document | Use Cases |
| File | `docs/07-use-cases.md` |
| Version | 1.0 |
| Status | Draft |
| Project Type | Enterprise Support Operations Platform |
| Business Context | Multinational BPO / Contact Center Operations |

---

## 1. Purpose

This document defines the initial use cases for OpsSphere.

The purpose of this document is to describe how users interact with the system, what goals they accomplish, what business rules apply, and what conditions must be satisfied before and after each use case.

This document connects:

- Business actors.
- Operational workflows.
- System behavior.
- Functional requirements.
- Business rules.
- Acceptance criteria.

Use cases help validate that OpsSphere is not only a collection of screens or database tables, but a structured operational platform that supports real business execution.

---

## 2. Scope

This document covers the initial use cases for the MVP version of OpsSphere.

The initial use cases are:

| Use Case ID | Use Case Name |
|---|---|
| UC-001 | Authenticate User |
| UC-002 | Create Ticket |
| UC-003 | Assign Ticket |
| UC-004 | Update Ticket Status |
| UC-005 | Add Internal Comment |
| UC-006 | Escalate Ticket |
| UC-007 | Resolve Ticket |
| UC-008 | Close Ticket |
| UC-009 | View SLA Dashboard |
| UC-010 | Manage Users |

---

## 3. Actor Summary

| Actor | Description |
|---|---|
| Admin | Configures users, roles, permissions, and operational structure. |
| Operations Manager | Oversees operational performance across assigned regions. |
| Supervisor | Manages agents, tickets, assignments, escalations, and operational execution. |
| Agent | Handles assigned tickets and updates ticket progress. |
| Viewer | Reviews operational information in read-only mode. |
| Customer | Person or entity associated with a support case. Customers do not directly access the initial system. |
| System | Automated platform behavior such as validation, authorization, audit logging, and SLA state handling. |

---

## 4. Use Case Format

Each use case follows the structure below:

- Actor
- Goal
- Preconditions
- Main Flow
- Alternative Flows
- Business Rules
- Postconditions
- Acceptance Criteria

---

# UC-001: Authenticate User

## Actor

Primary actor:

- Admin
- Operations Manager
- Supervisor
- Agent
- Viewer

Supporting actor:

- System

## Goal

Allow a registered internal user to securely access OpsSphere using valid credentials.

## Preconditions

- The user exists in the system.
- The user account is active.
- The user has assigned credentials.
- The authentication service is available.
- The user has at least one assigned role.

## Main Flow

1. The user opens the OpsSphere login page.
2. The system displays the login form.
3. The user enters valid credentials.
4. The user submits the login form.
5. The system validates the credentials.
6. The system verifies that the user account is active.
7. The system identifies the user's assigned role.
8. The system generates a valid authentication token.
9. The system redirects the user to the appropriate landing page based on role and scope.
10. The system records the successful login event if login auditing is enabled.

## Alternative Flows

### AF-001: Invalid Credentials

1. The user enters invalid credentials.
2. The system rejects the authentication attempt.
3. The system displays a generic login error.
4. The user remains on the login page.

### AF-002: Deactivated User

1. The user enters valid credentials.
2. The system identifies that the user account is deactivated.
3. The system denies access.
4. The system displays an account access error.
5. The system does not generate an authentication token.

### AF-003: Missing Role

1. The user enters valid credentials.
2. The system authenticates the user.
3. The system detects that the user has no assigned role.
4. The system denies access to protected modules.
5. The system displays an authorization error or redirects the user to a restricted access page.

### AF-004: Expired Session

1. The user's session token expires.
2. The user attempts to access a protected feature.
3. The system rejects the request.
4. The system redirects the user to the login page.

## Business Rules

- Only active users may authenticate.
- Deactivated users must not access protected features.
- Authentication tokens must be required for protected API endpoints.
- User access must be determined by role and operational scope.
- The system must not expose sensitive authentication failure details.

## Postconditions

Successful outcome:

- The user is authenticated.
- A valid session or token exists.
- The user can access features allowed by role and scope.

Failure outcome:

- The user is not authenticated.
- No valid session or token is created.
- The user cannot access protected features.

## Acceptance Criteria

- Users can log in with valid credentials.
- Users cannot log in with invalid credentials.
- Deactivated users cannot access the system.
- Protected API endpoints require valid authentication.
- Authenticated users are redirected according to role and scope.
- Expired sessions prevent access to protected features.

---

# UC-002: Create Ticket

## Actor

Primary actor:

- Agent
- Supervisor

Supporting actor:

- System

Related actor:

- Customer

## Goal

Allow an authorized internal user to create a ticket linked to a customer and operational context.

## Preconditions

- The user is authenticated.
- The user has permission to create tickets.
- The user belongs to a valid operational scope.
- The customer exists or can be created by the authorized user.
- The target account and campaign exist.
- Required ticket fields are available.

## Main Flow

1. The user opens the ticket creation module.
2. The system displays the ticket creation form.
3. The user selects or creates the customer record.
4. The user selects the account.
5. The user selects the campaign.
6. The user enters the ticket category.
7. The user selects the ticket priority.
8. The user enters the ticket description.
9. The user submits the ticket.
10. The system validates required fields.
11. The system validates that the user has permission within the selected operational scope.
12. The system creates the ticket with an initial status of `Open`.
13. The system associates the ticket with customer, account, campaign, and creator information.
14. The system assigns or calculates the initial SLA target and SLA state.
15. The system records ticket creation in audit history.
16. The system displays the created ticket.

## Alternative Flows

### AF-001: Missing Required Fields

1. The user submits the ticket without completing all required fields.
2. The system rejects the submission.
3. The system highlights the missing fields.
4. The user completes the missing fields.
5. The user submits the ticket again.

### AF-002: Unauthorized Operational Scope

1. The user selects an account or campaign outside their assigned scope.
2. The system detects the unauthorized scope.
3. The system prevents ticket creation.
4. The system displays an authorization error.

### AF-003: Customer Does Not Exist

1. The user searches for a customer.
2. The customer is not found.
3. The system allows the user to create a customer record if authorized.
4. The user creates the customer.
5. The system links the new customer to the ticket.

### AF-004: Automatic Assignment Rule Applies

1. The user submits the ticket.
2. The system detects an assignment rule for the selected campaign.
3. The system creates the ticket with an initial status of `Assigned`.
4. The system assigns the ticket to an eligible agent.
5. The system records ticket creation and assignment in audit history.

## Business Rules

- A ticket must be linked to a customer.
- A ticket must be linked to an account and campaign.
- A ticket must have a category.
- A ticket must have a priority.
- A ticket must have a description.
- A ticket must have a creator.
- New tickets must start as `Open` unless assignment rules set them directly to `Assigned`.
- Ticket creation must be recorded in audit history.
- Ticket visibility must respect role and operational scope.

## Postconditions

Successful outcome:

- A new ticket exists.
- The ticket is linked to a customer.
- The ticket has account and campaign context.
- The ticket has an initial status.
- The ticket has an SLA target or SLA state.
- An audit record exists.

Failure outcome:

- No ticket is created.
- The user is informed of the validation or authorization issue.

## Acceptance Criteria

- Authorized users can create tickets.
- Tickets require customer, account, campaign, category, priority, and description.
- Created tickets include operational context.
- Created tickets receive an initial status.
- Created tickets include SLA information.
- Ticket creation creates an audit record.
- Users cannot create tickets outside their operational scope.

---

# UC-003: Assign Ticket

## Actor

Primary actor:

- Supervisor

Supporting actor:

- System

Related actor:

- Agent

## Goal

Allow a supervisor to assign or reassign a ticket to an eligible agent within the correct operational scope.

## Preconditions

- The supervisor is authenticated.
- The supervisor has permission to assign tickets.
- The ticket exists.
- The ticket is within the supervisor's assigned operational scope.
- The target agent exists.
- The target agent is active.
- The target agent belongs to the correct account or campaign scope.
- The ticket is not closed.

## Main Flow

1. The supervisor opens the ticket detail page.
2. The system displays the current ticket information.
3. The supervisor selects the assignment action.
4. The system displays eligible agents for the ticket's account or campaign.
5. The supervisor selects an eligible agent.
6. The supervisor confirms the assignment.
7. The system validates the supervisor's permission.
8. The system validates that the selected agent is eligible.
9. The system assigns the ticket to the selected agent.
10. The system updates the ticket status to `Assigned` if applicable.
11. The system records the assignment in ticket history.
12. The system records the assignment in audit history.
13. The system displays the updated ticket assignment.

## Alternative Flows

### AF-001: Agent Outside Scope

1. The supervisor attempts to assign the ticket to an agent outside the ticket's operational scope.
2. The system rejects the assignment.
3. The system displays an eligibility error.

### AF-002: Ticket Already Closed

1. The supervisor attempts to assign a closed ticket.
2. The system detects that the ticket is closed.
3. The system prevents assignment.
4. The system displays a closed ticket restriction message.

### AF-003: Unauthorized Supervisor

1. A supervisor attempts to assign a ticket outside their assigned scope.
2. The system detects the unauthorized scope.
3. The system prevents assignment.
4. The system displays an authorization error.

### AF-004: Reassignment

1. The ticket is already assigned to an agent.
2. The supervisor selects another eligible agent.
3. The system validates the reassignment.
4. The system updates the assigned agent.
5. The system records the reassignment in ticket history and audit history.

## Business Rules

- A ticket can only be assigned to an eligible active agent.
- The agent must belong to the correct account or campaign scope.
- Supervisors can only assign tickets within their operational scope.
- Closed tickets cannot be assigned unless a reopening workflow is allowed.
- Assignment and reassignment must be recorded in audit history.
- Assignment should update the ticket status when appropriate.

## Postconditions

Successful outcome:

- The ticket has an assigned agent.
- The ticket status reflects assignment when applicable.
- Ticket history is updated.
- Audit history is updated.

Failure outcome:

- The ticket assignment remains unchanged.
- The user is informed of the validation or authorization issue.

## Acceptance Criteria

- Supervisors can assign tickets within their scope.
- Supervisors can reassign tickets within their scope.
- Tickets cannot be assigned to agents outside the correct operational scope.
- Closed tickets cannot be assigned.
- Assignment changes are recorded in audit history.
- Assignment changes are visible in ticket history.

---

# UC-004: Update Ticket Status

## Actor

Primary actor:

- Agent
- Supervisor

Supporting actor:

- System

## Goal

Allow an authorized user to move a ticket through valid workflow statuses.

## Preconditions

- The user is authenticated.
- The user has permission to update ticket status.
- The ticket exists.
- The ticket is within the user's operational scope.
- The ticket is not closed unless reopening is explicitly allowed.
- The requested status transition is valid.

## Main Flow

1. The user opens the ticket detail page.
2. The system displays the current ticket status.
3. The user selects a new status.
4. The system validates the user's role and scope.
5. The system validates the requested status transition.
6. The system updates the ticket status.
7. The system records the status change in ticket history.
8. The system records the status change in audit history.
9. The system displays the updated ticket status.

## Alternative Flows

### AF-001: Invalid Status Transition

1. The user selects a status that is not valid from the current ticket status.
2. The system rejects the transition.
3. The system displays an invalid workflow transition error.

### AF-002: Unauthorized Status Change

1. The user attempts to update a ticket outside their scope.
2. The system detects unauthorized access.
3. The system prevents the status change.
4. The system displays an authorization error.

### AF-003: Closed Ticket Modification

1. The user attempts to change the status of a closed ticket.
2. The system detects that the ticket is closed.
3. The system prevents the update unless a reopening workflow is allowed.
4. The system displays a closed ticket restriction message.

### AF-004: Status Requires Additional Data

1. The user selects a status that requires additional information.
2. The system prompts the user for the required information.
3. The user provides the information.
4. The system validates the information.
5. The system updates the ticket status.

## Business Rules

- Ticket status must follow valid workflow transitions.
- Status changes must be recorded in audit history.
- Closed tickets cannot be modified unless reopening is explicitly allowed.
- Users can only update tickets within their operational scope.
- Some status changes may require additional information.
- Tickets must be resolved before they can be closed.

## Postconditions

Successful outcome:

- The ticket has the new valid status.
- Ticket history includes the status change.
- Audit history includes the status change.

Failure outcome:

- The ticket status remains unchanged.
- The user is informed of the validation or authorization issue.

## Acceptance Criteria

- Authorized users can update ticket status.
- Invalid status transitions are rejected.
- Users cannot update tickets outside their scope.
- Closed ticket restrictions are enforced.
- Status changes are recorded in ticket history.
- Status changes are recorded in audit history.

---

# UC-005: Add Internal Comment

## Actor

Primary actor:

- Agent
- Supervisor
- Operations Manager

Supporting actor:

- System

## Goal

Allow authorized internal users to add comments to a ticket for operational collaboration and traceability.

## Preconditions

- The user is authenticated.
- The user has permission to view the ticket.
- The user has permission to add internal comments.
- The ticket exists.
- The ticket is within the user's operational scope.
- The ticket is not closed unless comments on closed tickets are allowed.

## Main Flow

1. The user opens the ticket detail page.
2. The system displays the ticket information and internal comments section.
3. The user enters an internal comment.
4. The user submits the comment.
5. The system validates that the comment is not empty.
6. The system validates the user's role and scope.
7. The system saves the internal comment.
8. The system records the comment author and timestamp.
9. The system records comment creation in audit history.
10. The system displays the new comment in the ticket comments section.

## Alternative Flows

### AF-001: Empty Comment

1. The user submits an empty comment.
2. The system rejects the comment.
3. The system displays a validation message.

### AF-002: Unauthorized User

1. The user attempts to comment on a ticket outside their scope.
2. The system detects unauthorized access.
3. The system prevents comment creation.
4. The system displays an authorization error.

### AF-003: Closed Ticket Comment Restriction

1. The user attempts to comment on a closed ticket.
2. The system checks whether comments are allowed on closed tickets.
3. If comments are not allowed, the system rejects the comment.
4. The system displays a closed ticket restriction message.

## Business Rules

- Internal comments are visible only to authorized internal users.
- Internal comments must not be exposed to customers in the initial version.
- Internal comments must include author and timestamp.
- Empty comments are not allowed.
- Comment creation must be recorded in audit history.
- Comment visibility must respect role and operational scope.

## Postconditions

Successful outcome:

- The internal comment is saved.
- The comment is linked to the ticket.
- The comment includes author and timestamp.
- Audit history is updated.

Failure outcome:

- No comment is saved.
- The user is informed of the validation or authorization issue.

## Acceptance Criteria

- Authorized users can add internal comments.
- Empty comments are rejected.
- Internal comments display author and timestamp.
- Internal comments are visible only to authorized internal users.
- Internal comments are not exposed to customers.
- Comment creation is recorded in audit history.

---

# UC-006: Escalate Ticket

## Actor

Primary actor:

- Agent
- Supervisor

Supporting actor:

- System

Related actor:

- Supervisor
- Operations Manager

## Goal

Allow an authorized user to escalate a ticket that requires higher-level attention.

## Preconditions

- The user is authenticated.
- The user has permission to escalate tickets.
- The ticket exists.
- The ticket is within the user's operational scope.
- The ticket is not closed.
- An escalation reason is available.

## Main Flow

1. The user opens the ticket detail page.
2. The user selects the escalate action.
3. The system displays the escalation form.
4. The user enters an escalation reason.
5. The user submits the escalation.
6. The system validates that the ticket is not closed.
7. The system validates that an escalation reason was provided.
8. The system validates the user's role and scope.
9. The system marks the ticket as escalated.
10. The system updates the ticket status to `Escalated` if applicable.
11. The system makes the ticket visible to authorized supervisors and managers within scope.
12. The system records the escalation in ticket history.
13. The system records the escalation in audit history.
14. The system displays the updated ticket escalation state.

## Alternative Flows

### AF-001: Missing Escalation Reason

1. The user submits the escalation without a reason.
2. The system rejects the escalation.
3. The system displays a validation message.
4. The user provides the escalation reason.
5. The system continues with escalation processing.

### AF-002: Closed Ticket

1. The user attempts to escalate a closed ticket.
2. The system detects that the ticket is closed.
3. The system rejects the escalation.
4. The system displays a closed ticket restriction message.

### AF-003: Unauthorized Escalation

1. The user attempts to escalate a ticket outside their scope.
2. The system detects unauthorized access.
3. The system prevents escalation.
4. The system displays an authorization error.

### AF-004: Already Escalated Ticket

1. The user attempts to escalate a ticket that is already escalated.
2. The system detects the existing escalation state.
3. The system either prevents duplicate escalation or appends a new escalation event based on configured business rules.
4. The system records the result if a new event is allowed.

## Business Rules

- Escalated tickets must include an escalation reason.
- Closed tickets cannot be escalated.
- Escalation must be visible to authorized supervisors and managers within scope.
- Escalation events must be recorded in ticket history.
- Escalation events must be recorded in audit history.
- Users can only escalate tickets within their operational scope.

## Postconditions

Successful outcome:

- The ticket is marked as escalated.
- The escalation reason is saved.
- Authorized supervisors and managers can see the escalation.
- Ticket history is updated.
- Audit history is updated.

Failure outcome:

- The ticket escalation state remains unchanged.
- The user is informed of the validation or authorization issue.

## Acceptance Criteria

- Authorized users can escalate tickets.
- Escalated tickets require an escalation reason.
- Closed tickets cannot be escalated.
- Escalated tickets are visible to supervisors within scope.
- Escalation events are recorded in ticket history.
- Escalation events are recorded in audit history.

---

# UC-007: Resolve Ticket

## Actor

Primary actor:

- Agent
- Supervisor

Supporting actor:

- System

## Goal

Allow an authorized user to mark a ticket as resolved after the issue has been handled.

## Preconditions

- The user is authenticated.
- The user has permission to resolve tickets.
- The ticket exists.
- The ticket is within the user's operational scope.
- The ticket is not closed.
- The ticket is in a status that allows resolution.

## Main Flow

1. The user opens the ticket detail page.
2. The system displays the current ticket status and information.
3. The user selects the resolve action.
4. The system displays the resolution form if resolution details are required.
5. The user enters resolution details if required.
6. The user confirms the resolution.
7. The system validates the user's role and scope.
8. The system validates that the ticket can be resolved from its current status.
9. The system updates the ticket status to `Resolved`.
10. The system updates the SLA state if applicable.
11. The system records the resolution in ticket history.
12. The system records the resolution in audit history.
13. The system displays the resolved ticket state.

## Alternative Flows

### AF-001: Invalid Status for Resolution

1. The user attempts to resolve a ticket from a status that does not allow resolution.
2. The system rejects the resolution.
3. The system displays an invalid workflow transition error.

### AF-002: Unauthorized Resolution

1. The user attempts to resolve a ticket outside their scope.
2. The system detects unauthorized access.
3. The system prevents the resolution.
4. The system displays an authorization error.

### AF-003: Closed Ticket

1. The user attempts to resolve an already closed ticket.
2. The system detects that the ticket is closed.
3. The system prevents the action.
4. The system displays a closed ticket restriction message.

### AF-004: Resolution Details Required

1. The user attempts to resolve a ticket without required resolution details.
2. The system rejects the resolution.
3. The system asks the user to complete the required information.
4. The user enters the required information.
5. The system continues with ticket resolution.

## Business Rules

- Tickets must be resolved before they can be closed.
- Closed tickets cannot be resolved again.
- Resolution must follow valid workflow transitions.
- Users can only resolve tickets within their operational scope.
- Resolution events must be recorded in ticket history.
- Resolution events must be recorded in audit history.
- Completed tickets should preserve their final SLA outcome.

## Postconditions

Successful outcome:

- The ticket status is `Resolved`.
- Resolution information is saved if applicable.
- SLA state is updated or preserved.
- Ticket history is updated.
- Audit history is updated.

Failure outcome:

- The ticket remains unchanged.
- The user is informed of the validation or authorization issue.

## Acceptance Criteria

- Authorized users can resolve tickets.
- Tickets cannot be resolved from invalid statuses.
- Tickets outside user scope cannot be resolved.
- Closed tickets cannot be resolved again.
- Resolution events are recorded in ticket history.
- Resolution events are recorded in audit history.
- Resolved tickets can proceed to closure.

---

# UC-008: Close Ticket

## Actor

Primary actor:

- Agent
- Supervisor

Supporting actor:

- System

## Goal

Allow an authorized user to close a resolved ticket and prevent further modification unless a reopening workflow is allowed.

## Preconditions

- The user is authenticated.
- The user has permission to close tickets.
- The ticket exists.
- The ticket is within the user's operational scope.
- The ticket has status `Resolved`.
- The ticket is not already closed.

## Main Flow

1. The user opens the resolved ticket.
2. The system displays the current ticket information.
3. The user selects the close action.
4. The system asks the user to confirm closure.
5. The user confirms closure.
6. The system validates the user's role and scope.
7. The system validates that the ticket is resolved.
8. The system updates the ticket status to `Closed`.
9. The system preserves the final SLA outcome.
10. The system prevents further modification unless reopening is allowed.
11. The system records closure in ticket history.
12. The system records closure in audit history.
13. The system displays the closed ticket state.

## Alternative Flows

### AF-001: Ticket Not Resolved

1. The user attempts to close a ticket that is not resolved.
2. The system rejects the closure.
3. The system displays a message indicating that the ticket must be resolved before closure.

### AF-002: Unauthorized Closure

1. The user attempts to close a ticket outside their scope.
2. The system detects unauthorized access.
3. The system prevents closure.
4. The system displays an authorization error.

### AF-003: Already Closed Ticket

1. The user attempts to close an already closed ticket.
2. The system detects that the ticket is already closed.
3. The system prevents duplicate closure.
4. The system displays the current closed state.

### AF-004: Closure Confirmation Cancelled

1. The user selects the close action.
2. The system asks for confirmation.
3. The user cancels the action.
4. The system leaves the ticket unchanged.

## Business Rules

- A ticket must be resolved before it can be closed.
- Closed tickets cannot be modified unless reopening is explicitly allowed.
- Users can only close tickets within their operational scope.
- Closure must preserve final SLA outcome.
- Closure must be recorded in ticket history.
- Closure must be recorded in audit history.

## Postconditions

Successful outcome:

- The ticket status is `Closed`.
- The final SLA outcome is preserved.
- The ticket becomes restricted from further modification.
- Ticket history is updated.
- Audit history is updated.

Failure outcome:

- The ticket remains unchanged.
- The user is informed of the validation or authorization issue.

## Acceptance Criteria

- Authorized users can close resolved tickets.
- Unresolved tickets cannot be closed.
- Closed tickets cannot be modified without an allowed reopening process.
- Closure preserves final SLA outcome.
- Closure events are recorded in ticket history.
- Closure events are recorded in audit history.

---

# UC-009: View SLA Dashboard

## Actor

Primary actor:

- Operations Manager
- Supervisor
- Agent
- Viewer

Supporting actor:

- System

## Goal

Allow authorized users to view SLA-related operational visibility according to their role and assigned scope.

## Preconditions

- The user is authenticated.
- The user has permission to access dashboard views.
- The user has an assigned operational scope.
- Ticket and SLA data exist.
- Dashboard data can be filtered by role and scope.

## Main Flow

1. The user opens the SLA dashboard.
2. The system identifies the user's role.
3. The system identifies the user's operational scope.
4. The system retrieves SLA-related ticket data within the user's scope.
5. The system displays open ticket counts.
6. The system displays overdue ticket counts.
7. The system displays tickets grouped by SLA state.
8. The system displays tickets close to SLA breach.
9. The system allows the user to filter dashboard data by available dimensions.
10. The system ensures all displayed data respects role and scope restrictions.

## Alternative Flows

### AF-001: No SLA Data Available

1. The user opens the SLA dashboard.
2. The system finds no SLA data within the user's scope.
3. The system displays an empty dashboard state.
4. The system does not display unauthorized data.

### AF-002: Unauthorized Dashboard Access

1. The user attempts to access the SLA dashboard without permission.
2. The system denies access.
3. The system displays an authorization error.

### AF-003: Filter Returns No Results

1. The user applies dashboard filters.
2. The system finds no matching records.
3. The system displays an empty filtered result state.

### AF-004: Viewer Access

1. A Viewer opens the SLA dashboard.
2. The system applies read-only access.
3. The system displays dashboard data within assigned scope.
4. The system prevents any modification action.

## Business Rules

- Dashboard data must respect role and operational scope.
- SLA state must be visible to authorized users.
- Tickets that exceed SLA must be marked as breached.
- Tickets approaching SLA breach should be marked as at risk.
- Viewers cannot modify operational data.
- OpsSphere dashboards provide basic operational visibility and do not replace Power BI.

## Postconditions

Successful outcome:

- The user sees SLA dashboard data within allowed scope.
- The user can identify SLA state, overdue tickets, and at-risk tickets.
- No unauthorized data is exposed.

Failure outcome:

- The user cannot access dashboard data.
- The system displays an appropriate authorization or empty-state message.

## Acceptance Criteria

- Authorized users can view the SLA dashboard.
- Dashboard shows open ticket counts.
- Dashboard shows overdue ticket counts.
- Dashboard shows SLA state information.
- Dashboard identifies tickets close to SLA breach.
- Dashboard data respects user role and scope.
- Viewers have read-only access.
- Unauthorized users cannot access dashboard data.

---

# UC-010: Manage Users

## Actor

Primary actor:

- Admin

Supporting actor:

- System

Related actors:

- Operations Manager
- Supervisor
- Agent
- Viewer

## Goal

Allow an Admin to create, update, deactivate, and assign roles and scopes to internal users.

## Preconditions

- The Admin is authenticated.
- The Admin has permission to manage users.
- Required roles exist in the system.
- Required operational structure exists if assigning scope.
- The target user exists for update, role assignment, scope assignment, or deactivation actions.

## Main Flow

1. The Admin opens the user management module.
2. The system displays the user list.
3. The Admin selects a user management action.
4. The Admin enters or updates user information.
5. The Admin assigns the appropriate role.
6. The Admin assigns operational scope if required.
7. The Admin submits the user management action.
8. The system validates required fields.
9. The system validates role and scope consistency.
10. The system creates or updates the user.
11. The system records the administrative change in audit history.
12. The system displays the updated user record.

## Alternative Flows

### AF-001: Create New User

1. The Admin selects the create user action.
2. The system displays the create user form.
3. The Admin enters user details.
4. The Admin assigns a role.
5. The Admin assigns operational scope if required.
6. The Admin submits the form.
7. The system validates the data.
8. The system creates the user.
9. The system records the creation in audit history.

### AF-002: Update Existing User

1. The Admin selects an existing user.
2. The Admin updates user information.
3. The system validates the updated data.
4. The system saves the changes.
5. The system records the update in audit history.

### AF-003: Assign Role

1. The Admin selects a user.
2. The Admin assigns or changes the user's role.
3. The system validates that the role exists.
4. The system saves the role assignment.
5. The system records the role change in audit history.

### AF-004: Assign Operational Scope

1. The Admin selects a user.
2. The Admin assigns operational scope.
3. The system validates that the scope exists.
4. The system validates that the scope is compatible with the user's role.
5. The system saves the scope assignment.
6. The system records the scope assignment in audit history.

### AF-005: Deactivate User

1. The Admin selects an active user.
2. The Admin selects the deactivate action.
3. The system asks for confirmation.
4. The Admin confirms deactivation.
5. The system deactivates the user.
6. The system prevents the user from authenticating.
7. The system records deactivation in audit history.

### AF-006: Missing Required Fields

1. The Admin submits incomplete user information.
2. The system rejects the submission.
3. The system displays validation messages.
4. The Admin completes the required fields.
5. The Admin submits the form again.

### AF-007: Invalid Scope Assignment

1. The Admin assigns a scope that is incompatible with the user's role.
2. The system rejects the assignment.
3. The system displays a scope validation error.

## Business Rules

- Only Admin users can create or modify users.
- Only Admin users can assign roles.
- Only Admin users can assign operational scope.
- Deactivated users cannot authenticate.
- User management changes must be recorded in audit history.
- Users must have valid roles.
- Operational scope must be compatible with the assigned role.
- Viewers must have read-only access.
- Customers do not directly access the initial system.

## Postconditions

Successful outcome:

- The user record is created, updated, assigned, or deactivated.
- Role and scope assignments are saved.
- Audit history is updated.
- Deactivated users are prevented from accessing the system.

Failure outcome:

- The user record remains unchanged.
- The Admin is informed of the validation or authorization issue.

## Acceptance Criteria

- Admin users can create users.
- Admin users can update users.
- Admin users can assign roles.
- Admin users can assign operational scope.
- Admin users can deactivate users.
- Deactivated users cannot access the system.
- Invalid role or scope assignments are rejected.
- User management changes are recorded in audit history.

---

# 5. Use Case Traceability Matrix

| Use Case | Primary Actor | Main Requirement Groups | Main Business Areas |
|---|---|---|---|
| UC-001 Authenticate User | All internal users | FR-AUTH, NFR-SEC | Authentication, Authorization, RBAC |
| UC-002 Create Ticket | Agent, Supervisor | FR-TICKET, FR-CUST, FR-SLA, FR-AUDIT | Ticket Management, Customer Management, SLA |
| UC-003 Assign Ticket | Supervisor | FR-TICKET, FR-ORG, FR-AUDIT | Ticket Assignment, Operational Scope |
| UC-004 Update Ticket Status | Agent, Supervisor | FR-TICKET, FR-WF, FR-AUDIT | Ticket Workflow |
| UC-005 Add Internal Comment | Agent, Supervisor, Manager | FR-COMMENT, FR-AUDIT | Collaboration, Ticket History |
| UC-006 Escalate Ticket | Agent, Supervisor | FR-ESC, FR-TICKET, FR-AUDIT | Escalation Management |
| UC-007 Resolve Ticket | Agent, Supervisor | FR-TICKET, FR-WF, FR-SLA, FR-AUDIT | Ticket Resolution |
| UC-008 Close Ticket | Agent, Supervisor | FR-TICKET, FR-WF, FR-SLA, FR-AUDIT | Ticket Closure |
| UC-009 View SLA Dashboard | Manager, Supervisor, Agent, Viewer | FR-DASH, FR-SLA, FR-REP | Dashboard, SLA Visibility |
| UC-010 Manage Users | Admin | FR-USER, FR-ORG, FR-AUDIT, NFR-SEC | User Management, RBAC, Scope Control |

---

# 6. General Use Case Rules

The following rules apply across multiple use cases:

## 6.1 Authentication Rules

- Users must be authenticated before accessing protected features.
- Expired sessions must prevent access to protected features.
- Deactivated users must not be allowed to authenticate.

## 6.2 Authorization Rules

- Users may only access features permitted by their assigned role.
- Users may only access operational data within their assigned scope.
- Unauthorized access attempts must be rejected.

## 6.3 Audit Rules

- Critical business actions must create audit records.
- Audit records should include actor, timestamp, entity, action, previous value, and new value when applicable.
- Audit history must support operational traceability.

## 6.4 Ticket Workflow Rules

- Tickets must follow valid status transitions.
- Tickets must be resolved before they can be closed.
- Closed tickets cannot be modified unless reopening is explicitly allowed.

## 6.5 SLA Rules

- Tickets must include SLA visibility.
- Breached tickets must be identifiable.
- Tickets close to SLA breach should be identifiable.
- Completed tickets should preserve their final SLA outcome.

## 6.6 Scope Rules

- Managers view data within assigned regions.
- Supervisors manage tickets within assigned accounts or campaigns.
- Agents work only within assigned operational scope.
- Viewers access read-only data within assigned scope.
- Customers do not directly access the initial system.

---

# 7. Out of Scope for Initial Use Cases

The following use cases are outside the initial version:

- Customer portal login.
- Customer self-service ticket creation.
- Real-time chat support.
- Omnichannel message handling.
- Telephony-based ticket creation.
- AI-assisted ticket classification.
- Predictive SLA handling.
- Advanced Power BI dashboard creation.
- Workforce management scheduling.
- Payroll workflows.
- Multi-tenant billing workflows.
- Production-grade enterprise SSO.
- Mobile application workflows.
- Complex workflow automation builder.

---

# 8. Notes for Future Expansion

Future versions of this document may include additional use cases such as:

- Reopen Ticket.
- Create Customer.
- Update Customer.
- Manage Regions.
- Manage Countries.
- Manage Accounts.
- Manage Campaigns.
- Manage Roles and Permissions.
- Export Operational Data.
- Generate Audit Report.
- Configure SLA Rules.
- Configure Notification Rules.
- Integrate External Reporting Tools.
- Customer Portal Ticket Submission.
- Supervisor Workload Balancing.
- Manager Regional Performance Review.
- Power BI Dataset Preparation.

---

# 9. Document Summary

This document defines the initial behavioral foundation of OpsSphere.

The use cases describe how internal users authenticate, create and manage tickets, collaborate through internal comments, escalate operational issues, resolve and close cases, view SLA information, and manage users.

Together with the requirements document, this use case document provides a senior-level bridge between business operations, system behavior, and technical implementation.