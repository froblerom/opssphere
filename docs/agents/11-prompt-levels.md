# OpsSphere Prompt Levels

## Purpose

This document defines prompt levels for AI-assisted work inside OpsSphere.

The goal is to reduce hallucination, avoid unnecessary token usage, and choose the correct amount of context, validation, and agent structure for each task.

Prompt levels do not exist to make prompts longer.

Prompt levels exist to decide:

```text
How much context to load
Which files to inspect
Whether subagents are needed
Which validation is required
How strict the final verification should be
```

---

# Level 0 — Tiny / Mechanical Change

## Use When

Use Level 0 for very small changes such as:

```text
Fix typo
Rename heading
Update one checklist item
Add one link
Fix markdown formatting
Adjust one sentence
```

## Context

Always load:

```text
docs/agents/00-agent-operating-protocol.md
docs/agents/01-project-context.md
```

Also load:

```text
Only the file being modified
```

## Subagents

```text
None
```

## Validation

```text
Review the changed file.
Confirm no unrelated files were modified.
```

## Prompt Instruction

```text
Make the smallest possible change.
Do not inspect unrelated files.
Do not summarize project context.
Do not expand scope.
```

---

# Level 1 — Single-Area Documentation or Localized Change

## Use When

Use Level 1 for isolated single-area documentation or localized changes.

Examples:

```text
Add ADR
Update README
Add docs/agents file
Update API design doc
Update testing strategy
Update risk register
Add PR template
```

Do not use Level 1 for cross-document consistency audits, architecture-sensitive documentation reviews, or harness audits that must compare multiple canonical sources.

## Context

Always load:

```text
docs/agents/00-agent-operating-protocol.md
docs/agents/01-project-context.md
```

Load one relevant area context:

```text
docs/agents/02-architecture-context.md
docs/agents/03-domain-context.md
docs/agents/04-business-rules-context.md
docs/agents/05-testing-and-validation-context.md
docs/agents/08-devops-context.md
docs/agents/09-observability-context.md
```

Load canonical docs only when needed.

## Subagents

Usually:

```text
None
```

Optional:

```text
verification-agent
```

## Validation

```text
Review changed files.
Confirm issue scope.
Confirm no application source code changed unless required.
```

---

# Level 2 — Feature Implementation

## Use When

Use Level 2 for implementation inside one main feature area.

Examples:

```text
CreateTicketCommand
AssignTicketCommand
JWT login endpoint
EF Core entity configuration
Angular login page
Ticket creation form
API endpoint plus tests
```

## Context

Always load:

```text
docs/agents/00-agent-operating-protocol.md
docs/agents/01-project-context.md
docs/agents/02-architecture-context.md
docs/agents/03-domain-context.md
docs/agents/04-business-rules-context.md
docs/agents/05-testing-and-validation-context.md
```

Then load the relevant specialized context:

```text
docs/agents/06-backend-context.md
docs/agents/07-frontend-context.md
docs/agents/08-devops-context.md
docs/agents/09-observability-context.md
```

Load specific source files related to the feature.

## Subagents

Recommended:

```text
backend-implementation-agent
verification-agent
```

Optional:

```text
testing-agent
```

## Validation

Run relevant validation:

```text
dotnet build
dotnet test
npm run build
npm run test
```

Use only commands relevant to the changed area.

---

# Level 3 — Cross-Layer / Architecture-Sensitive Work

## Use When

Use Level 3 for changes that affect multiple layers or critical system rules.

Examples:

```text
Ticket lifecycle end-to-end
Authorization infrastructure
Scope filtering framework
Audit logging foundation
Database schema plus API plus tests
CI pipeline
Observability foundation
Project structure changes
Cross-document documentation audit
Agent harness consistency audit
Canonical documentation alignment
```

## Context

Always load:

```text
docs/agents/00-agent-operating-protocol.md
docs/agents/01-project-context.md
docs/agents/02-architecture-context.md
docs/agents/03-domain-context.md
docs/agents/04-business-rules-context.md
docs/agents/05-testing-and-validation-context.md
```

Load all relevant specialized context:

```text
docs/agents/06-backend-context.md
docs/agents/07-frontend-context.md
docs/agents/08-devops-context.md
docs/agents/09-observability-context.md
```

Load canonical docs only as needed for exact rules.

## Subagents

Recommended sequence:

```text
architecture-auditor
backend-implementation-agent
testing-agent
verification-agent
```

No parallel subagents unless explicitly required.

## Validation

Run all relevant validation:

```text
dotnet restore
dotnet build
dotnet test
npm install
npm run build
npm run test
docker compose up -d
```

Only run Docker-related validation if the task requires infrastructure.

---

# Classification Rules

## Default Classification

If unsure:

```text
Start at the lowest level that can safely complete the task.
```

## Escalate to Higher Level If

Escalate when the task touches:

```text
Multiple layers
Security
Authorization
Scope filtering
Audit logging
Database schema
CI/CD
Deployment
Observability
Architecture structure
Core ticket lifecycle
```

## Do Not Escalate Just Because

Do not increase the level only because:

```text
The prompt looks important.
The issue description is long.
The agent wants more context.
The project has many docs.
```

Context must be justified by the actual files and behavior affected.

---

# Anti-Hallucination Rules

Agents must:

```text
Use repository files as source of truth.
State uncertainty explicitly.
Inspect existing conventions before editing.
Avoid inventing undocumented requirements.
Avoid adding out-of-scope features.
Prefer exact file references over memory.
Run validation or state why it was not run.
```

Agents must not:

```text
Assume implementation details not present in the repo.
Create imaginary files.
Claim tests passed without running them.
Expand MVP scope.
Replace canonical docs with guesses.
```

---

# Token Saving Rules

Agents must:

```text
Load minimal context.
Avoid summarizing context unless needed.
Avoid reading unrelated folders.
Avoid repeating long docs in final output.
Use final summaries focused on changed files and validation.
```

Agents must not:

```text
Paste entire docs into the prompt.
Load all docs for every issue.
Use subagents for simple changes.
Generate huge plans for tiny tasks.
```
