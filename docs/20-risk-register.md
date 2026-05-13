# Risk Register

## Document Information

| Field | Value |
|---|---|
| Project | OpsSphere |
| Document | Risk Register |
| File | `docs/20-risk-register.md` |
| Version | 1.0 |
| Status | Draft |
| Project Type | Enterprise Support Operations Platform |
| Related Issue | #6 |
| Phase | Delivery Planning |

---

## 1. Purpose

This document defines the initial risk register for OpsSphere.

The purpose of the risk register is to identify project, business, technical, security, delivery, and operational risks before the implementation phase advances too far.

OpsSphere is planned as a maintainable, testable, deployable, and supportable enterprise application. For that reason, risks must be tracked explicitly and reviewed as the project evolves.

This document supports:

- Project planning.
- Scope control.
- Delivery management.
- Architecture decisions.
- Security planning.
- Testing strategy.
- Deployment readiness.
- Support readiness.

---

## 2. Risk Management Goals

The main goals of risk management for OpsSphere are:

1. Identify risks early.
2. Reduce uncertainty before implementation.
3. Prevent scope from growing beyond the MVP.
4. Protect security and access-control boundaries.
5. Keep the architecture maintainable.
6. Ensure testing and deployment are planned.
7. Preserve auditability and operational traceability.
8. Make risks visible instead of implicit.

---

## 3. Risk Register Format

Each risk is tracked using the following fields:

| Field | Description |
|---|---|
| Risk ID | Unique identifier for the risk |
| Risk | Description of the risk |
| Impact | Expected consequence if the risk happens |
| Probability | Estimated likelihood: Low, Medium, or High |
| Mitigation | Planned action to reduce the risk |
| Owner | Role responsible for monitoring or reducing the risk |
| Status | Current state: Open, Monitoring, Mitigated, or Closed |

---

## 4. Risk Probability Scale

| Probability | Meaning |
|---|---|
| Low | The risk is unlikely but still possible |
| Medium | The risk is realistic and should be monitored |
| High | The risk is likely unless actively controlled |

---

## 5. Risk Impact Scale

| Impact | Meaning |
|---|---|
| Low | Minor inconvenience or small rework |
| Medium | Noticeable delay, rework, or quality issue |
| High | Major delivery, security, architecture, or business impact |

---

# 6. Risk Register

