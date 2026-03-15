---
description: 'Guidelines for creating and maintaining custom agent files in this repository.'
applyTo: '**/*.agent.md'
---

# Custom Agent File Guidelines

## Frontmatter Requirements
- Include a non-empty description field.
- Prefer including a human-readable name.
- Set model explicitly for predictable behavior.
- Use handoffs only when a guided workflow is truly needed.

## Naming and Placement
- Store repository agents under .github/agents/.
- Use lowercase kebab-case filenames ending with .agent.md.
- Keep one clear responsibility per agent.

## Authoring Rules
- Be specific about scope and boundaries.
- State what the agent should and should not do.
- Keep instructions concise and action-oriented.
- Avoid contradictory guidance across agents.

## Quality Checklist
- Agent purpose is unique and non-duplicative.
- Instructions align with .github/copilot-instructions.md.
- Tool/model assumptions are realistic for the environment.
- At least one representative task has been tested.
