# BE Alignment Plan v2 (FE-First, Non-Breaking)

## 1) Objective

- Align current .NET backend with the existing frontend contract in src/config/api.ts and src/types/backend.d.ts.
- Keep frontend code unchanged.
- Apply minimal, production-safe, backward-compatible changes.

## 2) Guiding Principles

- FE contract is source of truth for route, payload, and response shape.
- Do not remove old payload shapes immediately; support parallel shapes for a transition window.
- Prefer small, testable increments over broad refactors.
- Keep response envelope stable: statusCode, message, error, data.

## 3) Verified Current State

### 3.1 Pagination state by module

- Already FE-shape (meta/result): Users.
- Has pagination but old shape (Page/PageSize/Total/Items): Jobs, Resumes.
- No FE-style pagination endpoint yet: Companies, Skills, Permissions, Roles, Subscribers.

### 3.2 Endpoint coverage mismatch

- FE calls Subscribers list/by-id/delete.
- BE currently exposes Subscribers create/update/skills only.

### 3.3 Payload mismatch

- Jobs request in FE uses object ids for skills/company in common paths.
- Resumes create in FE sends user.id and job.id.
- Roles and Subscribers can send id collections not strictly long[].
- BE currently expects strict long-based DTOs in these modules.

### 3.4 Auth mismatch

- FE expects user.role.permissions[] in login/account/refresh responses.
- BE auth DTO currently returns role id/name only.

### 3.5 Response shape mismatch in Jobs

- FE expects job.company object and job.skills object array.
- BE currently returns skills as List<string> and no company object in ResJobDto.

## 4) Scope

### 4.1 In scope

- Companies/Skills/Permissions/Roles/Subscribers list endpoints with FE pagination shape.
- Jobs/Resumes pagination shape migration to FE meta/result.
- Flexible payload parsing for Jobs/Resumes/Roles/Subscribers.
- Auth response extension to include role.permissions.
- Subscribers endpoint parity with FE calls.
- Backward-compatible bridge for old payload shapes.

### 4.2 Out of scope

- Frontend code changes.
- Database schema redesign not required by contract alignment.
- Permission model redesign.
- Unrelated refactor and code style churn.

## 5) Target Contract Specification

### 5.1 Envelope

- Keep envelope for all API responses:
  - statusCode: number
  - message: string or validation object list
  - error: string (optional)
  - data: payload or null

### 5.2 Pagination

- Standardize data payload to:
  - meta: { page, pageSize, pages, total }
  - result: []

### 5.3 Query compatibility

- Accept aliases in all list endpoints:
  - page or current -> page
  - size or pageSize -> pageSize
- Keep optional filter query support using FE format currently in use.

### 5.4 Flexible id parsing rules

- Supported input forms:
  - number: 123
  - numeric string: "123"
  - object id ref: { id: 123 } or { id: "123" }
- For array ids:
  - [1,2], ["1","2"], [{id:1},{id:"2"}] are all valid.
- Invalid shape returns 400 with safe, clear message.

### 5.5 Module-specific target

- Jobs create/update:
  - skills: support id array and object-id array.
  - company: support id scalar and object-id.
  - response: include company {id,name,logo?} and skills [{id,name}].
- Resumes create:
  - support both user/job scalar id and object-id shape.
- Roles create/update:
  - permissions accepts number/string/object id arrays.
- Subscribers create/update:
  - skills accepts number/string/object id arrays.
- Auth login/account/refresh:
  - user.role.permissions[] included.
- Subscribers endpoints:
  - add GET /api/v1/subscribers
  - add GET /api/v1/subscribers/{id}
  - add DELETE /api/v1/subscribers/{id}

## 6) Implementation Design

### 6.1 Shared contracts and helpers

- Add flexible input DTO helpers in Application.Contracts:
  - ReqObjectIdDto-like id ref for mixed numeric/string id.
  - wrapper DTOs for scalar-or-object id.
  - wrapper DTOs for mixed id array.
- Add parsing helper in Application layer (single place):
  - ParseSingleId(...)
  - ParseIdList(...)
  - normalize to List<long>.
- Add pagination mapper helper:
  - Convert old paging result to FE meta/result shape.

### 6.2 Controller strategy

- Keep existing routes unchanged.
- Expand request binding contracts for mixed payloads.
- Keep response envelope behavior via existing filter/middleware.
- Return ActionResult<T> where multiple outcomes exist.

