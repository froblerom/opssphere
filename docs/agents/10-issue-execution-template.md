# OpsSphere Issue Execution Template

Use this template when assigning an issue to an AI agent or subagent.

---

## Task

```text
[Paste the issue title and description here.]
```

---

## Required Operating Context

Always load:

```text
docs/agents/00-agent-operating-protocol.md
docs/agents/01-project-context.md
```

---

## Additional Context Selection

Select only the files required by the task.

| Task Type | Load |
|---|---|
| Architecture | `docs/agents/02-architecture-context.md` |
| Domain | `docs/agents/03-domain-context.md` |
| Business rules | `docs/agents/04-business-rules-context.md` |
| Testing | `docs/agents/05-testing-and-validation-context.md` |
| Backend | `docs/agents/06-backend-context.md` |
| Frontend | `docs/agents/07-frontend-context.md` |
| DevOps | `docs/agents/08-devops-context.md` |
| Observability | `docs/agents/09-observability-context.md` |

Canonical docs may be loaded only when exact detail is needed.

---

## Prompt

```text
You are the lead implementation agent for OpsSphere.

Work inside the repository using Harness Engineering principles.

The repository is the source of truth. Do not rely on chat memory when repository context exists.

Task:
[PASTE ISSUE HERE]

Required operating context:
- docs/agents/00-agent-operating-protocol.md
- docs/agents/01-project-context.md

Select additional docs/agents context files only if needed.

Rules:
- Keep context loading minimal.
- Do not expand MVP scope.
- Respect Clean Architecture.
- Keep controllers thin.
- Use application commands/queries for workflows.
- Protect domain invariants.
- Enforce backend authorization.
- Consider scope-based access.
- Preserve auditability for critical actions.
- Add or update tests when behavior changes.
- Do not modify unrelated files.
- Do not invent undocumented requirements.

Execution:
1. Restate the task scope.
2. List context files selected and why.
3. Inspect relevant files.
4. Propose a short implementation plan.
5. Implement the smallest complete change.
6. Run validation or state exactly why validation was not run.
7. Produce final summary.

Final output format:
- Scope understood
- Context files used
- Files inspected
- Files changed
- Validation performed
- Acceptance criteria status
- Risks or follow-ups
```

---

## Validation Checklist

Before final response, verify:

```text
[ ] Scope matches issue.
[ ] No unrelated files changed.
[ ] Architecture boundaries respected.
[ ] Business rules preserved.
[ ] Authorization considered.
[ ] Scope filtering considered.
[ ] Audit behavior considered.
[ ] Tests added or updated if behavior changed.
[ ] Build/test validation run or explicitly explained.
[ ] Documentation updated if behavior changed.
```

---

## Final Response Format

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