# Review: Dotnet Unfinished Items Scan (03-04-2026)

Date: 2026-04-03
Scope: Scan docs + full .NET backend codebase (`JobHunter.Api`, `JobHunter.Application`, `JobHunter.Domain`, `JobHunter.Infrastructure`).

## 1) Cac hang muc con chua lam

### 1. Permission enforcement chua phu deu (moi mot phan endpoint dung HasPermission)

- Muc do: High
- Bang chung:
  - Da dung `HasPermission` o Users/Roles/Permissions/Jobs/Resumes/Subscribers.
  - Nhung Companies/Skills/Files/Mail va nhieu action read dang dung `[Authorize]` thay vi permission key chi tiet.
  - Files: `JobHunter.Api/Controllers/FilesController.cs`
  - Companies: `JobHunter.Api/Controllers/CompaniesController.cs`
  - Skills: `JobHunter.Api/Controllers/SkillsController.cs`
  - Mail: `JobHunter.Api/Controllers/MailController.cs`
- Anh huong:
  - Chua dat parity voi huong Java interceptor check permission theo endpoint/method.
- De xuat:
  - Chot matrix permission cho tat ca endpoint can bao ve.
  - Thong nhat dung `HasPermission` (hoac policy toan cuc + explicit exception cho endpoint public).

### 2. File upload hardening chua day du

- Muc do: Medium
- Bang chung:
  - Co config `FileStorage.MaxFileSizeMb` trong appsettings nhung chua duoc su dung trong code.
  - `JobHunter.Application/Services/FileService.cs` dang ghep path truc tiep tu `folder` + `fileName` (can hardening them de tranh path traversal).
- Anh huong:
  - Chua parity phan hardening cho upload/download production-safe.
- De xuat:
  - Validate `folder` theo allowlist, normalize path, chan `..` segments.
  - Enforce max file size tai API layer va server limits.

### 3. Filter DSL moi dat muc co ban, chua parity day du SpringFilter nang cao

- Muc do: Medium
- Bang chung:
  - `JobHunter.Infrastructure/Repositories/SpringFilterQuery.cs` hien parse chu yeu `contains (~)`, `equals (=)`, `in (...)`, sort.
  - Chua thay parser cho bieu thuc phuc hop (logic group, so sanh nang cao, nested operations).
- Anh huong:
  - Con khoang cach voi Java DSL neu can parity 100%.
- De xuat:
  - Xac dinh muc parity cuoi cung: giu parser toi gian theo FE hay nang cap DSL day du.

## 2) Cac muc da duoc fix so voi review cu

- Response envelope da co (`ApiResponse<T>` + `ApiResponseEnvelopeFilter`).
- Exception mapping da bo sung `PermissionException` (403), `StorageException` (400), validation envelope.
- Seed admin password da hash (`PasswordSecurity.HashPassword`).
- Permission claim model da dong bo theo `PermissionClaimValue.Encode(method, apiPath, module)`.
- Resume by-user da lay `ClaimTypes.NameIdentifier` dung uu tien.
- Mail endpoint + scheduled job da dung chung `IJobEmailDispatchService`.

## 3) Verification da chay trong lan quet nay

- `dotnet restore`: pass
- `dotnet build`: fail do file lock process (khong phai loi compile logic), dong thoi con warnings nullability nhu muc (3).

## 4) Uu tien de dong backlog

1. Tao test project va smoke/contract tests toi thieu.
2. Hoan tat matrix permission (HasPermission/global policy) cho endpoint can bao ve.
3. Xu ly nullability warnings o request DTO.
4. Hardening upload/download (max size + path validation).
5. Dong bo MIGRATION.md theo trang thai verified moi nhat.
