# Review: Phase 1 Cross-cutting Foundation

Date: 2026-03-16
Agent: JobHunter Reviewer
Severity Summary: Critical: 1, High: 0, Medium: 2, Low: 1

## Findings

1. [Critical] Hardcoded JWT signing key is committed in source-controlled configuration.

- Evidence: [JobHunter.Api/appsettings.json](JobHunter.Api/appsettings.json#L8), [JobHunter.Api/appsettings.Development.json](JobHunter.Api/appsettings.Development.json#L8).
- Risk: Anyone with repository access can mint valid tokens, leading to authentication bypass across environments reusing this key.
- Remediation: Remove signing keys from tracked config files; load from environment variables or user-secrets only. Keep placeholder values in JSON.

2. [Medium] Validation response does not provide field-level error details as required by migration parity.

- Evidence: [JobHunter.Api/Program.cs](JobHunter.Api/Program.cs#L30) only extracts plain error messages; [JobHunter.Api/Middleware/GlobalExceptionMiddleware.cs](JobHunter.Api/Middleware/GlobalExceptionMiddleware.cs#L80) returns a single ValidationException message.
- Risk: Clients cannot reliably map errors to specific fields, and behavior diverges from Java `MethodArgumentNotValidException` handling.
- Remediation: Return a structured field-error payload (for example `{ field, message }[]` or a dictionary keyed by field name) in both model-binding and exception paths.

3. [Medium] Error envelope contract is inconsistent between middleware and result filter for 4xx responses.

- Evidence: [JobHunter.Api/Middleware/GlobalExceptionMiddleware.cs](JobHunter.Api/Middleware/GlobalExceptionMiddleware.cs#L61) sets 400 error to `Exception occurs...`, while [JobHunter.Api/Filters/ApiResponseEnvelopeFilter.cs](JobHunter.Api/Filters/ApiResponseEnvelopeFilter.cs#L118) and [JobHunter.Api/Filters/ApiResponseEnvelopeFilter.cs](JobHunter.Api/Filters/ApiResponseEnvelopeFilter.cs#L139) derive error from HTTP reason phrase.
- Risk: Same logical failure can produce different `error` values depending on whether it is thrown as exception or returned from controller, making clients brittle.
- Remediation: Centralize error-envelope construction in one shared policy/service and reuse it from middleware and response filter.

4. [Low] No automated tests were added for the new global envelope and exception behavior.

- Evidence: No test project exists (`**/*Tests*.csproj` not found), while Phase 1 changed global API response formatting.
- Risk: Contract regressions may go unnoticed when future controllers/services are migrated.
- Remediation: Add a test project with integration tests covering success envelope shape, model validation envelope, and key exception mappings.

## Open Questions

- Should the project preserve exact Java error text values for all 4xx cases, or is semantic parity sufficient?
- Is there any existing client already integrated with current .NET responses that requires a staged rollout for envelope changes?

## Secondary Summary

Phase 1 establishes the foundation successfully, but the current implementation introduces a critical secret-management issue and still has contract consistency gaps in validation and 4xx error shapes.
