---
agent: agent
model: GPT-5
description: 'Run a multi-agent workflow for planning, implementation, review, and security checks.'
---

# Multi-Agent Workflow

Use @jobhunter-orchestrator to coordinate this task.

## Inputs
- Task request: ${input:task_request}
- Constraints: ${input:constraints}

## Workflow
1. Plan with @jobhunter-planner.
2. Implement with @jobhunter-implementer.
3. Review with @jobhunter-reviewer.
4. Security review with @jobhunter-security-reviewer.
5. Summarize findings, risks, and final validation status.
