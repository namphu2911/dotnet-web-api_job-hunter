# Plan: Continue Java to .NET Migration

Date: 2026-03-16
Agent: JobHunter Planner
Status: Draft

## 1. Scope summary

Kế hoạch này ưu tiên các hạng mục còn thiếu theo MIGRATION.md để tiếp tục triển khai dự án theo hướng an toàn hợp đồng API, giảm rủi ro regressions, và tạo nền tảng cho các controller/service còn lại.

Phạm vi đợt kế tiếp gồm:

- Hoàn thiện nền tảng cross-cutting: response envelope, exception mapping, config parity, permission enforcement baseline.
- Migrate theo cụm chức năng nghiệp vụ theo độ phụ thuộc thấp đến cao.
- Bổ sung file/email/scheduler/seeding sau khi các module lõi ổn định.
- Thiết lập test project tối thiểu để chặn regressions trong các đợt migrate tiếp theo.

## 2. Acceptance criteria

- Có chuẩn response envelope thống nhất cho endpoint thành công và lỗi.
- Exception mapping bao phủ tối thiểu: PermissionException (403), StorageException (400), validation errors (400 có field list).
- appsettings có đủ cấu hình nền tảng cho FileStorage và Email, không hardcode secrets.
- Hoàn tất migrate tối thiểu 2 cụm nghiệp vụ đầu tiên (Company, Skill) gồm repository + service + controller + DTO.
- Các endpoint mới/đã migrate được áp authorization phù hợp (Authorize/HasPermission) theo policy hiện có.
- Có test project chạy được để kiểm tra smoke/contract cơ bản cho API.
- dotnet restore và dotnet build chạy thành công ở root solution.

## 3. Implementation steps

1. Cross-cutting foundation (ưu tiên cao nhất)

- Thiết kế ApiResponse<T> tương đương RestResponse của Java.
- Áp cơ chế wrap response (action filter hoặc xử lý thủ công nhất quán tại controller).
- Mở rộng GlobalExceptionMiddleware: map PermissionException, StorageException, validation errors với payload an toàn.
- Bổ sung cấu hình FileStorage và Email trong appsettings/appsettings.Development.
- Bật Swagger PersistAuthorization tương đương Java.

Test impact:

- Thêm test cho shape của success response và error response.
- Thêm test middleware cho từng exception mapping chính.

2. Domain/Infrastructure repository completion (7 repositories còn thiếu)

- Tạo interfaces còn thiếu tại Domain/Repositories.
- Implement EF Core repositories tương ứng tại Infrastructure/Repositories.
- Đăng ký đầy đủ DI trong Infrastructure DependencyInjection.

Test impact:

- Thêm unit test mức repository (nếu có thể với in-memory provider) cho CRUD và các truy vấn đặc thù.
- Nếu chưa có test infra, tạo tối thiểu smoke test wiring DI.

3. Migrate cụm nghiệp vụ đầu tiên: Company + Skill

- Tạo DTO contracts cho Company và Skill.
- Tạo abstractions + services ở Application layer.
- Tạo CompaniesController và SkillsController với CRUD/list theo contract mục tiêu.
- Gắn authorization/permission cho endpoint thay đổi dữ liệu.

Test impact:

- API tests: status code + contract + validation cho create/update/delete/get/list.
- Service tests: happy path, duplicate-name conflict, not-found path.

4. Migrate cụm nghiệp vụ thứ hai: Permission + Role

- Tạo DTO contracts, services, controllers.
- Bảo toàn logic quan hệ role-permissions.
- Chuẩn hóa hành vi conflict/duplicate theo exception mapping mới.

Test impact:

- API tests cho CRUD và gán permissions.
- Kiểm tra forbidden khi thiếu permission.

5. Migrate Job, Resume, Subscriber theo dependency chain

- Job sau Company/Skill.
- Resume sau Job/User.
- Subscriber sau Skill.
- Mỗi cụm hoàn thành đủ repository + service + controller + DTO + auth.

