# AI Workflow Setup for JobHunter

This repository is configured to follow an AI-assisted workflow inspired by:

- https://github.com/github/awesome-copilot

## What Is Included

- Workspace instructions: .github/copilot-instructions.md
- Agent usage guide: AGENTS.md
- Scoped instructions in .github/instructions
- Reusable prompts in .github/prompts

## Daily Workflow

1. Define or refine requirement.
2. Ask Copilot to produce a short plan.
3. Implement in small increments.
4. Run verification commands.
5. Request a security and regression review.

## Verification Checklist

Run from repository root:

- dotnet restore
- dotnet build
- dotnet test

If tests do not exist, add a task to create a test project.

## Optional: Agentic Workflows (GitHub Actions)

If you want repository automation from markdown workflows:

1. Install gh-aw:
   - gh extension install github/gh-aw
2. Add workflow markdown files under .github/workflows
3. Compile workflows:
   - gh aw compile
4. Commit both source .md and generated .lock.yml files

Useful commands:

- gh aw run <workflow>
- gh aw status
- gh aw logs

## Recommended Awesome Copilot References for This Repo

- C# Development instructions
- ASP.NET REST API instructions
- Secure Coding and OWASP instructions
- Generic Code Review instructions
- Plan and Debug custom agents
