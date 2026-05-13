# OpsSphere Prompt Classifier

## Purpose

Classify incoming work into the correct prompt level before assigning it to an agent.

---

## Classification Output Format

```text
Classification:
Level 0 / Level 1 / Level 2 / Level 3

Reason:
...

Required context:
- ...

Recommended agents:
- ...

Validation:
- ...
```

---

## Level Selection Questions

Ask:

```text
Does this only modify text or formatting?
Does it touch one file or many?
Does it change application behavior?
Does it affect backend, frontend, database, or tests?
Does it affect authorization, scope, audit, or security?
Does it cross multiple layers?
Does documentation need to be checked against multiple canonical sources?
Does it audit or change the agent harness itself?
Does it require validation commands?
```

---

## Fast Mapping

| Task | Level |
|---|---|
| Typo or formatting | Level 0 |
| Typo or formatting in docs | Level 0 |
| Single-file Markdown documentation | Level 1 |
| ADR | Level 1 |
| PR template | Level 1 |
| Single backend command | Level 2 |
| Single frontend form | Level 2 |
| API endpoint with tests | Level 2 |
| EF Core entity and migration | Level 2 or 3 |
| Auth system | Level 3 |
| Scope filtering framework | Level 3 |
| Audit logging foundation | Level 3 |
| Ticket lifecycle end-to-end | Level 3 |
| CI/CD pipeline | Level 3 |
| Observability foundation | Level 3 |
| Cross-document documentation audit | Level 3 |
| Agent harness audit | Level 3 |
| Canonical documentation alignment | Level 3 |