Test impact:

- Kiểm thử liên kết chéo (job-skill-company, resume-user-job, subscriber-skill).
- Kiểm thử filter/list cơ bản theo từng module.

6. Cross-cutting nâng cao sau khi lõi ổn định

- File upload/download service + FilesController + static resource mapping /storage.
- Email service + template + MailController.
- Scheduled email jobs bằng hosted service.
- Database seeder: permissions, SUPER_ADMIN role, admin account; chỉ seed khi DB trống.

Test impact:

- Validation file extension, upload/download flow, not-found file path.
- Email service tests (mock SMTP hoặc abstraction), scheduler trigger logic.
- Seeder idempotency test (chạy nhiều lần không tạo trùng).

7. Filter strategy rollout

- Chốt chiến lược filter cho list endpoints (ưu tiên query params rõ ràng hoặc Sieve).
- Triển khai đồng nhất trước cho Users, Companies, Jobs, Skills, Permissions, Roles, Resumes.

Test impact:

- API tests cho filter hợp lệ/không hợp lệ.
- Kiểm tra phân trang và tổng số bản ghi trả về ổn định.

8. Test foundation

- Tạo project JobHunter.Tests (xUnit).
- Thiết lập smoke test cho app startup và ít nhất 1 test contract cho endpoint hiện có.
- Chuẩn bị cấu trúc test để mở rộng theo từng module migrate.

Test impact:

- Có baseline CI checks cho contract và regressions sớm.

## 4. Risks and mitigations

- API contract drift risk:
  - Rủi ro: Đổi shape response có thể làm vỡ client hiện tại.
  - Giảm thiểu: Chốt một chuẩn ApiResponse và áp dần theo feature flag hoặc rollout theo nhóm endpoint; cập nhật tài liệu contract rõ ràng.

- Migration ordering risk:
  - Rủi ro: Làm controller trước repository/service gây pending dependency.
  - Giảm thiểu: Tuân thủ thứ tự repository -> service -> controller -> test theo từng cụm.

- Authorization regression risk:
  - Rủi ro: Gắn HasPermission thiếu/nhầm làm chặn truy cập hợp lệ.
  - Giảm thiểu: Lập mapping permission-endpoint rõ ràng, thêm test authorized/forbidden cho endpoint quan trọng.

- Data seeding safety risk:
  - Rủi ro: Seed lặp hoặc ghi đè dữ liệu thật.
  - Giảm thiểu: Seed idempotent, chỉ chạy khi bảng trống, log rõ số bản ghi được tạo.

- File and email security risk:
  - Rủi ro: Path traversal, upload file độc hại, lộ thông tin SMTP.
  - Giảm thiểu: Validate folder/fileName whitelist, giới hạn extension/size, cấu hình secrets qua environment.

- Schedule job operational risk:
  - Rủi ro: Job chạy trùng hoặc gây tải cao.
  - Giảm thiểu: Thêm lock logic đơn giản, giới hạn batch, logging và health metrics.

## 5. Validation plan (restore/build/test and targeted checks)

1. Restore and build

- Chạy dotnet restore tại root solution.
- Chạy dotnet build tại root solution.

2. Automated tests

- Không cần chạy test.

3. Targeted checks theo từng cụm

- Envelope and exception:
  - Kiểm tra endpoint thành công trả đúng shape ApiResponse.
  - Kiểm tra 400/403/404/500 có payload lỗi an toàn.
- Authorization:
  - Kiểm tra endpoint có bảo vệ trả 401 khi chưa login và 403 khi thiếu permission.
- CRUD modules:
  - Kiểm tra create/get/update/delete/list với dữ liệu hợp lệ và case lỗi chính.
- File and email:
  - Kiểm tra upload extension không hợp lệ bị từ chối.
  - Kiểm tra download file không tồn tại trả lỗi đúng format.
- Seeder:
  - Khởi động lại ứng dụng nhiều lần, xác nhận không tạo dữ liệu trùng.