| Risk ID | Risk | Impact | Probability | Mitigation | Owner | Status |
|---|---|---|---|---|---|---|
| R-001 | Scope grows too large before the MVP is complete. | High | High | Define a clear MVP, keep Phase 2 and Phase 3 items separate, and reject advanced features until the core workflows are stable. | Technical Owner | Open |
| R-002 | OpsSphere becomes a generic ticketing system instead of an enterprise operations platform. | High | Medium | Keep documentation and implementation aligned with regions, countries, accounts, campaigns, supervisors, agents, SLAs, audit history, and operational dashboards. | Technical Owner | Monitoring |
| R-003 | Permission model becomes too complex too early. | High | Medium | Start with a simple role-based access matrix and add scope-based rules gradually. Validate access rules through tests. | Technical Owner | Open |
| R-004 | Scope-based visibility is implemented inconsistently. | High | Medium | Enforce scope filtering in backend queries, not only in the frontend. Add integration and API tests for cross-scope access. | Technical Owner | Open |
| R-005 | Frontend hides unauthorized actions but backend does not enforce authorization. | High | Medium | Treat backend authorization as the source of truth. Test protected endpoints directly. | Technical Owner | Open |
| R-006 | Audit logging is forgotten or added too late. | High | Medium | Include audit logging requirements in ticket, user, role, permission, assignment, escalation, resolution, and closure workflows. | Technical Owner | Open |
| R-007 | Ticket workflow rules become inconsistent. | High | Medium | Define allowed status transitions clearly and cover them with unit and API tests. | Technical Owner | Open |
| R-008 | SLA logic becomes too advanced for the MVP. | Medium | Medium | Keep MVP SLA tracking simple: target due date, SLA state, at-risk, breached, and completed. Defer business-hour calendars and advanced SLA rules. | Technical Owner | Monitoring |
| R-009 | Domain model becomes overengineered. | Medium | Medium | Model only concepts needed for documented use cases and business rules. Avoid premature abstractions. | Technical Owner | Monitoring |
| R-010 | Business rules are implemented directly in controllers or UI. | High | Medium | Use Clean Architecture and keep business rules in domain/application layers. Keep controllers thin. | Technical Owner | Open |
| R-011 | Database design does not preserve historical traceability. | High | Medium | Use audit logs, history tables where needed, and deactivation instead of physical deletion for records referenced by tickets. | Technical Owner | Open |
| R-012 | User, role, or operational structure records are deleted and break historical tickets. | High | Medium | Use soft delete or deactivation for historical records. Preserve original ticket context. | Technical Owner | Open |
| R-013 | Test coverage focuses only on happy paths. | Medium | High | Include negative tests for invalid transitions, unauthorized access, missing fields, closed ticket restrictions, and scope violations. | Technical Owner | Open |
| R-014 | Integration tests use unrealistic persistence behavior. | Medium | Medium | Use SQL Server Testcontainers instead of relying only on in-memory database testing. | Technical Owner | Monitoring |
| R-015 | E2E tests become too slow or fragile. | Medium | Medium | Keep E2E tests limited to critical journeys and cover most rules through unit, integration, and API tests. | Technical Owner | Monitoring |
| R-016 | Local development setup becomes difficult to reproduce. | Medium | Medium | Use Docker Compose, `.env.example`, clear setup instructions, seed data, and documented commands. | Technical Owner | Open |
| R-017 | Secrets are accidentally committed to source control. | High | Medium | Use `.gitignore`, `.env.example`, .NET User Secrets, GitHub Secrets, and Azure Key Vault for hosted environments. | Technical Owner | Open |
| R-018 | CI pipeline is added too late. | Medium | Medium | Add GitHub Actions early for restore, build, and tests. Require passing checks before merge when branch protection is enabled. | Technical Owner | Open |
| R-019 | Database migrations break staging or production. | High | Medium | Review migrations, test them locally and in staging, avoid destructive changes, and use backups before production migration. | Technical Owner | Monitoring |
| R-020 | Azure deployment assumptions are not validated. | Medium | Medium | Keep deployment Azure-ready, then validate incrementally with Azure App Service, Azure SQL, Key Vault, and Application Insights. | Technical Owner | Open |
| R-021 | Application lacks useful logs during failures. | Medium | Medium | Add structured logging with Serilog, correlation IDs, request logging, and safe exception logging. | Technical Owner | Open |
| R-022 | Error responses expose internal technical details. | High | Medium | Use global exception handling and safe API error response models. Do not expose stack traces to users. | Technical Owner | Open |
| R-023 | Health checks are missing or too shallow. | Medium | Medium | Add `/health` endpoint with API, database, and configuration checks. Expand later for background jobs and external services. | Technical Owner | Open |
| R-024 | Operational dashboard data becomes inaccurate or misleading. | Medium | Medium | Define dashboard metrics clearly, filter by role and scope, and validate queries through integration tests. | Technical Owner | Open |
| R-025 | Reporting scope expands into full BI platform. | Medium | Medium | Keep Power BI and advanced analytics outside MVP. Focus on structured data and simple operational dashboards. | Technical Owner | Monitoring |
| R-026 | Support process is undefined after deployment. | Medium | Medium | Define troubleshooting flows, severity levels, incident review template, and support information to collect. | Technical Owner | Open |
| R-027 | Application performance degrades due to dashboard queries. | Medium | Medium | Add indexes for common filters, paginate lists, measure request duration, and optimize dashboard queries. | Technical Owner | Monitoring |
| R-028 | Background SLA processing becomes unreliable. | Medium | Low | Start with simple request-based SLA evaluation or controlled background checks. Log worker results and failures. | Technical Owner | Monitoring |
| R-029 | Frontend and backend API contracts drift apart. | Medium | Medium | Use typed DTOs, OpenAPI/Swagger documentation, and API tests. Validate frontend against stable endpoints. | Technical Owner | Open |
| R-030 | Sample data accidentally resembles real confidential company data. | High | Low | Use fictional names only, such as NovaBank, Streamly, Shopora, and fictional users/customers. | Technical Owner | Monitoring |
| R-031 | Authentication implementation is insecure or incomplete. | High | Medium | Use secure password hashing, JWT expiration, protected endpoints, and tests for missing or expired tokens. | Technical Owner | Open |
| R-032 | Viewer role accidentally gains write permissions. | High | Medium | Treat Viewer as read-only. Add API and E2E tests confirming that Viewer cannot create, update, assign, escalate, resolve, close, or delete records. | Technical Owner | Open |
| R-033 | Agent can access tickets outside assigned scope. | High | Medium | Validate role and scope on all ticket queries and commands. Add cross-scope denial tests. | Technical Owner | Open |
| R-034 | Supervisor can assign tickets to ineligible agents. | High | Medium | Validate agent eligibility by account, campaign, active status, and supervisor scope before assignment. | Technical Owner | Open |
| R-035 | Closed tickets are modified without proper reopening workflow. | Medium | Medium | Enforce closed-ticket restrictions in domain/application logic and cover with unit and API tests. | Technical Owner | Open |
| R-036 | Escalations lack enough context for supervisors. | Medium | Medium | Require escalation reason and preserve escalation history, ticket history, and audit records. | Technical Owner | Open |
| R-037 | Internal comments are exposed outside authorized users. | High | Low | Treat comments as internal-only in the MVP. Enforce authorization and scope filtering on comment endpoints. | Technical Owner | Open |
| R-038 | Project documentation becomes outdated as implementation evolves. | Medium | Medium | Update docs when architecture, requirements, workflows, or delivery decisions change. Link PRs to issues. | Technical Owner | Monitoring |
| R-039 | Pull requests become too large and difficult to review. | Medium | Medium | Use issue-based branches and keep PRs focused on one document, feature, or workflow at a time. | Technical Owner | Monitoring |
| R-040 | Overemphasis on documentation delays implementation. | Medium | Medium | Keep documentation practical and implementation-oriented. Move forward once baseline documents are sufficient. | Technical Owner | Monitoring |

