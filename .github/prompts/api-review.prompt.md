---
agent: ask
model: GPT-5
description: "Perform a risk-focused API code review for this repository."
---

Review the current changes with a code-review mindset.

## Priorities

1. Functional bugs and regressions.
2. API contract compatibility risks.
3. Security issues (OWASP-oriented).
4. Missing tests for changed behavior.

## Output Format

- Findings first, ordered by severity.
- Include file and line references.
- List open assumptions/questions.
- Provide concise remediation steps.

If no findings are discovered, explicitly say so and mention residual testing risks.
