---
description: "Testing guidance for behavior changes in API and backend logic. Use when adding features, fixing bugs, or refactoring behavior."
applyTo: "**/*.{cs,csproj,sln}"
---

# Testing Instructions

## Test Expectations

- Any behavior change should include or update automated tests.
- Cover happy path, edge cases, and one failure path at minimum.
- Keep tests deterministic and isolated.

## API Testing Focus

- Verify status code and response contract.
- Verify validation and error handling paths.
- Verify route behavior and serialization assumptions.

## Frontend Compatibility Checks (JobHunter)

- For backend behavior changes, verify compatibility with `src/config/api.ts` and `src/types/backend.d.ts`.
- Ensure response envelope fields expected by frontend remain present (`statusCode`, `message`, `error`, `data`).
- Include coverage for auth/session flows that frontend depends on (`/api/v1/auth/login`, `/api/v1/auth/account`, `/api/v1/auth/refresh`, `/api/v1/auth/logout`).
- If Java behavior and frontend expectations differ, test and preserve frontend-facing behavior unless explicitly requested otherwise.

## Quality Rules

- Avoid brittle timing-dependent tests.
- Use clear Arrange-Act-Assert structure.
- Prefer descriptive test names that encode expected behavior.

## Build and Test Gate

Before finishing implementation:

- dotnet build

If test projects are not present, explicitly report this and suggest creating one.
