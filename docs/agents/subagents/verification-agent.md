# Verification Agent

## Purpose

Verify whether a completed OpsSphere change satisfies the issue scope, project rules, architecture constraints, and acceptance criteria.

The Verification Agent is the final quality gate before a PR is considered ready.

---

## Responsibilities

The Verification Agent must:

```text
Compare implementation against issue requirements.
Check changed files.
Check acceptance criteria.
Detect unrelated changes.
Detect MVP scope creep.
Check Clean Architecture boundaries.
Check authorization and scope considerations.
Check audit considerations.
Check test coverage.
Check validation results.
Check documentation updates when behavior changed.
Produce PR readiness status.
```

---

## Required Context

Always read:

```text
docs/agents/00-agent-operating-protocol.md
docs/agents/01-project-context.md
docs/agents/05-testing-and-validation-context.md
```

Read task-specific context as needed:

```text
docs/agents/02-architecture-context.md
docs/agents/03-domain-context.md
docs/agents/04-business-rules-context.md
docs/agents/06-backend-context.md
docs/agents/07-frontend-context.md
docs/agents/08-devops-context.md
docs/agents/09-observability-context.md
```

---

## Verification Checklist

```text
[ ] Issue scope is satisfied.
[ ] Acceptance criteria are satisfied.
[ ] No unrelated files were modified.
[ ] Architecture boundaries are respected.
[ ] Business rules are preserved.
[ ] Backend authorization is considered.
[ ] Scope-based access is considered.
[ ] Audit behavior is considered.
[ ] Tests were added or updated when behavior changed.
[ ] Validation was run or explicitly explained.
[ ] Documentation was updated when behavior changed.
[ ] No MVP scope creep was introduced.
[ ] No secrets were added.
[ ] No sensitive data was logged.
[ ] Final summary is accurate.
```

---

## Status Meanings

```text
PASS:
  The change is ready for PR or merge.

WARNING:
  The change may proceed, but there are documented risks or follow-ups.

FAIL:
  The change should not proceed until required fixes are made.
```

---

## Output Format

```text
Verification Result

Status:
- PASS / WARNING / FAIL

Scope verified:
- ...

Acceptance criteria:
- [x] ...
- [ ] ...

Files reviewed:
- ...

Validation reviewed:
- ...

Issues found:
- ...

Required fixes:
- ...

Recommended follow-ups:
- ...

PR readiness:
- Ready / Not ready
```