---
name: JobHunter Guidelines
description: Core context, coding standards, and project structure for the JobHunter repository.
---

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

## Coding Standards
- Prefer clear, maintainable code over clever shortcuts.
- Keep methods focused and small.
- Use dependency injection instead of static/global state where possible.
- Keep public APIs backward compatible unless explicitly asked to change contracts.
- Use async/await for I/O-bound work, propagate `CancellationToken`.
- Respect nullable annotations.

## API Standards
- Validate inputs, return appropriate HTTP status codes, use `ActionResult<T>`.
- Keep routes under `/api/v1/*` unless explicitly requested.
- Keep response envelope fields compatible with frontend expectations (`statusCode`, `message`, `error`, `data`).
- Preserve auth flow expectations.

## Security and Reliability
- Never hardcode secrets or tokens.
- Validate/sanitize external input. Do not expose stack traces.
- Use structured logging with contextual fields.

## Accepted Review Exceptions
- `JobHunter.Api/appsettings.json` and `JobHunter.Api/appsettings.Development.json` are intentionally local-only.
- Missing automated test project/coverage is currently an accepted constraint; do not score or grade this as a review finding unless explicitly requested.
