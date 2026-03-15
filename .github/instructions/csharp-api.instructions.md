---
description: "C# and ASP.NET Core API coding standards for this repository. Use when editing C# files, controllers, services, DTOs, and API endpoints."
applyTo: "**/*.cs"
---

# C# and API Instructions

## Language and Style

- Use nullable reference types correctly.
- Prefer explicit, descriptive naming.
- Keep methods short and focused.
- Use expression-bodied members only when readability improves.

## Async and Cancellation

- Use async/await for I/O operations.
- Return Task or Task<T> for async APIs.
- Accept and pass CancellationToken in async flows where appropriate.

## API Controller Patterns

- Use ActionResult<T> for endpoints with multiple outcomes.
- Return precise status codes:
  - 200 for successful fetch/update
  - 201 for create
  - 400 for validation issues
  - 404 when resource is missing
  - 409 for conflict scenarios
- Validate input with model binding and annotations.

## DTO and Contract Discipline

- Do not expose internal persistence/domain models directly from controllers.
- Use request/response DTOs for API boundaries.
- Preserve existing route and payload contracts unless explicitly requested.

## Logging

- Use ILogger<T> with structured fields.
- Avoid logging secrets or sensitive personal data.
