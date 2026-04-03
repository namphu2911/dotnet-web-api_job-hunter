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
2. Contract check: inspect `src/config/api.ts` and `src/types/backend.d.ts` before backend edits.
3. Implement: apply smallest safe .NET backend code change.
4. Verify: run build/tests and inspect warnings.
5. Review: check security and API contract stability.

## Prompt Patterns

- "Create an implementation plan for this ASP.NET Core API feature with risks and tests."
- "Implement the approved plan with minimal changes and run build/tests."
- "Review this PR for security, behavior regressions, and missing tests."

## Repository Conventions

- Keep changes focused per task.
- Do not introduce unrelated refactors.
- Preserve existing API routes unless explicitly changing contract.
- Document behavior changes in PR summary.
- Treat `src/` frontend API usage as source of truth for contract compatibility.
- Treat `src/main/**` and `src/test/**` Java code as migration reference, not primary contract.

## Review Scope Exceptions (Current Stage)

- `JobHunter.Api/appsettings.json` and `JobHunter.Api/appsettings.Development.json` are treated as local-only and not intended for git push.
- During review, do not classify content in those two appsettings files as a Critical finding.
- Temporary lack of automated test project/coverage is accepted and should not be graded as a review issue unless explicitly requested.
