# Review: Whole Project Progress Snapshot (02-04-2026)

Date: 2026-04-02  
Agent: JobHunter Reviewer  
Scope: Frontend contract compatibility, backend migration progress, build/test readiness, and security posture.

Severity Summary: Critical 1 / High 4 / Medium 4 / Low 1

## Findings

### Critical

1. SMTP credentials are still committed in plaintext configuration

- Evidence:
  - [JobHunter.Api/appsettings.json](JobHunter.Api/appsettings.json#L19)
  - [JobHunter.Api/appsettings.json](JobHunter.Api/appsettings.json#L20)
  - [JobHunter.Api/appsettings.Development.json](JobHunter.Api/appsettings.Development.json#L19)
  - [JobHunter.Api/appsettings.Development.json](JobHunter.Api/appsettings.Development.json#L20)
- Impact:
  - Immediate secret exposure risk and mailbox credential compromise.
- Remediation:
  - Remove secrets from tracked config.
  - Move to environment variables or user-secrets.
  - Rotate compromised SMTP password.

### High

2. Permission identity model remains inconsistent (token claims vs endpoint requirements)

- Evidence:
  - HasPermission policies require action keys such as [job:create](JobHunter.Api/Controllers/JobsController.cs#L22), [resume:create](JobHunter.Api/Controllers/ResumesController.cs#L22), [subscriber:create](JobHunter.Api/Controllers/SubscribersController.cs#L23).
  - Authorization handler performs exact claim equality against required key at [PermissionAuthorizationHandler](JobHunter.Api/Authorization/PermissionAuthorizationHandler.cs#L10).
  - Non-admin claims are emitted from permission ApiPath values in [AuthService](JobHunter.Application/Services/AuthService.cs#L193).
- Impact:
  - Non-admin users with valid DB permissions can still fail HasPermission checks.
- Remediation:
  - Standardize one canonical permission key format end-to-end.
  - Emit claims using the same value HasPermission expects.

3. Resume by-user endpoint still risks false Unauthorized due to claim precedence

- Evidence:
  - Endpoint reads first matching claim between sub and nameidentifier at [ResumesController](JobHunter.Api/Controllers/ResumesController.cs#L93).
  - Access token sub is email at [JwtTokenService](JobHunter.Infrastructure/Authentication/JwtTokenService.cs#L25), while numeric user id is in NameIdentifier at [JwtTokenService](JobHunter.Infrastructure/Authentication/JwtTokenService.cs#L27).
- Impact:
  - Valid authenticated users can be rejected if sub is selected first and long parse fails.
- Remediation:
  - Parse user id from ClaimTypes.NameIdentifier only.

4. Flexible payload compatibility is partial, not fully FE-shape tolerant yet

- Evidence:
  - Core id wrapper is strict long id at [ReqObjectIdDto](JobHunter.Application/Contracts/ReqObjectIdDto.cs#L5).
  - Request DTOs still require object wrappers for ids in key FE paths:
    - [ReqCreateJobDto skills/company](JobHunter.Application/Contracts/Jobs/ReqCreateJobDto.cs#L14)
    - [ReqCreateResumeDto user/job](JobHunter.Application/Contracts/Resumes/ReqCreateResumeDto.cs#L8)
    - [ReqCreateRoleDto permissions](JobHunter.Application/Contracts/Roles/ReqCreateRoleDto.cs#L10)
    - [ReqCreateSubscriberDto skills](JobHunter.Application/Contracts/Subscribers/ReqCreateSubscriberDto.cs#L7)
  - FE plan requires accepting scalar and numeric string ids in addition to object-id form at [be-alignment-plan-with-existing-fe.md](JobHunter.Api/docs/be-alignment-plan-with-existing-fe.md#L80).
- Impact:
  - Requests using scalar or string id forms can fail model binding or validation in modules targeted by Phase 3.
- Remediation:
  - Add converters/wrapper contracts to accept number, numeric string, and object-id forms for single and list ids.

5. Seeded admin password is still plain text

- Evidence:
  - Plain password assignment remains in [DatabaseSeeder](JobHunter.Infrastructure/DatabaseSeeder.cs#L113).
- Impact:
  - Security weakness and potential auth inconsistency with BCrypt verification path.
- Remediation:
  - Hash seeded password with the same PasswordSecurity flow used by auth.

### Medium

6. Automated test foundation is still missing

- Evidence:
  - No test project found from workspace search for \*Tests.csproj.
  - Plan already calls out missing test project at [be-alignment-plan-with-existing-fe.md](JobHunter.Api/docs/be-alignment-plan-with-existing-fe.md#L245).
- Impact:
  - High regression risk during ongoing migration and contract alignment.
- Remediation:
  - Create a .NET test project and add contract-focused integration tests.

7. Email template rendering remains placeholder implementation

- Evidence:
  - Placeholder comment and inline serialized object body at [EmailService](JobHunter.Application/Services/EmailService.cs#L58).
- Impact:
  - Output quality and behavior still differ from template-based parity expectations.
- Remediation:
  - Implement template file rendering (for example RazorLight/Scriban) for job emails.

8. Mail trigger endpoint still uses service locator and duplicated dispatch flow

- Evidence:
  - Service locator usage in [MailController](JobHunter.Api/Controllers/MailController.cs#L24).
  - Duplicated orchestration logic with hosted service in [EmailJobHostedService](JobHunter.Infrastructure/EmailJobHostedService.cs#L17).
- Impact:
  - Harder testability and maintainability; higher divergence risk between manual and scheduled paths.
- Remediation:
  - Extract one application dispatch service and reuse from both controller and hosted job.

9. Nullable contract warnings remain in build output for request DTOs

- Evidence:
  - Warnings in build for required id objects in:
    - [ReqCreateResumeDto User](JobHunter.Application/Contracts/Resumes/ReqCreateResumeDto.cs#L8)
    - [ReqCreateResumeDto Job](JobHunter.Application/Contracts/Resumes/ReqCreateResumeDto.cs#L9)
    - [ReqCreateJobDto Company](JobHunter.Application/Contracts/Jobs/ReqCreateJobDto.cs#L15)
    - [ReqUpdateJobDto Company](JobHunter.Application/Contracts/Jobs/ReqUpdateJobDto.cs#L16)
- Impact:
  - Increased risk of null-related runtime faults at API boundaries.
- Remediation:
  - Use required members/validation attributes or nullable handling consistently.

### Low

10. Migration tracking document remains stale versus current implementation

- Evidence:
  - Several items in [MIGRATION.md](JobHunter.Api/docs/MIGRATION.md) are still marked not migrated despite implemented controllers/services/repositories.
- Impact:
  - Planning and reporting drift.
- Remediation:
  - Refresh migration matrix to match current code state.

## Progress Snapshot (Positive)

- FE pagination shape is broadly adopted:
  - Query aliases page/current and size/pageSize are implemented at [ListQueryParameters](JobHunter.Api/Contracts/ListQueryParameters.cs#L5).
  - Controllers use resolved pagination values across modules (users, companies, skills, permissions, roles, jobs, resumes, subscribers), for example [UsersController](JobHunter.Api/Controllers/UsersController.cs#L61), [CompaniesController](JobHunter.Api/Controllers/CompaniesController.cs#L34), [SubscribersController](JobHunter.Api/Controllers/SubscribersController.cs#L39).
  - Shared response supports meta/result and compatibility fields at [ResultPaginationDto](JobHunter.Application/Contracts/ResultPaginationDto.cs#L5).

- Subscribers endpoint parity with FE is now present:
  - List, by-id, delete, skills endpoints exist in [SubscribersController](JobHunter.Api/Controllers/SubscribersController.cs#L37).

- Jobs response shape moved toward FE expectation:
  - Company object and skills object array are exposed in [ResJobDto](JobHunter.Application/Contracts/Jobs/ResJobDto.cs#L19).

- Auth response includes role.permissions[]:
  - DTO and mapping present at [ResLoginDto](JobHunter.Application/Contracts/Auth/ResLoginDto.cs#L28) and [AuthService](JobHunter.Application/Services/AuthService.cs#L157).

- Repository includes for job/resume relations are in place:
  - [JobRepository](JobHunter.Infrastructure/Repositories/JobRepository.cs#L20)
  - [ResumeRepository](JobHunter.Infrastructure/Repositories/ResumeRepository.cs#L20)

## Phase Status vs BE Alignment Plan

1. Phase 1 - Subscribers parity: Mostly done (list/by-id/delete/skills/create/update available).
2. Phase 2 - Pagination normalization: Largely done (meta/result everywhere, alias query support in place).
3. Phase 3 - Flexible payload inputs: Partial (DTO/model binding still strict object-id in key endpoints).
4. Phase 4 - Auth contract completion: Partial (role.permissions in response done; permission-claim model still mismatched for HasPermission).
5. Phase 5 - Hardening and docs: Not done (secrets, test foundation, templating, migration doc freshness still open).

## Verification Gates Run

- dotnet restore: Passed
- dotnet build: Passed with warnings (5)
- dotnet test: Not run (no .NET test project present)

## Recommended Next Steps

1. Security hotfix now: remove committed SMTP credentials and rotate password.
2. Align permission model now: claims generation and HasPermission requirement must use one canonical key.
3. Fix by-user claim parsing: use NameIdentifier only.
4. Complete flexible id parsing with converters and add unit tests.
5. Create JobHunter.Tests and add contract tests for auth, jobs, resumes, subscribers.
6. Replace email placeholder rendering and remove service locator duplication.
7. Update MIGRATION.md to current status.
