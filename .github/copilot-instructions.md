# Copilot Instructions for JobHunter

This repository is an ASP.NET Core Web API project targeting .NET 8.

## Project Context

- Runtime: .NET 8
- Application type: ASP.NET Core Web API
- Primary language: C# with nullable reference types enabled
- API style: RESTful controllers

## Engineering Workflow

When handling feature work, follow this order:

1. Clarify requirements and acceptance criteria.
2. Propose a small implementation plan.
3. Implement minimal, production-safe changes.
4. Add or update tests when behavior changes.
5. Run build and tests before finishing.
6. Summarize risks and follow-up actions.

## Coding Standards

- Prefer clear, maintainable code over clever shortcuts.
- Keep methods focused and small.
- Use dependency injection instead of static/global state where possible.
- Keep public APIs backward compatible unless explicitly asked to change contracts.
- Use async/await for I/O-bound work.
- Propagate CancellationToken for async operations when applicable.
- Respect nullable annotations and avoid null-forgiving operator unless justified.

## API Standards

- Validate inputs and return appropriate HTTP status codes.
- Return ActionResult<T> in controllers when multiple responses are possible.
- Keep DTOs explicit and avoid leaking internal domain models directly.
- Add OpenAPI metadata for new endpoints where useful.

## Security and Reliability

- Never hardcode secrets or tokens.
- Validate and sanitize external input.
- Avoid exposing stack traces or internal details in API responses.
- Use structured logging with contextual fields.

## Verification Commands

Use these commands from repository root before considering work complete:

- dotnet restore
- dotnet build

If no test project exists yet, call that out explicitly in the result.
