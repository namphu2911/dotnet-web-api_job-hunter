# Copilot Instructions for JobHunter

This repository is an ASP.NET Core Web API project targeting .NET 8.

## Project Context

- Runtime: .NET 8
- Application type: ASP.NET Core Web API
- Primary language: C# with nullable reference types enabled
- API style: RESTful controllers

## Repository Context (Important)

- `src/` is active React frontend code and is the primary API contract consumer.
- `src/config/api.ts` defines frontend API calls and endpoint expectations.
- `src/types/backend.d.ts` defines response and payload shape expectations used by frontend.
- `src/main/` and `src/test/` contain legacy Java code used as migration reference only.
- Active backend implementation lives in `JobHunter.Api`, `JobHunter.Application`, `JobHunter.Domain`, and `JobHunter.Infrastructure`.

When Java behavior and frontend behavior differ, prioritize frontend compatibility unless explicitly requested otherwise.

## Engineering Workflow

When handling feature work, follow this order:

1. Clarify requirements and acceptance criteria.
2. Verify expected API contract from `src/config/api.ts` and `src/types/backend.d.ts`.
3. Propose a small implementation plan.
4. Implement minimal, production-safe changes in .NET backend projects.
5. Add or update tests when behavior changes.
6. Run build and tests before finishing.
7. Summarize risks and follow-up actions.

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
- Preserve frontend-facing API compatibility by default:
  - Keep routes under `/api/v1/*` unless explicitly requested.
  - Keep response envelope fields compatible with frontend expectations (`statusCode`, `message`, `error`, `data`).
  - Preserve auth flow expectations (`/auth/login`, `/auth/refresh`, `/auth/account`, `/auth/logout`, refresh cookie behavior).

## Migration Guidance

- Use `src/main/java/**` and `src/test/java/**` to understand legacy behavior during migration.
- Do not treat Java source as the final contract when frontend calls indicate different behavior.
- Avoid editing frontend files when implementing backend migration unless user explicitly asks for FE changes.

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

## Accepted Review Exceptions (Current Project Stage)

- `JobHunter.Api/appsettings.json` and `JobHunter.Api/appsettings.Development.json` are intentionally local-only and are not intended to be pushed to git.
- While this policy is active, do not classify values in those two local appsettings files as Critical findings in review reports.
- Missing automated test project/coverage is currently an accepted constraint; do not score or grade this as a review finding unless explicitly requested.
