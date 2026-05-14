# OpsSphere Agent Progress - Current State

## Purpose

This file tracks the current state of the OpsSphere repository for AI-assisted work.

It should be updated when major documentation, architecture, implementation, testing, or delivery milestones are completed.

The goal is to give future agents a short operational memory without forcing them to read the entire repository.

---

## Current Project Phase

```text
Planning and architecture documentation phase.
```

OpsSphere has not yet entered full application implementation.

The repository currently emphasizes:

```text
Business documentation
Requirements documentation
Process modeling
Domain modeling
Architecture planning
Security design
Database design
API design
Testing strategy
Deployment planning
Observability planning
Risk management
Architecture Decision Records
Agent harness documentation
```

---

## Current Product Identity

```text
OpsSphere is an enterprise support operations platform for multinational BPO/contact center environments.
```

It is not:

```text
A generic ticketing system
A CRM replacement
A Power BI replacement
A telephony platform
A workforce management system
An AI automation suite
```

---

## Current Canonical Technical Direction

```text
Backend:
  .NET 10
  ASP.NET Core Web API
  Clean Architecture
  CQRS with MediatR
  Entity Framework Core
  SQL Server
  JWT authentication
  Role-based authorization
  Scope-based access control
  Serilog

Frontend:
  Angular
  TypeScript
  RxJS
  Reactive Forms
  Angular Router
  Route guards
  HTTP interceptors
  Angular Material or Tailwind

Testing:
  xUnit
  WebApplicationFactory
  Testcontainers
  Integration tests
  API tests
  Frontend tests
  E2E tests for critical flows

DevOps:
  Docker
  Docker Compose
  GitHub Actions
  Azure-ready deployment
  Azure App Service
  Azure SQL
  Azure Key Vault
  Application Insights
```

---

## Current MVP Scope

The MVP includes:

```text
Authentication
JWT API access
Role-based authorization
Permission-based authorization
Scope-based visibility
User and role management
Operational structure management
Customer management
Ticket lifecycle
Ticket assignment
Ticket status workflow
Ticket priority
Basic SLA tracking
Internal comments
Escalation
Resolution
Closure
Audit history
Basic dashboards
Filtered operational views
Structured data for external reporting
```

---

## Current MVP Non-Goals

The MVP excludes:

```text
Advanced Power BI dashboards
AI ticket classification
Predictive SLA modeling
Telephony integration
Call recording
Omnichannel integrations
Workforce scheduling
Payroll
HR management
External customer portal
Real-time chat
Mobile app
Complex notification engine
Enterprise SSO
Multi-tenant billing
```

---

## Current Documentation Baseline

Canonical docs currently include:

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

---

## Current Agent Harness Baseline

Agent harness docs should live under:

```text
docs/agents/
```

Recommended files:

```text
docs/agents/00-agent-operating-protocol.md
docs/agents/01-project-context.md
docs/agents/02-architecture-context.md
docs/agents/03-domain-context.md
docs/agents/04-business-rules-context.md
docs/agents/05-testing-and-validation-context.md
docs/agents/06-backend-context.md
docs/agents/07-frontend-context.md
docs/agents/08-devops-context.md
docs/agents/09-observability-context.md
docs/agents/10-issue-execution-template.md
docs/agents/11-prompt-levels.md
docs/agents/12-prompt-classifier.md

docs/agents/subagents/architecture-auditor.md
docs/agents/subagents/backend-implementation-agent.md
docs/agents/subagents/testing-agent.md
docs/agents/subagents/verification-agent.md

docs/agents/progress/current-state.md
```

---

## Current Critical Rules for Agents

```text
Repository is source of truth.
Do not rely on chat memory when docs exist.
Load minimal context.
Do not expand MVP scope.
Respect Clean Architecture.
Keep controllers thin.
Backend authorization is source of truth.
Frontend visibility is not security.
Enforce role + permission + scope.
Audit critical actions.
Test behavior changes.
Do not modify unrelated files.
Do not commit secrets.
Use fictional sample data only.
```

---

## Next Recommended Work

Recommended next focus:

```text
Complete or review the AI agent harness audit, then move intentionally into Milestone 1 technical foundation work when scoped.
```

Recommended scope:

```text
Keep docs/agents aligned with canonical docs.
Do not add product scope through agent instructions.
Use Milestone 1 only when the issue explicitly starts technical foundation work.
Do not change application source code from harness maintenance tasks.
```

---

## Last Updated

```text
2026-05-13
```