### 6.3 Service strategy

- Service receives normalized ids only (long/List<long>) after mapping.
- Validate existence and business rules as before.
- Keep cancellation tokens propagated.

### 6.4 Backward compatibility window

- Support old and new payload shape in parallel for 1-2 releases.
- Add deprecation note in docs for old-only shape paths.

## 7) Delivery Plan (Phased)

### Phase 0 - Baseline and guardrails

- Freeze current FE-used endpoints list.
- Capture baseline samples for key responses (auth, jobs, resumes, subscribers).
- Define quick regression checklist.

Done when:

- Baseline contract checklist documented and approved.

### Phase 1 - Subscribers parity

- Add missing Subscribers GET list/by-id/delete endpoints.
- Add list pagination FE shape.
- Extend service abstraction and implementation accordingly.

Done when:

- FE subscriber API set is fully callable without FE change.

### Phase 2 - Pagination normalization

- Convert Jobs and Resumes list responses to FE meta/result.
- Add pagination/filter support to Companies/Skills/Permissions/Roles/Subscribers list.
- Keep Users behavior unchanged.

Done when:

- All FE list endpoints return same FE pagination shape.

### Phase 3 - Flexible payload inputs

- Implement mixed id parsing for Jobs/Resumes/Roles/Subscribers.
- Add validation errors for invalid mixed payload.

Done when:

- No shape-related 4xx on FE normal create/update flows.

### Phase 4 - Auth contract completion

- Extend login/account/refresh role object with permissions[].
- Ensure no sensitive internal fields leaked.

Done when:

- FE account model reads role.permissions without fallback hacks.

### Phase 5 - Hardening and docs

- Final pass for error handling consistency.
- Update API docs and migration notes.
- Confirm no route or envelope regression.

Done when:

- Build green, smoke checklist green, docs updated.

## 8) Acceptance Criteria

- FE keeps current code and can call all existing APIs successfully.
- List APIs used by FE return data.meta/data.result shape.
- Jobs/Resumes/Roles/Subscribers accept mixed id payload forms described above.
- Subscribers has complete endpoint coverage used by FE.
- Auth login/account/refresh includes user.role.permissions[].
- Envelope keys stay compatible: statusCode, message, error, data.
- No new high-severity build issues.

## 9) Test Strategy

### 9.1 Unit tests

- Flexible id parser:
  - number/string/object id single parse.
  - mixed arrays parse.
  - invalid shape failure path.
- Pagination mapper:
  - total/pages calculation.
  - empty result handling.

### 9.2 API/integration tests

- Happy path for each FE-used endpoint group.
- Failure paths:
  - invalid id shape
  - unknown referenced ids
  - malformed filter/pagination values
- Contract assertions:
  - envelope keys
  - pagination meta/result keys
  - auth role.permissions presence

### 9.3 Current repository constraint

- Solution currently has no dedicated .NET test project.
- Add at least one test project before claiming test coverage completion.

## 10) Risks and Mitigations

- Risk: Breaking non-FE clients using old payload only.
  - Mitigation: parallel support window and clear deprecation notes.
- Risk: Complex parsing increases bug surface.
  - Mitigation: centralized parser and deterministic unit tests.
- Risk: Auth payload expansion impacts performance.
  - Mitigation: load only needed permission fields and verify query behavior.
- Risk: Inconsistent pagination/filter implementation across controllers.
  - Mitigation: shared query normalization helper.

## 11) Rollout and Rollback

- Rollout:
  - merge by phase order, deploy incrementally.
  - smoke FE critical flows after each phase.
- Rollback:
  - preserve old accepted payload paths during transition.
  - revert endpoint-specific changes if regression appears.

## 12) Verification Gates

Run from solution root:

```bash
dotnet restore
dotnet build
```

When test project is available:

```bash
dotnet test
```

Mandatory manual smoke after build:

- Auth: login, account, refresh, logout.
- Admin lists: companies, skills, permissions, roles, users.
- Jobs: list, create, update, get by id.
- Resumes: list, create, by-user.
- Subscribers: list, by id, create, update, delete, skills.

## 13) Deliverables

- Backend endpoints aligned with current FE contract.
- New/updated automated tests for changed behavior.
- Updated alignment document and API notes.
- Brief migration note for old payload deprecation timeline.
