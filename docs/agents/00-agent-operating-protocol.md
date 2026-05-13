# OpsSphere Agent Operating Protocol

## Purpose

This document defines how AI agents must work inside the OpsSphere repository.

OpsSphere uses the repository as the source of truth. The chat is not the source of truth when repository context exists.

Agents must work from documented project context, explicit issue scope, implementation files, and validation results.

The goal of this protocol is to make AI-assisted development:

- Repeatable
- Auditable
- Token-efficient
- Scope-controlled
- Architecture-safe
- Validation-driven

---

## Core Principle

```text
Do not treat the AI chat as the system memory.
Treat the repository as the system memory.
```

Agents must use the repository to understand:

- Project scope
- Business rules
- Domain model
- Architecture constraints
- Security model
- Testing expectations
- DevOps expectations
- Current implementation state

---

## Mandatory Operating Rules

1. Do not guess project scope.
2. Do not expand MVP scope unless the issue explicitly requires it.
3. Keep OpsSphere focused on enterprise support operations for multinational BPO/contact center environments.
4. Do not turn OpsSphere into a generic ticketing system.
5. Respect Clean Architecture boundaries.
6. Keep business logic out of controllers.
7. Enforce business rules in the Domain and Application layers.
8. Enforce backend authorization independently from frontend visibility.
9. Apply role-based, permission-based, and scope-based access control.
10. Preserve auditability for critical business actions.
11. Add or update tests when behavior changes.
12. Do not introduce external integrations unless explicitly requested.
13. Do not modify unrelated files.
14. Do not invent requirements.
15. Do not use production or real customer data.
16. Prefer small, verifiable changes over broad rewrites.
17. Validate work before declaring completion.

---

## Context Loading Strategy

Agents must load only the minimum context required for the task.

Always load:

```text
docs/agents/00-agent-operating-protocol.md
docs/agents/01-project-context.md
```

Load additional context based on the task:

| Task Type | Context File |
|---|---|
| Architecture | `docs/agents/02-architecture-context.md` |
| Domain modeling | `docs/agents/03-domain-context.md` |
| Business rules | `docs/agents/04-business-rules-context.md` |
| Testing | `docs/agents/05-testing-and-validation-context.md` |
| Backend implementation | `docs/agents/06-backend-context.md` |
| Frontend implementation | `docs/agents/07-frontend-context.md` |
| DevOps | `docs/agents/08-devops-context.md` |
| Observability/support | `docs/agents/09-observability-context.md` |

Use original canonical docs only when needed for deeper detail.

---

## Canonical Documentation Sources

The following files are canonical sources for OpsSphere context:

```text
docs/00-executive-summary.md
docs/01-business-context.md
docs/02-business-case.md
docs/03-project-charter.md
docs/04-stakeholders.md
docs/05-scope-and-roadmap.md
docs/06-requirements.md
docs/07-use-cases.md
docs/08-business-process-flows.md
docs/09-business-rules.md
docs/10-domain-model.md
docs/11-architecture-overview.md
docs/12-c4-architecture.md
docs/13-uml-diagrams.md
docs/14-database-design.md
docs/15-api-design.md
docs/16-security-and-permissions.md
docs/17-testing-strategy.md
docs/18-deployment-and-devops.md
docs/19-observability-and-support.md
docs/20-risk-register.md
docs/decisions/
```

Do not duplicate all canonical documentation into prompts.

Use `docs/agents/` as compressed operational context.

Use canonical docs for confirmation when a task requires exact detail.

---

## Execution Protocol

For each task, the lead agent must follow this sequence:

```text
1. Read the issue or task.
2. Restate the task scope.
3. Identify the minimum context files needed.
4. Inspect relevant repository files.
5. Identify likely files to change.
6. Make the smallest complete change.
7. Add or update validation.
8. Run validation if possible.
9. Report validation result.
10. Summarize changed files.
11. Report risks, assumptions, and follow-ups.
```

---

## Subagent Protocol

Use subagents only when they provide value.

Do not create unnecessary multiagent overhead.

Recommended subagent sequence:

```text
architecture-auditor
  → backend-implementation-agent
  → testing-agent
  → verification-agent
```

Use fewer agents for simple documentation or isolated changes.

Do not spawn parallel subagents unless the task explicitly requires it.

---

## Token Efficiency Rules

Agents must avoid wasting tokens.

Do:

