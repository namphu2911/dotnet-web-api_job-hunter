---
agent: ask
model: GPT-5
description: "Create a safe implementation plan and code changes for an ASP.NET Core API feature in this repository."
---

You are implementing a feature for this ASP.NET Core Web API repository.

## Inputs

- Feature request: ${input:feature_request}
- Constraints: ${input:constraints}

## Required Process

1. Summarize acceptance criteria.
2. Propose a minimal implementation plan.
3. Implement only necessary changes.
4. Add/update tests for behavior changes.
5. Run verification commands:
   - dotnet restore
   - dotnet build
   - dotnet test
6. Report:
   - files changed
   - risks
   - follow-up tasks

## Guardrails

- Preserve API contracts unless explicitly requested to change them.
- Do not hardcode secrets.
- Follow nullable and async best practices.
