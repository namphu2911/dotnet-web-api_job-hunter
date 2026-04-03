# Migration Checklist: Java → .NET Web API

Tài liệu này liệt kê trạng thái migration từ dự án Java Spring Boot sang dự án .NET Web API
([dotnet-web-api_job-hunter](https://github.com/namphu2911/dotnet-web-api_job-hunter/tree/develop)).

Cap nhat tien do gan nhat: 2026-04-03

---

## Tổng quan trạng thái

| Thành phần              | Đã migrate | Chưa migrate / Còn gap                                                      |
| ----------------------- | :--------: | --------------------------------------------------------------------------- |
| Domain Entities         |    8/8     | –                                                                           |
| Domain Enums            |    3/3     | –                                                                           |
| Domain Repositories     |    8/8     | –                                                                           |
| Infra Repositories      |    8/8     | –                                                                           |
| Application Services    |   11/11    | Có thêm luồng dispatch email dùng chung (`IJobEmailDispatchService`)        |
| API Controllers         |   12/12    | Đã có đủ nhóm chính + Mail + Home                                           |
| DTO Contracts           |  Partial   | Company, Job, Skill, Permission, Role, Resume, Subscriber, File, Email      |
| Response Envelope       |  ✅ Đã có  | Dùng `ApiResponse<T>` + `ApiResponseEnvelopeFilter`                         |
| Exception Mapping       |  Partial   | Thiếu: PermissionException→403, validation field list, StorageException→400 |
| Filter/Query DSL        |  Partial   | Đã có alias/filter cơ bản, chưa parity hoàn toàn SpringFilter DSL           |
| Static Resource Serving |  ✅ Đã có  | Đã map `/storage` cho uploaded files                                        |
| Config Parity           |  Partial   | Upload base-uri, file size limit, mail SMTP config trong appsettings.json   |
| Database Seeding        |  ✅ Đã có  | Có seed permissions, SUPER_ADMIN, admin user + migrate plaintext password   |
| File Upload/Download    |  ✅ Đã có  | Đã có `FilesController` + `FileService`                                     |
| Email Service           |  ✅ Đã có  | Dùng template `JobHunter.Application/Templates/job.html`                    |
| Scheduled Jobs          |  ✅ Đã có  | Có `EmailJobHostedService` gọi dispatch service dùng chung                  |
| Permission Enforcement  |  Partial   | Đã áp `HasPermission` cho nhiều endpoint, còn cần rà đủ toàn bộ             |
| Test Project            | ❌ Chưa có | Java có `JobhunterApplicationTests.java`                                    |

---

## ✅ Đã migrate

### Domain Layer (`JobHunter.Domain`)

- `Company.cs`, `Job.cs`, `Skill.cs`, `Permission.cs`, `Resume.cs`, `Role.cs`, `Subscriber.cs`, `User.cs` – 8 entities
- `NamedEntityReference.cs` – value object tham chiếu
- `Gender.cs`, `Level.cs`, `ResumeState.cs` – 3 enums

### Infrastructure Layer (`JobHunter.Infrastructure`)

- `JobHunterDbContext.cs` – EF Core DbContext (đủ tất cả DbSet)
- Các repositories đã có implementation đầy đủ: User/Company/Skill/Job/Resume/Permission/Role/Subscriber
- `JwtTokenService.cs` + `JwtOptions.cs` – JWT authentication
- `DependencyInjection.cs` – đăng ký DI
- `DatabaseSeeder.cs` – seed dữ liệu mặc định + hash password
- `EmailJobHostedService.cs` – background job gửi email định kỳ

### Application Layer (`JobHunter.Application`)

- `IAuthService.cs` + `AuthService.cs` – đăng nhập, refresh token, logout, đăng ký, đổi mật khẩu
- `IJwtTokenService.cs` – interface JWT
- `IUserManagementService.cs` + `UserManagementService.cs` – quản lý người dùng (CRUD)
- Đã có các service nghiệp vụ còn lại: Company/Skill/Permission/Role/Job/Resume/Subscriber/File/Email
- Đã bổ sung `IJobEmailDispatchService` + `JobEmailDispatchService` để gom luồng gửi email jobs
- Auth DTOs: `ReqLoginDto`, `ReqChangePasswordDto`, `ResLoginDto`
- User DTOs: `ReqCreateUserDto`, `ReqUpdateUserDto`, `ResUserDto`, `ResCreateUserDto`, `ResUpdateUserDto`, `ResObjectIdNameDto`, `ResultPaginationDto<T>`

### API Layer (`JobHunter.Api`)

- `AuthController.cs` – `POST /auth/login`, `GET /auth/account`, `GET /auth/refresh`, `POST /auth/logout`, `POST /auth/register`, `POST /auth/change-password`
- `UsersController.cs` – `POST /users`, `DELETE /users/{id}`, `GET /users/{id}`, `GET /users`, `PUT /users`
- Đã có đầy đủ controller chính: Companies/Jobs/Skills/Permissions/Roles/Resumes/Subscribers/Files/Mail/Home
- `Authorization/` – `HasPermissionAttribute`, `PermissionAuthorizationHandler`, `PermissionPolicyProvider`, `PermissionRequirement` _(đã áp dụng cho nhiều endpoint)_
- `Middleware/GlobalExceptionMiddleware.cs` – xử lý lỗi toàn cục _(còn thiếu một số loại lỗi)_
- `Program.cs` – CORS, JWT Bearer, Swagger cơ bản đã có

---

## ⏳ Mục tiêu migration ban đầu (đang theo dõi)

Lưu ý: danh sách mục tiêu gốc bên dưới được giữ nguyên để theo dõi lịch sử. Một số mục đã hoàn thành và được cập nhật trạng thái ở phần tổng quan và các mục 6.x.

### 1. API Controllers

#### `HomeController` → `HomeController.cs`

| HTTP Method | Endpoint | Mô tả                                             |
| ----------- | -------- | ------------------------------------------------- |
| GET         | `/`      | Redirect đến `/swagger-ui.html` (entry point app) |

#### `CompanyController` → `CompaniesController.cs`

| HTTP Method | Endpoint                 | Mô tả                                       |
| ----------- | ------------------------ | ------------------------------------------- |
| POST        | `/api/v1/companies`      | Tạo mới công ty                             |
| GET         | `/api/v1/companies`      | Lấy danh sách công ty (phân trang + filter) |
| PUT         | `/api/v1/companies`      | Cập nhật công ty                            |
| DELETE      | `/api/v1/companies/{id}` | Xoá công ty                                 |
| GET         | `/api/v1/companies/{id}` | Lấy công ty theo id                         |

#### `JobController` → `JobsController.cs`

| HTTP Method | Endpoint            | Mô tả                                        |
| ----------- | ------------------- | -------------------------------------------- |
| POST        | `/api/v1/jobs`      | Tạo mới việc làm                             |
| PUT         | `/api/v1/jobs`      | Cập nhật việc làm                            |
| DELETE      | `/api/v1/jobs/{id}` | Xoá việc làm                                 |
| GET         | `/api/v1/jobs/{id}` | Lấy việc làm theo id                         |
| GET         | `/api/v1/jobs`      | Lấy danh sách việc làm (phân trang + filter) |

#### `SkillController` → `SkillsController.cs`

| HTTP Method | Endpoint              | Mô tả                                       |
| ----------- | --------------------- | ------------------------------------------- |
| POST        | `/api/v1/skills`      | Tạo mới kỹ năng                             |
| PUT         | `/api/v1/skills`      | Cập nhật kỹ năng                            |
| DELETE      | `/api/v1/skills/{id}` | Xoá kỹ năng                                 |
| GET         | `/api/v1/skills`      | Lấy danh sách kỹ năng (phân trang + filter) |

#### `PermissionController` → `PermissionsController.cs`

| HTTP Method | Endpoint                   | Mô tả                                         |
| ----------- | -------------------------- | --------------------------------------------- |
| POST        | `/api/v1/permissions`      | Tạo mới quyền hạn                             |
| PUT         | `/api/v1/permissions`      | Cập nhật quyền hạn                            |
| DELETE      | `/api/v1/permissions/{id}` | Xoá quyền hạn                                 |
| GET         | `/api/v1/permissions/{id}` | Lấy quyền hạn theo id                         |
| GET         | `/api/v1/permissions`      | Lấy danh sách quyền hạn (phân trang + filter) |

#### `RoleController` → `RolesController.cs`

| HTTP Method | Endpoint             | Mô tả                                       |
| ----------- | -------------------- | ------------------------------------------- |
| POST        | `/api/v1/roles`      | Tạo mới vai trò                             |
| PUT         | `/api/v1/roles`      | Cập nhật vai trò                            |
| DELETE      | `/api/v1/roles/{id}` | Xoá vai trò                                 |
| GET         | `/api/v1/roles`      | Lấy danh sách vai trò (phân trang + filter) |
| GET         | `/api/v1/roles/{id}` | Lấy vai trò theo id                         |

#### `ResumeController` → `ResumesController.cs`

| HTTP Method | Endpoint                  | Mô tả                                                                      |
| ----------- | ------------------------- | -------------------------------------------------------------------------- |
| POST        | `/api/v1/resumes`         | Tạo mới CV                                                                 |
| PUT         | `/api/v1/resumes`         | Cập nhật trạng thái CV                                                     |
| DELETE      | `/api/v1/resumes/{id}`    | Xoá CV                                                                     |
| GET         | `/api/v1/resumes/{id}`    | Lấy CV theo id                                                             |
| GET         | `/api/v1/resumes`         | Lấy danh sách CV (phân trang + filter, lọc theo công ty của user hiện tại) |
| POST        | `/api/v1/resumes/by-user` | Lấy danh sách CV của user hiện tại                                         |

#### `SubscriberController` → `SubscribersController.cs`

| HTTP Method | Endpoint                     | Mô tả                                        |
| ----------- | ---------------------------- | -------------------------------------------- |
| POST        | `/api/v1/subscribers`        | Tạo mới subscriber                           |
| PUT         | `/api/v1/subscribers`        | Cập nhật skills của subscriber               |
| POST        | `/api/v1/subscribers/skills` | Lấy danh sách skills của subscriber hiện tại |

#### `FileController` → `FilesController.cs`

| HTTP Method | Endpoint        | Mô tả                                     |
| ----------- | --------------- | ----------------------------------------- |
| POST        | `/api/v1/files` | Upload file (pdf, jpg, png, doc, docx)    |
| GET         | `/api/v1/files` | Download file theo `fileName` và `folder` |

#### `MailController` → `MailController.cs`

| HTTP Method | Endpoint        | Mô tả                                      |
| ----------- | --------------- | ------------------------------------------ |
| GET         | `/api/v1/email` | Trigger gửi email việc làm đến subscribers |

---

### 2. Application Layer – Services & Abstractions

| Interface            | Implementation      | Mô tả                                               |
| -------------------- | ------------------- | --------------------------------------------------- |
| `ICompanyService`    | `CompanyService`    | CRUD công ty, lấy danh sách phân trang              |
| `IJobService`        | `JobService`        | CRUD việc làm, kiểm tra skills/company              |
| `ISkillService`      | `SkillService`      | CRUD kỹ năng, kiểm tra trùng tên                    |
| `IPermissionService` | `PermissionService` | CRUD quyền hạn, kiểm tra trùng (module/path/method) |
| `IRoleService`       | `RoleService`       | CRUD vai trò, gán danh sách permissions             |
| `IResumeService`     | `ResumeService`     | CRUD CV, lấy theo user, lọc theo công ty            |
| `ISubscriberService` | `SubscriberService` | CRUD subscriber, gán skills, gửi email việc làm     |
| `IFileService`       | `FileService`       | Lưu trữ file lên local filesystem, download file    |
| `IEmailService`      | `EmailService`      | Gửi email đơn giản + email HTML từ template         |

---

### 3. Domain Layer – Repository Interfaces

Cần tạo trong `JobHunter.Domain/Repositories/`:

| Interface               | Mô tả                                               |
| ----------------------- | --------------------------------------------------- |
| `ICompanyRepository`    | Thêm, sửa, xoá, lấy công ty; tìm users theo công ty |
| `IJobRepository`        | Thêm, sửa, xoá, lấy việc làm; tìm theo skills       |
| `ISkillRepository`      | Thêm, sửa, xoá, lấy kỹ năng; kiểm tra tên tồn tại   |
| `IPermissionRepository` | Thêm, sửa, xoá, lấy quyền hạn; tìm theo IDs         |
| `IRoleRepository`       | Thêm, sửa, xoá, lấy vai trò; tìm theo tên           |
| `IResumeRepository`     | Thêm, sửa, xoá, lấy CV; lọc theo email / job        |
| `ISubscriberRepository` | Thêm, sửa, xoá, lấy subscriber; tìm theo email      |

---

### 4. Infrastructure Layer – Repository Implementations

Cần tạo trong `JobHunter.Infrastructure/Repositories/`:

| Class                  | Mô tả                                              |
| ---------------------- | -------------------------------------------------- |
| `CompanyRepository`    | EF Core implementation của `ICompanyRepository`    |
| `JobRepository`        | EF Core implementation của `IJobRepository`        |
| `SkillRepository`      | EF Core implementation của `ISkillRepository`      |
| `PermissionRepository` | EF Core implementation của `IPermissionRepository` |
| `RoleRepository`       | EF Core implementation của `IRoleRepository`       |
| `ResumeRepository`     | EF Core implementation của `IResumeRepository`     |
| `SubscriberRepository` | EF Core implementation của `ISubscriberRepository` |

---

### 5. DTO Contracts

Cần tạo trong `JobHunter.Application/Contracts/`:

#### Companies/

- `ReqCreateCompanyDto` – name, description, address, logo
- `ReqUpdateCompanyDto` – id, name, description, address, logo

#### Jobs/

- `ReqCreateJobDto` – name, location, salary, quantity, level, description, startDate, endDate, active, skills (list id), company (id)
- `ReqUpdateJobDto` – id + các field như create
- `ResJobDto` – id, name, location, salary, quantity, level, description, startDate, endDate, active, createdAt, createdBy, updatedAt, updatedBy, skills (list name)

#### Skills/

- `ReqCreateSkillDto` – name
- `ReqUpdateSkillDto` – id, name

#### Permissions/

- `ReqCreatePermissionDto` – name, apiPath, method, module
- `ReqUpdatePermissionDto` – id, name, apiPath, method, module

#### Roles/

- `ReqCreateRoleDto` – name, description, active, permissions (list id)
- `ReqUpdateRoleDto` – id, name, description, active, permissions (list id)

#### Resumes/

- `ReqCreateResumeDto` – email, url, status, user (id), job (id)
- `ReqUpdateResumeDto` – id, status
- `ResCreateResumeDto` – id, createdAt, createdBy
- `ResResumeDto` – id, email, url, status, companyName, user (id+name), job (id+name), createdAt, createdBy, updatedAt, updatedBy
- `ResUpdateResumeDto` – updatedAt, updatedBy

#### Subscribers/

- `ReqCreateSubscriberDto` – email, name, skills (list id)
- `ReqUpdateSubscriberDto` – id, skills (list id)

#### Files/

- `ResUploadFileDto` – fileName, uploadedAt

#### Emails/

- `ResEmailJobDto` – name, salary, company (name), skills (list name)

---

### 6. Tính năng Cross-cutting chưa migrate

#### 6.1 Response Envelope (`FormatRestResponse.java`, `RestResponse.java`, `@ApiMessage`)

**Java pattern:**

- Mọi response thành công đều được bọc trong `RestResponse<T>`:
  ```json
  {
    "statusCode": 200,
    "message": "Fetch companies",
    "error": null,
    "data": { ... }
  }
  ```
- `FormatRestResponse` implement `ResponseBodyAdvice` – tự động wrap toàn bộ response.
- `@ApiMessage("Fetch companies")` annotation trên method controller điều khiển nội dung trường `message`.
- Response lỗi cũng dùng cùng envelope với `data: null` và `error` mô tả loại lỗi.

**Trạng thái .NET:** Không có wrapper nào – controller trả thẳng object hoặc `ActionResult<T>`.

**Cần làm trong .NET:**

- Tạo `ApiResponse<T>` record/class tương đương `RestResponse<T>` (statusCode, error, message, data).
- Implement `IActionFilter` hoặc dùng custom `OutputFormatter` để tự động wrap response.
- Hoặc trả `ApiResponse<T>` thủ công trong từng controller action nếu muốn đơn giản hơn.

#### 6.2 Exception Mapping chi tiết (`GlobalException.java` vs `GlobalExceptionMiddleware.cs`)

**Java** xử lý từng loại lỗi riêng biệt và trả `RestResponse<T>`:

| Exception Java                    | HTTP Status | Trường `error`             |
| --------------------------------- | :---------: | -------------------------- |
| `Exception` (generic)             |     500     | "Internal server error..." |
| `UsernameNotFoundException`       |     400     | "Exception occurs..."      |
| `BadCredentialsException`         |     400     | "Exception occurs..."      |
| `IdInvalidException`              |     400     | "Exception occurs..."      |
| `NoResourceFoundException`        |     404     | "404 Not Found..."         |
| `MethodArgumentNotValidException` |     400     | field-level error list     |
| `StorageException`                |     400     | "Exception upload file..." |
| `PermissionException`             |   **403**   | "Permission denied..."     |

**Trạng thái .NET `GlobalExceptionMiddleware.cs`:**

| Exception .NET                | HTTP Status | Gap so với Java                       |
| ----------------------------- | :---------: | ------------------------------------- |
| `DbException`                 |     503     | –                                     |
| `UnauthorizedAccessException` |     401     | –                                     |
| `InvalidOperationException`   |     409     | –                                     |
| `KeyNotFoundException`        |     404     | –                                     |
| `_` (generic)                 |     500     | –                                     |
| _(thiếu)_ StorageException    |      –      | ❌ chưa map                           |
| _(thiếu)_ PermissionException |      –      | ❌ chưa map → cần 403                 |
| _(thiếu)_ Validation errors   |      –      | ❌ chưa trả field error list          |
| Response envelope             |      –      | ❌ chưa dùng `RestResponse<T>` format |

**Cần làm trong .NET:**

- Thêm case `PermissionException` → 403 trong `GlobalExceptionMiddleware`.
- Thêm case `StorageException` → 400.
- Xử lý `ValidationException` (FluentValidation) hoặc `BadHttpRequestException` → 400 với danh sách lỗi field.
- Đồng bộ format envelope với `RestResponse<T>` nếu áp dụng mục 6.1.

#### 6.3 Database Seeding (`DatabaseInitializer.java`)

**Java:** `CommandLineRunner` tự động tạo dữ liệu mẫu khi khởi động:

- 34 permissions mặc định (COMPANIES, JOBS, PERMISSIONS, RESUMES, ROLES, USERS, SUBSCRIBERS, FILES)
- Role `SUPER_ADMIN` với toàn bộ permissions
- Admin user: `admin@gmail.com` / `123456`

**Trạng thái .NET hiện tại:**

- ✅ Đã có `DatabaseSeeder` (IHostedService)
- ✅ Đã seed permissions, role `SUPER_ADMIN`, admin user
- ✅ Có thêm migration dữ liệu password plaintext cũ sang BCrypt

#### 6.4 File Upload/Download (`FileService.java`, `FileController.java`)

**Java:** Lưu file lên local filesystem theo `baseURI + folder + "/" + finalName`. Download trả về stream.

**Trạng thái .NET hiện tại:**

- ✅ Đã có `IFileService` + `FileService`
- ✅ Đã có `FilesController` cho upload/download
- ✅ Đã dùng static file mapping `/storage` để truy cập file upload

#### 6.5 Static Resources Mapping (`StaticResourcesWebConfiguration.java`)

**Java:** Map `/storage/**` → local filesystem để client có thể truy cập trực tiếp file đã upload.

```java
registry.addResourceHandler("/storage/**").addResourceLocations(baseURI);
```

**Trạng thái .NET hiện tại:**

- ✅ Đã map static resources qua `/storage` trong `Program.cs`

#### 6.6 Email Service (`EmailService.java`, `MailController.java`)

**Java:** Spring Mail + Thymeleaf template `job.html` gửi email HTML cho subscribers.

**Trạng thái .NET hiện tại:**

- ✅ Dùng `System.Net.Mail`
- ✅ Đã có `IEmailService` + `EmailService`
- ✅ Đã render template từ file trong dự án .NET: `JobHunter.Application/Templates/job.html`
- ✅ `MailController` trigger gửi email đã dùng chung service dispatch

#### 6.7 Scheduled Email Jobs

**Java:** `@Scheduled(cron = "*/30 * * * * *")` (đang comment out) trên method trong `MailController`.

**Trạng thái .NET hiện tại:**

- ✅ Đã có `EmailJobHostedService` chạy theo chu kỳ
- ✅ Đã dùng luồng dispatch chung (`IJobEmailDispatchService`) với `MailController`

#### 6.8 Config Parity (`application.properties` → `appsettings.json`)

| Cấu hình Java                                | Giá trị Java     | Tương đương .NET (appsettings.json)      |
| -------------------------------------------- | ---------------- | ---------------------------------------- |
| `spring.servlet.multipart.max-file-size`     | `50MB`           | `IFormFile` validation / Kestrel limits  |
| `phunn.upload-file.base-uri`                 | `file:///D:/...` | `FileStorage:BasePath` trong appsettings |
| `spring.mail.host`                           | `smtp.gmail.com` | `Email:Host`                             |
| `spring.mail.port`                           | `587`            | `Email:Port`                             |
| `spring.mail.username`                       | _(env var)_      | `Email:Username`                         |
| `spring.mail.password`                       | _(env var)_      | `Email:Password`                         |
| `springdoc.swagger-ui.persist-authorization` | `true`           | Swagger `PersistAuthorization = true`    |

**Trạng thái .NET:** Đã có các section `FileStorage` và `Email`, vẫn cần tiếp tục hardening cấu hình cho production.

#### 6.9 Permission Enforcement (`PermissionInterceptor.java`)

**Java:** Mỗi request đến endpoint có bảo vệ đều qua `PermissionInterceptor.preHandle()`, kiểm tra user có permission tương ứng (theo `apiPath` + `method`).

**Trạng thái .NET:** Có khung (`HasPermissionAttribute`, `PermissionAuthorizationHandler`) và đã gắn `[HasPermission]` cho nhiều endpoint quản trị.

**Cần làm:**

- Gắn `[Authorize]` hoặc `[HasPermission("...")]` attribute vào tất cả endpoint cần bảo vệ của các controllers mới.
- Hoặc áp dụng global authorization policy trong `Program.cs` để tất cả endpoint đều yêu cầu authentication.

#### 6.10 Filter / Query DSL cho list endpoints (`SpringFilter` → custom query params)

**Java:** Tất cả list endpoint đều hỗ trợ filter kiểu SpringFilter DSL qua query param `filter`:

```
GET /api/v1/jobs?filter=name~'java'&page=1&size=10
GET /api/v1/companies?filter=name~'cty'
GET /api/v1/resumes?filter=status~'PENDING'
```

**Trạng thái .NET:** Đã hỗ trợ query phân trang/alias và filter cơ bản ở nhiều endpoint; chưa parity 100% SpringFilter DSL nâng cao.

**Cần làm trong .NET:**

- Chọn một trong các hướng:
  1. Thêm query parameter `filter` và parse theo cú pháp đơn giản.
  2. Dùng thư viện `Sieve` hoặc `OData` cho phép filter/sort linh hoạt.
  3. Định nghĩa query params cụ thể theo từng entity (ví dụ `?name=java&level=SENIOR`).
- Áp dụng cho tất cả list endpoints: `/users`, `/companies`, `/jobs`, `/skills`, `/permissions`, `/roles`, `/resumes`.

#### 6.11 Test Project

**Java:** Có `JobhunterApplicationTests.java` – context load test cơ bản với `@SpringBootTest`.

**Trạng thái .NET:** Không có test project nào.

**Cần làm trong .NET:**

- Tạo project `JobHunter.Tests` (xUnit hoặc NUnit)
- Thêm context load / smoke test tương đương
- Khi migrate từng service, thêm unit test tương ứng

---

## ⚠️ Migrate một phần nhưng còn gap quan trọng

| Thành phần                  | Đã có trong .NET                                                                      | Gap còn lại                                                                                        |
| --------------------------- | ------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| List endpoints              | Có pagination + alias + filter cơ bản ở nhiều module                                  | Cần parity sâu hơn với SpringFilter DSL nâng cao                                                   |
| `GlobalExceptionMiddleware` | Xử lý 5 loại exception cơ bản                                                         | Thiếu PermissionException→403, StorageException→400, validation field list                         |
| Permission/AuthZ framework  | `HasPermissionAttribute`, `PermissionAuthorizationHandler` + đã áp nhiều endpoint     | Cần rà phủ đầy đủ các endpoint còn lại                                                             |
| CORS                        | Origins, methods, headers (`x-no-retry` included), credentials, preflight TTL đã đúng | Không còn gap – đã parity với Java `CorsConfig.java`                                               |
| Swagger                     | Basic bearer auth scheme                                                              | Thiếu `PersistAuthorization = true` (`springdoc.swagger-ui.persist-authorization=true` trong Java) |

---

## Thứ tự migration đề xuất

1. **Response Envelope** – tạo `ApiResponse<T>` và cơ chế wrap response (ưu tiên cao vì ảnh hưởng toàn bộ API)
2. **Exception Mapping** – bổ sung `PermissionException`, `StorageException`, validation errors vào `GlobalExceptionMiddleware`
3. **Config** – thêm `FileStorage` và `Email` sections vào `appsettings.json`
4. **Domain Repositories** – tạo 7 interface còn lại
5. **Infrastructure Repositories** – implement 7 repository còn lại
6. **DTOs** – tạo contracts cho Company, Job, Skill, Permission, Role, Resume, Subscriber
7. **Company & Skill** – migrate service + controller (đơn giản nhất, ít phụ thuộc)
8. **Permission & Role** – migrate service + controller (phụ thuộc lẫn nhau)
9. **Job** – migrate service + controller (phụ thuộc Company và Skill)
10. **Resume** – migrate service + controller (phụ thuộc Job và User)
11. **Subscriber** – migrate service + controller (phụ thuộc Skill)
12. **HomeController** – redirect `/` → `/swagger-ui.html`
13. **Filter/Query DSL** – thêm filter params vào tất cả list endpoints
14. **File Upload/Download** – `FilesController` + `FileService` + static resource mapping
15. **Email Service** – `EmailService` + HTML template + `MailController`
16. **Database Seeding** – seed 34 permissions, SUPER_ADMIN role, admin user
17. **Scheduled Jobs** – background service gửi email định kỳ
18. **Permission Enforcement** – gắn `[HasPermission]` / `[Authorize]` vào tất cả endpoints mới
19. **Test Project** – tạo `JobHunter.Tests` với smoke tests cơ bản
