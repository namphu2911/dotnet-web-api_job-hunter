# Review: Migration Current Status vs Java + Plan 16-03

Date: 2026-03-17
Agent: JobHunter Reviewer
Severity Summary: Critical 1 / High 4 / Medium 5 / Low 2

## Findings

### Critical

1. SMTP credentials are committed in plaintext config

- Evidence:
  - JobHunter.Api/appsettings.json:19
  - JobHunter.Api/appsettings.json:20
  - JobHunter.Api/appsettings.Development.json:19
  - JobHunter.Api/appsettings.Development.json:20
- Impact:
  - Secret leakage risk (OWASP configuration/secrets management violation).
  - Compromise of real mailbox/app password if repository/shared logs are exposed.
- Remediation:
  - Remove real credentials from source-controlled config immediately.
  - Move Email:Username/Email:Password to environment variables or user-secrets.
  - Rotate compromised SMTP password now.

### High

2. Phase 5 controllers are still placeholders (behavior not implemented)

- Evidence:
  - JobHunter.Api/Controllers/JobsController.cs:24
  - JobHunter.Api/Controllers/JobsController.cs:57
  - JobHunter.Api/Controllers/ResumesController.cs:24
  - JobHunter.Api/Controllers/ResumesController.cs:65
  - JobHunter.Api/Controllers/SubscribersController.cs:24
  - JobHunter.Api/Controllers/SubscribersController.cs:41
- Impact:
  - API endpoints return 200/empty regardless of input, causing contract drift vs Java and functional failure.
- Remediation:
  - Wire each action to corresponding service methods.
  - Return proper codes and payloads (201 create, 200 read/update, 404 not-found, 400 validation).

3. Authorization logic mismatch: seeded role SUPER_ADMIN does not receive wildcard permission

- Evidence:
  - JobHunter.Infrastructure/DatabaseSeeder.cs:54 (seed role SUPER_ADMIN)
  - JobHunter.Application/Services/AuthService.cs:160 (wildcard granted only for role ADMIN)
- Impact:
  - HasPermission-protected endpoints may incorrectly return 403 for seeded admin accounts.
- Remediation:
  - Align role naming/permission extraction strategy.
  - Preferred: derive permissions from Role->Permissions table instead of hardcoded role-name check.

4. Repositories for Job/Resume/Subscriber do not include required navigation data

- Evidence:
  - JobHunter.Infrastructure/Repositories/JobRepository.cs:20
  - JobHunter.Infrastructure/Repositories/ResumeRepository.cs:20
  - JobHunter.Infrastructure/Repositories/SubscriberRepository.cs:20
- Impact:
  - Service mapping depends on navigation properties (skills/company/user/job). Missing Include() leads to null/empty DTO fields and inconsistent outputs.
- Remediation:
  - Add Include()/ThenInclude() for required relations in GetById/GetAll query paths.

5. Database seeder is incomplete relative to Java baseline (34 permissions)

- Evidence:
  - JobHunter.Infrastructure/DatabaseSeeder.cs:38
- Impact:
  - Permission matrix is not parity with Java; policy checks and feature access can be incomplete.
- Remediation:
  - Import full permission list from Java DatabaseInitializer and seed idempotently.

### Medium

6. Services for Job/Resume implement filtering/pagination in-memory instead of repository/query-level

- Evidence:
  - JobHunter.Application/Services/JobService.cs:117
  - JobHunter.Application/Services/ResumeService.cs:79
- Impact:
  - Performance and memory degradation on large datasets.
  - Divergence from Java Specification-based filtering.
- Remediation:
  - Push filter/paging down to repository (IQueryable + DB-side pagination).

7. Enum parsing silently downgrades invalid values to Unknown

- Evidence:
  - JobHunter.Application/Services/JobService.cs:45
  - JobHunter.Application/Services/ResumeService.cs:38
- Impact:
  - Invalid client input can be accepted silently; behavior differs from strict validation expectations.
- Remediation:
  - Validate enum strings and return 400 with field-level error when invalid.

8. Job DTO mapping writes DateTime.MinValue when source dates are null

- Evidence:
  - JobHunter.Application/Services/JobService.cs:149
  - JobHunter.Application/Services/JobService.cs:150
- Impact:
  - Contract ambiguity and data quality drift (fake dates exposed to clients).
- Remediation:
  - Make response date fields nullable or omit when missing.

9. Scheduled email job still uses hardcoded recipient and placeholder logic

- Evidence:
  - JobHunter.Infrastructure/EmailJobHostedService.cs:28
  - JobHunter.Infrastructure/EmailJobHostedService.cs:29
- Impact:
  - Not parity with Java subscriber job behavior; sends synthetic emails instead of subscriber-based content.
- Remediation:
  - Inject/use subscriber job flow (query subscribers + matching jobs + templated email).

10. MailController endpoint remains test-only behavior

- Evidence:
  - JobHunter.Api/Controllers/MailController.cs:20
- Impact:
  - Endpoint contract does not match Java intent (trigger sending job emails to subscribers).
- Remediation:
  - Replace test recipient flow with subscriber dispatch orchestration.

### Low

11. HomeController root redirect is still missing

- Evidence:
  - Missing file: JobHunter.Api/Controllers/HomeController.cs
- Impact:
  - Minor UX contract gap with Java root entry behavior.
- Remediation:
  - Add GET / redirect to swagger path.

12. No automated test project exists yet

- Evidence:
  - No \*Tests.csproj in solution.
- Impact:
  - Regression risk remains high while migration is ongoing.
- Remediation:
  - Create JobHunter.Tests (xUnit) with startup smoke + contract tests for migrated endpoints.

## Progress Report (vs Plan 2026-03-16 + MIGRATION.md)

- Completed or mostly completed:
  - Core repository set exists in Domain/Infrastructure (8/8).
  - Core service set exists in Application (11/11 files present).
  - Core controller set exists in API (11/11 files present, including Files/Mail).
  - ApiResponse envelope + ApiMessage + response filter are implemented.
  - GlobalExceptionMiddleware now maps PermissionException/StorageException/ValidationException.
  - Static resource mapping /storage is configured.
  - Swagger PersistAuthorization is enabled.

- Partially completed:
  - Phase 5 (Job/Resume/Subscriber): service logic exists, but controllers are not wired.
  - Database seeding exists but permission catalog is incomplete.
  - Scheduled jobs/email flow exists but still placeholder (not subscriber job parity).
  - Filter strategy exists only as simple string contains in services (not DSL-equivalent).

- Not completed:
  - HomeController root redirect.
  - Test foundation (JobHunter.Tests).
  - Full Java-equivalent permission enforcement model across all routes.

## Recommended Continuation Plan

1. Finish Phase 5 API wiring (highest priority)

- Implement JobsController/ResumesController/SubscribersController actions against services.
- Return correct status codes and consistent envelope.

2. Fix authz parity

- Resolve permissions from role-permission relationships in AuthService token generation.
- Align seeded admin role behavior with HasPermission policies.

3. Repair data retrieval correctness

- Add Include chains in Job/Resume/Subscriber repositories for DTO mapping requirements.
- Add repository-level pagination/filtering.

4. Complete cross-cutting parity

- Replace EmailJobHostedService placeholder with subscriber job dispatch flow.
- Replace MailController test send with trigger to subscriber dispatch flow.

5. Security hardening now

- Remove SMTP credentials from appsettings and rotate leaked password.
- Move secrets to environment/user-secrets.

6. Complete migration leftovers

- Add HomeController redirect endpoint.
- Finish full permissions seed list parity.

7. Add tests gate

- Create JobHunter.Tests with smoke tests and phase-5 endpoint contract tests.