---

# 7. High-Priority Risks

The following risks require the most attention during early implementation:

| Risk ID | Risk | Reason |
|---|---|---|
| R-001 | Scope grows too large before MVP is complete | Can delay or derail the entire project |
| R-003 | Permission model becomes too complex too early | Can make implementation and testing difficult |
| R-004 | Scope-based visibility is implemented inconsistently | Can create data exposure issues |
| R-005 | Backend authorization is not enforced | Critical security risk |
| R-006 | Audit logging is forgotten | Reduces enterprise credibility and traceability |
| R-010 | Business rules are implemented in controllers or UI | Weakens architecture and testability |
| R-017 | Secrets are committed to source control | Security risk |
| R-031 | Authentication is insecure or incomplete | Critical access-control risk |

---

# 8. Risk Mitigation Themes

## 8.1 Scope Control

Scope risks should be managed by keeping the MVP focused on:

```text
Authentication
Role-based access
Scope-based visibility
Ticket lifecycle
SLA tracking
Internal comments
Escalations
Audit history
Basic dashboards
```

Advanced features should remain outside the MVP unless explicitly promoted from the roadmap.

Deferred features include:

```text
AI ticket classification
Predictive SLA risk
Telephony integration
Power BI dashboard authoring
Workforce management
Customer portal
Mobile app
Enterprise SSO
Complex notification engine
```

---

## 8.2 Security and Access Control

Security risks should be managed by enforcing:

```text
Authentication on protected endpoints
Role-based authorization
Scope-based filtering
Backend authorization as source of truth
Least privilege
Safe error handling
No secrets in source control
```

Frontend visibility improves user experience, but it must never replace backend enforcement.

---

## 8.3 Auditability

Auditability risks should be managed by recording critical changes such as:

```text
Ticket created
Ticket assigned
Ticket reassigned
Status changed
Priority changed
Internal comment added
Ticket escalated
Ticket resolved
Ticket closed
User created
User deactivated
Role changed
Scope changed
Permission changed
```

Audit logs should preserve:

```text
Who performed the action
When it happened
What changed
Previous value when applicable
New value when applicable
Affected entity
Correlation ID when available
```

---

## 8.4 Testing

Testing risks should be managed by covering:

```text
Happy paths
Validation failures
Authorization failures
Scope violations
Invalid workflow transitions
Closed ticket restrictions
Audit side effects
SLA state behavior
Dashboard query filtering
```

The most important workflows should have unit, integration, API, E2E, and UAT coverage where appropriate.

---

## 8.5 DevOps and Deployment

Deployment risks should be managed by using:

```text
Docker Compose for local development
GitHub Actions for CI
Environment-specific configuration
GitHub Secrets
Azure Key Vault
Azure App Service readiness
Azure SQL readiness
Application Insights readiness
Health checks
Smoke tests
Rollback planning
```

---

# 9. Risk Review Process

## 9.1 Review Frequency

The risk register should be reviewed:

```text
At the start of each new project phase
Before starting implementation of major workflows
Before deployment to staging
Before production release
When a major architecture decision changes
When a risk becomes an active issue
```

## 9.2 Risk Status Values

| Status | Meaning |
|---|---|
| Open | Risk is active and requires mitigation |
| Monitoring | Risk is known and being watched |
| Mitigated | Risk has a mitigation in place |
| Closed | Risk is no longer relevant or has been resolved |

## 9.3 Updating the Risk Register

When updating a risk, record:

```text
What changed
Why the risk changed
Whether probability changed
Whether impact changed
What mitigation was added
Whether status changed
```

---

# 10. Risk Ownership

For the initial project phase, the main owner is:

```text
Technical Owner
```

As the project evolves, ownership may be split across:

| Area | Possible Owner |
|---|---|
| Business scope | Product Owner |
| Technical architecture | Technical Owner |
| Security | Technical Owner |
| Testing | QA Owner or Technical Owner |
| DevOps | DevOps Owner or Technical Owner |
| Support | Support Owner |
| Documentation | Technical Owner |

For the portfolio version of OpsSphere, the Technical Owner may temporarily cover all risk areas.

---

# 11. Risk Response Types

OpsSphere may use the following risk response types:

| Response Type | Meaning |
|---|---|
| Avoid | Change the plan to remove the risk |
| Mitigate | Reduce the probability or impact |
| Transfer | Move responsibility to another tool, service, or process |
| Accept | Acknowledge the risk and proceed |
| Monitor | Track the risk without immediate action |

Examples:

```text
Avoid:
  Do not include AI classification in MVP.

Mitigate:
  Add tests for scope-based authorization.

Transfer:
  Use Azure Key Vault instead of custom secret storage.

Accept:
  Do not implement enterprise SSO in the initial version.

Monitor:
  Watch dashboard query performance as data grows.
```

---

# 12. Initial Risk Summary

The initial risk profile of OpsSphere is manageable if the project remains disciplined.

The highest risk areas are:

```text
Scope control
Permission complexity
Scope-based data visibility
Backend authorization
Audit logging
Testing coverage
Secrets management
Deployment readiness
```

The strongest mitigations are:

```text
Clear MVP boundaries
Phased roadmap
Simple role-based access matrix
Backend authorization enforcement
Automated tests
Audit log requirements
Docker-based local setup
GitHub Actions CI
Azure-ready deployment plan
Structured logging and health checks
```

---

# 13. Summary

The OpsSphere risk register makes project risks explicit and manageable.

The main purpose is not to eliminate all risk. The purpose is to prevent avoidable problems by identifying them early and connecting each risk to a clear mitigation strategy.

This document demonstrates that OpsSphere is planned not only as a software implementation, but as an enterprise-grade delivery effort with attention to scope, security, testing, deployment, observability, support, and long-term maintainability.