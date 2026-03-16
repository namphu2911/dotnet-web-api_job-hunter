# JobHunter Custom Agents

This folder contains repository-level custom agents for JobHunter.

## Available Agents

- jobhunter-orchestrator: Coordinates multi-step workflows and suggests handoffs.
- jobhunter-planner: Produces implementation plans and validation strategy.
- jobhunter-implementer: Applies approved code changes and verifies build/tests.
- jobhunter-reviewer: Performs risk-first code review.
- jobhunter-security-reviewer: Performs OWASP-focused security review.

## Suggested Usage

1. Start with @jobhunter-orchestrator for non-trivial tasks.
2. Use handoff to @jobhunter-planner.
3. After approval, handoff to @jobhunter-implementer.
4. Finish with @jobhunter-reviewer and @jobhunter-security-reviewer.

## Markdown Artifact Conventions

- Non-code agents now write markdown artifacts for downstream use:
  - Planner: `JobHunter.Api/docs/ai-output/plans/YYYY-MM-DD-<slug>-plan.md`
  - Reviewer: `JobHunter.Api/docs/ai-output/reviews/YYYY-MM-DD-<slug>-review.md`
  - Security Reviewer: `JobHunter.Api/docs/ai-output/security/YYYY-MM-DD-<slug>-security-review.md`
  - Orchestrator summary: `JobHunter.Api/docs/ai-output/workflows/YYYY-MM-DD-<slug>-workflow.md`
- `JobHunter Implementer` remains focused on code generation and verification.

## Notes

- Agent filenames use lower-case with hyphens.
- Files are in .github/agents for repository-scoped discovery.
