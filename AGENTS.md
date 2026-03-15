# AGENTS Guide for JobHunter

This file documents recommended GitHub Copilot agent usage for this repository.

## Recommended Agent Stack

- @jobhunter-orchestrator: coordinate multi-step workflows.
- @jobhunter-planner: break down features and migration tasks.
- @jobhunter-implementer: implement API changes in C#/.NET.
- @jobhunter-reviewer: review regressions and test gaps.
- @jobhunter-security-reviewer: perform OWASP-oriented review before merge.

Agent definitions live in `.github/agents/` at repository root.

## Suggested Task Flow

1. Plan: define acceptance criteria and edge cases.
2. Implement: apply smallest safe code change.
3. Verify: run build/tests and inspect warnings.
4. Review: check security and API contract stability.

## Prompt Patterns

- "Create an implementation plan for this ASP.NET Core API feature with risks and tests."
- "Implement the approved plan with minimal changes and run build/tests."
- "Review this PR for security, behavior regressions, and missing tests."

## Repository Conventions

- Keep changes focused per task.
- Do not introduce unrelated refactors.
- Preserve existing API routes unless explicitly changing contract.
- Document behavior changes in PR summary.
