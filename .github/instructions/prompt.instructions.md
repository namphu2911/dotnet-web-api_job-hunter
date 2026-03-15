---
description: 'Guidelines for authoring high-quality prompt files for repository workflows.'
applyTo: '**/*.prompt.md'
---

# Prompt File Guidelines

## Frontmatter
- Include description with a clear actionable intent.
- Prefer specifying agent or mode when workflow depends on it.
- Include model only when deterministic model behavior is required.

## Structure
- Start with a clear heading that matches the prompt intent.
- Define inputs and expected output format.
- Describe workflow steps in deterministic order.
- Add validation steps (build/test/review) when relevant.

## Quality Rules
- Avoid vague directives and ambiguous acceptance criteria.
- Keep prompts task-focused and repository-aware.
- Reference related instruction files using relative paths.
- Keep output expectations concise and testable.
