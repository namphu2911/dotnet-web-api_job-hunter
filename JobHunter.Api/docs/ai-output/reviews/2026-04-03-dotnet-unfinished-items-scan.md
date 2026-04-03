# Review: Dotnet Unfinished Items Scan (03-04-2026)

Date: 2026-04-03
Scope: Scan docs + full .NET backend codebase (`JobHunter.Api`, `JobHunter.Application`, `JobHunter.Domain`, `JobHunter.Infrastructure`).

## 1) Cac hang muc con chua lam

### 1. Chua co test project .NET (chua co gate test tu dong)

- Muc do: High
- Bang chung:
  - Solution hien chi co 4 file csproj backend, khong co project test (`**/*Tests*.csproj` khong tim thay).
  - `JobHunter.sln` chua co `JobHunter.Tests`.
- Anh huong:
  - Chua co regression gate cho envelope, auth/authz, filter va contract FE.
- De xuat:
  - Tao `JobHunter.Tests` (xUnit) va them smoke tests + contract tests cho Auth, Users, Jobs, Resumes, Subscribers.

### 2. Permission enforcement chua phu deu (moi mot phan endpoint dung HasPermission)

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

### 3. DTO nullability warnings van ton tai trong hop dong request

- Muc do: Medium
- Bang chung (dotnet build):
  - `JobHunter.Application/Contracts/Jobs/ReqCreateJobDto.cs` (`Company`)
  - `JobHunter.Application/Contracts/Jobs/ReqUpdateJobDto.cs` (`Company`)
  - `JobHunter.Application/Contracts/Resumes/ReqCreateResumeDto.cs` (`User`, `Job`)
- Anh huong:
  - Bien gioi API co nguy co null runtime va warning debt chua dong.
- De xuat:
  - Dung `required`, nullable ro rang, va/hoac validation attributes de ep payload hop le.

### 4. Build gate chua on dinh khi API dang chay (file lock)

- Muc do: Medium
- Bang chung:
  - Build tong the fail do `MSB3027/MSB3021` vi DLL bi lock boi process `JobHunter.Api` va Visual Studio.
- Anh huong:
  - Kho dat gate CI/local repeatable neu app dang chay nen.
- De xuat:
  - Chuan hoa quy trinh verify: stop API process truoc build gate (hoac build sang output folder rieng/CI clean workspace).

### 5. File upload hardening chua day du

- Muc do: Medium
- Bang chung:
  - Co config `FileStorage.MaxFileSizeMb` trong appsettings nhung chua duoc su dung trong code.
  - `JobHunter.Application/Services/FileService.cs` dang ghep path truc tiep tu `folder` + `fileName` (can hardening them de tranh path traversal).
- Anh huong:
  - Chua parity phan hardening cho upload/download production-safe.
- De xuat:
  - Validate `folder` theo allowlist, normalize path, chan `..` segments.
  - Enforce max file size tai API layer va server limits.

### 6. Filter DSL moi dat muc co ban, chua parity day du SpringFilter nang cao

- Muc do: Medium
- Bang chung:
  - `JobHunter.Infrastructure/Repositories/SpringFilterQuery.cs` hien parse chu yeu `contains (~)`, `equals (=)`, `in (...)`, sort.
  - Chua thay parser cho bieu thuc phuc hop (logic group, so sanh nang cao, nested operations).
- Anh huong:
  - Con khoang cach voi Java DSL neu can parity 100%.
- De xuat:
  - Xac dinh muc parity cuoi cung: giu parser toi gian theo FE hay nang cap DSL day du.

### 7. Home redirect chua trung 100% voi migration target trong docs

- Muc do: Low
- Bang chung:
  - `JobHunter.Api/Controllers/HomeController.cs` redirect `"/swagger/index.html"`.
  - Trong `MIGRATION.md` muc tieu lich su ghi redirect `"/swagger-ui.html"`.
- Anh huong:
  - Khong blocker, nhung chua dong nhat tai lieu va implementation.
- De xuat:
  - Chot 1 duong dan chinh thuc va update docs cho khop.

### 8. Tai lieu migration chua dong bo hoan toan voi code hien tai

- Muc do: Low
- Bang chung:
  - `MIGRATION.md` van con doan lich su/noi dung cu mo ta trang thai .NET khong con dung voi implementation moi o mot so muc.
  - Cac artifact review truoc day va ma nguon hien tai da thay doi nhanh hon phan cap nhat matrix.
- Anh huong:
  - Team de bi lech thong tin khi lap ke hoach tiep theo.
- De xuat:
  - Tach ro 2 phan: "Current verified state" va "Historical baseline"; cap nhat theo moc ngay.

### 9. (Production hardening) Secrets trong appsettings van dang gia tri that

- Muc do: Medium (Accepted exception trong stage hien tai)
- Bang chung:
  - `JobHunter.Api/appsettings.json`
  - `JobHunter.Api/appsettings.Development.json`
- Ghi chu:
  - Theo policy hien tai cua repo, 2 file local appsettings khong duoc danh Critical trong review.
- De xuat:
  - Van nen lap task chuyen sang user-secrets/env vars truoc khi release production.

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
