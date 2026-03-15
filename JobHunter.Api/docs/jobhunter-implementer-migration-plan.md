# JobHunter .NET 8 Migration Plan

## Scope

Reimplement the Java Spring JobHunter system as ASP.NET Core Web API (.NET 8) with equivalent layers:

1. Configuration
2. Security (JWT)
3. Permission system
4. Controllers
5. Services
6. Repositories
7. Domain entities
8. Request and response DTOs
9. File upload
10. Email service
11. Pagination responses
12. Global exception handling
13. Utility classes
14. Enum constants

## Target Stack (Libraries and Tools)

- Runtime: .NET 8
- API: ASP.NET Core Web API
- ORM: Entity Framework Core + Pomelo.EntityFrameworkCore.MySql
- Auth: Microsoft.AspNetCore.Authentication.JwtBearer
- Authorization: Policy-based authorization + custom permission handler
- Validation: FluentValidation.AspNetCore
- Mapping: AutoMapper.Extensions.Microsoft.DependencyInjection
- Email: MailKit
- API docs: Swashbuckle.AspNetCore
- Logging: Serilog.AspNetCore
- Resilience: Polly
- Tests: xUnit, FluentAssertions, Moq, Microsoft.AspNetCore.Mvc.Testing

## Phase Plan for JobHunter Implementer

## Current Progress

Status date: 2026-03-15

- Phase 1: In progress
- Phase 4: In progress
- Phase 7: In progress

Completed in current iteration:

1. Added EF Core MySQL infrastructure wiring and DbContext scaffold.
2. Added JWT token service and ASP.NET authentication configuration.
3. Added dynamic permission policy provider and permission authorization handler.
4. Added global exception middleware and pipeline registration.
5. Migrated auth service from in-memory access token dictionary to JWT-based validation.
6. Added refresh token expiration support in domain and repository contracts.

### Phase 0: Baseline and Contract Freeze

Objective: lock current API behavior and migration boundaries.

Steps:

1. Build endpoint inventory from Java controllers and map to .NET route plan.
2. Freeze request and response contracts that must remain compatible.
3. Identify security-sensitive flows: login, refresh, logout, change-password, upload, mail.
4. Define non-goals for first migration cut to avoid scope creep.

Deliverables:

- Endpoint compatibility matrix
- Contract compatibility checklist

Exit criteria:

- All Java API modules are mapped to target .NET modules

### Phase 1: Solution Architecture and Project Wiring

Objective: ensure clean layering and dependency flow.

Steps:

1. Confirm project responsibilities:
   - JobHunter.Api: controllers, middleware, auth wiring
   - JobHunter.Application: use cases, service interfaces, DTOs
   - JobHunter.Domain: entities, enums, abstractions
   - JobHunter.Infrastructure: EF Core, repositories, external integrations
2. Configure dependency injection entry points per project.
3. Add base configuration sections for JWT, database, file storage, and SMTP.

Deliverables:

- Layer dependency diagram
- Registered service map

Exit criteria:

- API boots successfully with placeholder services

### Phase 2: Domain and DTO Migration

Objective: model the core business and API contracts.

Steps:

1. Create entities and relationships:
   - User, Role, Permission
   - Company, Job, Resume
   - Skill, Subscriber
2. Add enum constants migrated from Java constants.
3. Create request and response DTOs per endpoint group.
4. Add mapping profiles and contract tests for DTO serialization.

Deliverables:

- Complete entity model set
- DTO model set for all endpoint groups

Exit criteria:

- No controller is exposing entities directly

### Phase 3: Persistence Layer (EF Core + MySQL)

Objective: replace Spring Data JPA with EF Core repositories.

Steps:

1. Implement DbContext and Fluent API configurations.
2. Add repository interfaces and concrete implementations.
3. Implement paging, filtering, and sorting primitives.
4. Create initial migration and verify schema generation against MySQL.

Deliverables:

- DbContext and entity configurations
- Repository implementations per aggregate
- Baseline EF migration

Exit criteria:

- CRUD operations work for each aggregate in integration tests

### Phase 4: Authentication and Authorization

Objective: match Spring Security behavior with JWT and permission checks.

Steps:

1. Configure JWT bearer authentication.
2. Implement token creation and refresh workflow.
3. Add policy-based role checks.
4. Add custom permission requirement and authorization handler.
5. Apply authorization attributes and policies on controllers.

Deliverables:

- Auth service and token contracts
- Permission policy infrastructure

Exit criteria:

- Protected endpoints return expected 401 or 403 behavior

### Phase 5: Service and Controller Implementation

Objective: migrate Java business logic and endpoints module-by-module.

Steps:

1. Implement services for auth, users, companies, jobs, skills, roles, permissions, resumes, subscribers.
2. Implement controllers with ActionResult<T>, model validation, and status code discipline.
3. Maintain route compatibility where possible.
4. Add OpenAPI metadata for each endpoint group.

Deliverables:

- Service implementations
- Controller implementations
- Swagger contract visibility

Exit criteria:

- Endpoint matrix completion reaches 100 percent

### Phase 6: File Upload and Email

Objective: complete external I/O features safely.

Steps:

1. Implement file upload and listing endpoints.
2. Add file validation: extension allowlist, content length limits, generated safe names.
3. Implement SMTP email sender service.
4. Add template-friendly message composition contract.

Deliverables:

- Upload API and storage abstraction
- Email sending service

Exit criteria:

- Upload and email paths pass success and failure tests

### Phase 7: Cross-Cutting Middleware and Utilities

Objective: centralize error handling and reusable behaviors.

Steps:

1. Implement global exception middleware with safe error payloads.
2. Add standardized pagination response wrapper.
3. Add utility services (current user context, time provider, response helpers).
4. Add structured logging and correlation support.

Deliverables:

- Exception middleware
- Pagination response contracts
- Utility class set

Exit criteria:

- Unhandled exceptions are consistently translated to safe API responses

### Phase 8: Testing and Verification Gate

Objective: validate migrated behavior before release.

Steps:

1. Add unit tests for services, auth logic, and permission checks.
2. Add API integration tests for:
   - login and refresh
   - authorization enforcement
   - CRUD representative endpoints
   - upload endpoint
   - pagination payloads
   - global error responses
3. Run verification commands:
   - dotnet restore
   - dotnet build
   - dotnet test

Deliverables:

- Passing test suite
- Migration validation report

Exit criteria:

- All critical flows pass and no blocker defects remain

## Implementation Sequence (Recommended)

1. Auth + User + Role + Permission first
2. Company + Job + Skill second
3. Resume + Subscriber third
4. File + Mail fourth
5. Hardening and test expansion last

## Risk Controls

- Contract drift: maintain endpoint compatibility matrix and contract tests.
- Security gaps: enforce policy coverage checklist for every protected endpoint.
- Data mismatches: validate relationship and cascade rules in integration tests.
- Secret leakage: keep JWT, DB, SMTP secrets in environment or secret stores only.

## Definition of Done

1. All required layers are implemented and wired.
2. Authentication and permission-based authorization are enforced.
3. File upload and email services are working with validation and error handling.
4. Pagination and global exception middleware are active across API modules.
5. Build and tests pass on repository root commands.
6. API contract changes, if any, are documented explicitly.