```text
- Load only relevant context.
- Read only files related to the issue.
- Summarize findings briefly.
- Avoid repeating full documentation.
- Produce focused outputs.
- Keep final summaries short and useful.
```

Do not:

```text
- Paste entire canonical docs into the prompt.
- Summarize every loaded document.
- Read unrelated folders.
- Generate broad implementation plans for small issues.
- Rewrite files that only need small edits.
```

---

## Architecture Rules

OpsSphere uses Clean Architecture.

The dependency direction is:

```text
Presentation
  → Application
    → Domain
      ← Infrastructure
```

Rules:

- Domain must not depend on API, Angular, EF Core, SQL Server, JWT, Serilog, Azure, or external services.
- Application coordinates use cases through commands, queries, handlers, validation, authorization coordination, transactions, and audit coordination.
- Infrastructure implements persistence, JWT, password hashing, logging, external services, and database access.
- API controllers must stay thin.
- Business workflows must not live directly in controllers.
- Frontend visibility is not security.

---

## Security Rules

OpsSphere authorization depends on:

```text
Role
+ Permission
+ Operational Scope
```

A request should be allowed only when all required checks pass:

```text
1. User is authenticated.
2. User is active.
3. User has the required role.
4. User has the required permission.
5. User has scope over the target resource.
6. Resource state allows the action.
7. Audit-sensitive action is recorded.
```

The backend is the source of truth.

Frontend route guards and hidden buttons improve UX but are not security controls by themselves.

---

## Auditability Rules

Critical business actions must produce audit records when implemented.

Examples:

```text
TicketCreated
TicketAssigned
TicketReassigned
TicketStatusChanged
TicketPriorityChanged
TicketEscalated
TicketResolved
TicketClosed
InternalCommentAdded
UserCreated
UserUpdated
UserDeactivated
RoleAssigned
RoleRemoved
ScopeAssigned
ScopeUpdated
RegionCreated
CountryCreated
AccountCreated
CampaignCreated
PermissionChanged
```

---

## Testing Rules

When behavior changes, agents must add or update tests.

Prioritize tests for:

```text
Authentication
Authorization
Scope filtering
Ticket creation
Ticket assignment
Ticket status workflow
Escalation
Resolution
Closure
Audit logging
SLA state
API validation
Database persistence
```

Preferred validation commands may include:

```text
dotnet restore
dotnet build
dotnet test

npm install
npm run build
npm run test

docker compose up -d
```

Actual commands must be adjusted to the implemented repository structure.

---

## DevOps Rules

Agents must preserve delivery readiness.

Important principles:

```text
- Keep secrets out of source control.
- Use Docker Compose for repeatable local infrastructure.
- Use GitHub Actions for CI validation.
- Keep staging and production configuration separate.
- Validate build and tests before merge.
- Use health checks and smoke tests for deployment validation.
```

---

## Observability Rules

Agents must preserve operational supportability.

Important principles:

```text
- Use structured logging.
- Use correlation IDs.
- Do not log sensitive data.
- Keep business audit logs separate from technical logs.
- Add health checks for API, database, and required configuration.
- Return safe error responses.
- Do not expose stack traces to users.
```

---

## Output Format for Agents

Every agent response should include:

```text
Scope understood:
- ...

Context files used:
- ...

Files inspected:
- ...

Files changed:
- ...

Validation performed:
- ...

Acceptance criteria status:
- ...

Risks or follow-ups:
- ...
```

---

## Prohibited Behavior

Agents must not:

```text
- Invent undocumented business rules.
- Bypass authorization.
- Implement security only in the frontend.
- Put business logic directly in controllers.
- Add AI classification to MVP unless explicitly requested.
- Add telephony, payroll, workforce management, HR, real-time chat, customer portal, or omnichannel features to MVP.
- Replace Power BI or build a full BI platform.
- Use real company, employee, or customer data.
- Store secrets in source control.
- Modify unrelated files.
- Ignore failing tests.
- Claim validation passed without running or explaining it.
```

---

## Definition of Done

A task is done when:

```text
- The requested scope is satisfied.
- Architecture boundaries are respected.
- Business rules are preserved.
- Authorization and scope rules are considered.
- Audit behavior is considered.
- Tests are added or updated when behavior changes.
- Validation is run or explicitly explained.
- No unrelated files are modified.
- The final response clearly explains what changed.
```
