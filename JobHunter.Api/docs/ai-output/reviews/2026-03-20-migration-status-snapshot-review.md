# Review: Migration Status Snapshot (20-03-2026)

Date: 2026-03-20
Agent: JobHunter Reviewer
Severity Summary: Critical 1 / High 3 / Medium 4 / Low 2

## Findings

### Critical

1. SMTP credentials are still committed in plaintext configuration

- Evidence:
  - [JobHunter.Api/appsettings.json](JobHunter.Api/appsettings.json#L19)
  - [JobHunter.Api/appsettings.json](JobHunter.Api/appsettings.json#L20)
  - [JobHunter.Api/appsettings.Development.json](JobHunter.Api/appsettings.Development.json#L19)
  - [JobHunter.Api/appsettings.Development.json](JobHunter.Api/appsettings.Development.json#L20)
- Impact:
  - Immediate secret-exposure risk and possible account compromise.
- Remediation:
  - Remove credentials from tracked files.
  - Move to environment variables or dotnet user-secrets.
  - Rotate leaked SMTP app password.

### High

2. Permission claims are generated with ApiPath values but endpoint policies require action keys

- Evidence:
  - Policies require keys such as [job:create](JobHunter.Api/Controllers/JobsController.cs#L21), [resume:create](JobHunter.Api/Controllers/ResumesController.cs#L21), [subscriber:create](JobHunter.Api/Controllers/SubscribersController.cs#L21).
  - Auth token permissions for non-admin users are generated from ApiPath at [AuthService permission mapping](JobHunter.Application/Services/AuthService.cs#L170).
  - Authorization handler checks exact equality with requirement at [PermissionAuthorizationHandler](JobHunter.Api/Authorization/PermissionAuthorizationHandler.cs#L10).
- Impact:
  - Non-admin users will fail HasPermission checks even if role permissions exist in DB.
- Remediation:
  - Standardize one permission identity model end-to-end (for example module:action keys).
  - Generate token claims using the exact same value HasPermission expects.

3. Resume by-user endpoint can reject valid authenticated users due to claim-selection logic

- Evidence:
  - User id extraction at [ResumesController](JobHunter.Api/Controllers/ResumesController.cs#L92).
  - Access token includes subject as email and nameidentifier as id at [JwtTokenService sub claim](JobHunter.Infrastructure/Authentication/JwtTokenService.cs#L25) and [nameidentifier claim](JobHunter.Infrastructure/Authentication/JwtTokenService.cs#L27).
- Impact:
  - Endpoint may read email claim first, fail long parse, and return Unauthorized incorrectly.
- Remediation:
  - Read ClaimTypes.NameIdentifier first and parse only that claim for user id.

4. Seeded admin password is plain text while authentication requires BCrypt hash

- Evidence:
  - Seeder sets plain password at [DatabaseSeeder](JobHunter.Infrastructure/DatabaseSeeder.cs#L113).
  - Login verification uses BCrypt at [PasswordSecurity.VerifyPassword](JobHunter.Application/Services/Security/PasswordSecurity.cs#L12).
- Impact:
  - Seeded admin login can fail and introduces security weakness.
- Remediation:
  - Hash seeded password before save using the same PasswordSecurity.HashPassword flow.

### Medium

5. Filter strategy rollout is still incomplete and not Java-DSL parity

- Evidence:
  - Users endpoint only supports page/pageSize at [UsersController](JobHunter.Api/Controllers/UsersController.cs#L60).
  - Filter is only present in selected modules at [JobsController list](JobHunter.Api/Controllers/JobsController.cs#L81) and [ResumesController list](JobHunter.Api/Controllers/ResumesController.cs#L81).
- Impact:
  - Plan step 7 remains partial; contract/features differ from Java SpringFilter usage.
- Remediation:
  - Choose one consistent approach (Sieve/OData/custom parser) and apply to all list endpoints.

6. Email template rendering remains placeholder (no actual template engine usage)

- Evidence:
  - Placeholder implementation at [EmailService](JobHunter.Application/Services/EmailService.cs#L58).
- Impact:
  - Output formatting and behavior differ from Java Thymeleaf template flow.
- Remediation:
  - Implement template rendering from file (for example RazorLight/Scriban) and load job template content.

7. Manual Mail endpoint uses service locator and duplicates hosted-job orchestration logic

- Evidence:
  - Runtime service lookup at [MailController](JobHunter.Api/Controllers/MailController.cs#L22).
  - Duplicated dispatch logic in [MailController](JobHunter.Api/Controllers/MailController.cs#L27) and [EmailJobHostedService](JobHunter.Infrastructure/EmailJobHostedService.cs#L30).
- Impact:
  - Harder maintenance/testing and higher regression risk when changing dispatch flow.
- Remediation:
  - Extract one application service for subscriber job dispatch and reuse from both trigger points.

8. Automated test foundation is still missing

- Evidence:
  - No test project found (\*Tests.csproj absent in workspace).
- Impact:
  - High regression risk across ongoing migration phases.
- Remediation:
  - Create JobHunter.Tests with smoke tests and contract tests for migrated endpoints.

### Low

9. Home redirect path differs from Java contract

- Evidence:
  - Java redirects to /swagger-ui.html at [src/main/java/vn/phunn/jobhunter/controller/HomeController.java](src/main/java/vn/phunn/jobhunter/controller/HomeController.java#L10).
  - .NET redirects to /swagger/index.html at [JobHunter.Api/Controllers/HomeController.cs](JobHunter.Api/Controllers/HomeController.cs#L12).
- Impact:
  - Minor path difference; functionally acceptable in Swashbuckle.
- Remediation:
  - Optional: keep current path or add compatibility redirect if strict parity is required.

10. Migration tracking document is stale versus current implementation

- Evidence:
  - [JobHunter.Api/docs/MIGRATION.md](JobHunter.Api/docs/MIGRATION.md) still marks several items as not migrated although code now exists.
- Impact:
  - Planning/reporting drift and misleading status communication.
- Remediation:
  - Update migration matrix to reflect 20-03 status with completed/partial/not-started per module.

## Progress Report (vs Plan 2026-03-16, review 2026-03-17, and MIGRATION.md)

- Improved since 2026-03-17:
  - Phase 5 controllers are now wired with real service calls:
    - [JobHunter.Api/Controllers/JobsController.cs](JobHunter.Api/Controllers/JobsController.cs)
    - [JobHunter.Api/Controllers/ResumesController.cs](JobHunter.Api/Controllers/ResumesController.cs)
    - [JobHunter.Api/Controllers/SubscribersController.cs](JobHunter.Api/Controllers/SubscribersController.cs)
  - Job/Resume repositories now include navigation data and DB-level paging/filtering:
    - [JobHunter.Infrastructure/Repositories/JobRepository.cs](JobHunter.Infrastructure/Repositories/JobRepository.cs)
    - [JobHunter.Infrastructure/Repositories/ResumeRepository.cs](JobHunter.Infrastructure/Repositories/ResumeRepository.cs)
  - Scheduled job and mail trigger now execute subscriber/job dispatch flow (no longer hardcoded single recipient).
  - Home endpoint exists:
    - [JobHunter.Api/Controllers/HomeController.cs](JobHunter.Api/Controllers/HomeController.cs)

- Still partial:
  - Authorization parity is not complete (claim model mismatch with HasPermission).
  - Email templating parity is not complete (placeholder rendering).
  - Filter strategy rollout is not complete across all list endpoints.

- Still not completed:
  - Test foundation (JobHunter.Tests).
  - Secrets hardening in configuration files.

## Recommended Continuation Plan

1. Security hotfix first

- Remove SMTP credentials from appsettings files.
- Rotate SMTP app password.
- Adopt environment/user-secrets for Email credentials.

2. Permission model alignment

- Define canonical permission key format.
- Update seeder + token generation + HasPermission attributes to same format.
- Add authorization tests for success/forbidden cases.

3. Fix resume by-user claim extraction

- Use NameIdentifier claim only for user id parsing.
- Add one integration test for /api/v1/resumes/by-user.

4. Seeder/auth correctness

- Hash seeded admin password.
- Verify seeded admin can login and call HasPermission endpoints.

5. Complete filter rollout

- Apply consistent filter contract to Users/Companies/Skills/Permissions/Roles and validate behavior.

6. Email parity refactor

- Introduce shared job-dispatch service.
- Implement template engine-backed HTML generation.

7. Build test gate

- Create JobHunter.Tests with startup smoke and critical endpoint contract tests.
