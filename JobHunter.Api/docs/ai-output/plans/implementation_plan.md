# Analysis: Java vs .NET Gap – Unfinished Items Scan

Đây là kết quả đối chiếu 3 vấn đề trong review `2026-04-03-dotnet-unfinished-items-scan.md`,
so sánh cách Java đang làm và cách .NET đang làm, từ đó xác định cần sửa gì.

---

## Vấn đề 1: Permission enforcement chưa phủ đều

### Java làm gì?

Java dùng **global interceptor** (`PermissionInterceptor.java`) thay vì annotation từng endpoint.

**Cơ chế:**
1. `PermissionInterceptor.preHandle()` chạy **trước mọi request authenticated**.
2. Lấy email từ JWT → load `User` → lấy `Role.permissions`.
3. So sánh `(apiPath, method)` của request với list permission của user.
4. Nếu không match → throw `PermissionException` (403).

**Whitelist** (trong `PermissionInterceptorConfiguration.java`) là các path được **exempt khỏi interceptor**:
```
/auth/**
/storage/**
/companies/**        ← toàn bộ CRUD companies
/jobs/**             ← toàn bộ CRUD jobs
/skills/**           ← toàn bộ CRUD skills
/files               ← upload/download file
/resumes/**
/subscribers/**
```

**Java SecurityConfig whitelist** (permit all, không cần JWT):
```
GET /companies/**    ← read-only public
GET /jobs/**
GET /skills/**
/email/**            ← mail trigger (public)
```

> **Kết luận Java**: Interceptor check permission được áp cho **tất cả** endpoint đã authen,
> nhưng có whitelist rộng để bypass interceptor cho một số path.
> Thực tế Companies/Skills/Files/Resumes/Subscribers hoàn toàn **không bị check permission**.

---

### .NET đang làm gì?

.NET dùng `[HasPermission(apiPath, method, module)]` attribute gắn từng action method.
- Đã áp cho: `Users`, `Roles`, `Permissions`, `Jobs`, `Resumes`, `Subscribers`.
- **Chưa áp** cho: `Companies`, `Skills`, `Files`, `Mail`.
- Companies/Skills POST/PUT/DELETE chỉ có `[Authorize]` (xác thực JWT nhưng không check permission key).

---

### Gap phân tích

| Endpoint | Java interceptor? | Java SecurityConfig | .NET hiện tại |
|---|---|---|---|
| `GET /companies/**` | ❌ Whitelist exempt | `permitAll` (public) | Không có Authorize (public) ✅ |
| `POST/PUT/DELETE /companies/**` | ❌ Whitelist exempt | Authenticated only | `[Authorize]` – KHÔNG check perm ⚠️ |
| `GET /skills/**` | ❌ Whitelist exempt | `permitAll` (public) | Không có Authorize (public) ✅ |
| `POST/PUT/DELETE /skills/**` | ❌ Whitelist exempt | Authenticated only | `[Authorize]` – KHÔNG check perm ⚠️ |
| `POST /files` | ❌ Whitelist exempt | Authenticated only | `[Authorize]` – KHÔNG check perm ✅ parity |
| `GET /files` | ❌ Whitelist exempt | Authenticated only | `[Authorize]` – KHÔNG check perm ✅ parity |
| `GET /email` | ❌ Whitelist permit | `permitAll` (public) | `[Authorize]` ❌ KHÁC Java |
| `GET/POST/PUT/DELETE /users` | ✅ Interceptor check | Authenticated | `[HasPermission]` ✅ |

> [!IMPORTANT]
> **Thực tế Java**: Companies, Skills, Files, Resumes, Subscribers đều **không bị permission check** theo interceptor (whitelist toàn bộ).
> Chỉ Users, Roles, Permissions mới thực sự bị permission check.
> Mail (`/email`) trong Java là **public** (no auth), .NET dùng `[Authorize]` – đây là sự khác biệt.

---

### Cần sửa để parity với Java

**Option A (Parity chính xác với Java):** Companies/Skills/Files đang whitelist ở Java → .NET giữ `[Authorize]` là đủ, KHÔNG cần `[HasPermission]`. Mail cần đổi thành public.

**Option B (Upgrade security hơn Java):** Áp `[HasPermission]` đầy đủ cho tất cả write operations.

**Đề xuất theo yêu cầu (parity Java):**

#### 1.1 Mail: đổi `[Authorize]` → `[AllowAnonymous]`
Java `PermissionInterceptorConfig` whitelist `/email/**` và SecurityConfig cũng `permitAll` `/email/**`.
.NET `MailController` đang dùng `[Authorize]` → sai so với Java.

#### 1.2 Companies/Skills: POST/PUT/DELETE giữ `[Authorize]` (đúng parity Java)
Java interceptor whitelist toàn bộ `/companies/**` và `/skills/**`, chỉ cần authenticated.

#### 1.3 Files: `[Authorize]` là đúng (cả Java cũng chỉ cần authenticated, interceptor exempt)

Nếu muốn **nâng cấp cả hai** (vượt qua Java): thêm `[HasPermission]` cho Companies/Skills/Mail write ops.

---

## Vấn đề 2: File upload hardening chưa đầy đủ

### Java làm gì?

`FileController.java`:
- Validate file null/empty → throw `StorageException`
- Validate extension allowlist: `pdf, jpg, jpeg, png, doc, docx`
- **Không validate max file size** trong code Java (không có max size check)
- Java dùng `@Value("${phunn.upload-file.base-uri}")` → baseURI là dạng `file:///...` (URI-based)
- Store: `Files.copy(inputStream, Paths.get(URI))` – dùng `java.nio.Path` với URI scheme

`FileService.java` store method:
- Build path bằng `URI` constructor từ string `baseURI + folder + "/" + finalName`
- **Không validate folder**: nếu `folder = "../../etc"` thì path traversal thành công ⚠️

---

### .NET đang làm gì?

`FileService.cs`:
- `Path.Combine(_baseUri, folder, uniqueName)` → **không validate folder**
- Config có `FileStorage:MaxFileSizeMb = 50` nhưng **không đọc giá trị này trong code**
- Không kiểm tra path traversal (`..` segments)

`FilesController.cs`:
- Validate null/empty ✅
- Validate extension ✅  
- **Không check max file size** ⚠️

---

### Gap phân tích

| Hardening | Java | .NET hiện tại |
|---|---|---|
| Null/empty check | ✅ | ✅ |
| Extension allowlist | ✅ | ✅ |
| Max file size | ❌ (không có) | Config có, code chưa đọc ⚠️ |
| Path traversal prevention | ❌ (không có) | ❌ (không có) ⚠️ |
| Folder allowlist | ❌ (không có) | ❌ (không có) ⚠️ |

**Java cũng chưa có hardening path traversal** – nhưng .NET có cơ hội làm tốt hơn.

---

### Cần sửa

**2.1 Enforce max file size** từ config `FileStorage:MaxFileSizeMb` trong `FileService.cs` hoặc `FilesController.cs`:
```csharp
// FileService.cs constructor
var maxMb = configuration.GetValue<int>("FileStorage:MaxFileSizeMb", 50);
_maxFileSizeBytes = maxMb * 1024L * 1024L;

// UploadAsync - kiểm tra stream length
if (fileStream.Length > _maxFileSizeBytes)
    throw new StorageException($"File size exceeds maximum allowed {maxMb}MB.");
```

**2.2 Validate và sanitize folder** trong `FileService.cs`:
- Định nghĩa allowlist: `["company", "resume", "avatar", "job"]`
- Normalize path, block `..` segments
```csharp
private static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
    { "company", "resume", "avatar", "job" };

private static string ResolveAndValidateFolder(string basePath, string folder)
{
    if (!AllowedFolders.Contains(folder))
        throw new StorageException($"Folder '{folder}' is not allowed.");
    var fullPath = Path.GetFullPath(Path.Combine(basePath, folder));
    if (!fullPath.StartsWith(Path.GetFullPath(basePath), StringComparison.OrdinalIgnoreCase))
        throw new StorageException("Path traversal detected.");
    return fullPath;
}
```

---

## Vấn đề 3: Filter DSL chưa parity SpringFilter nâng cao

### Java làm gì?

Java dùng thư viện `com.turkraft.springfilter` – một DSL đầy đủ:
- `name ~ 'value'` → LIKE contains
- `name = 'value'` → exact match
- `field in ('a','b')` → IN clause
- `and`, `or`, `not` logic groups
- Nested expressions, numeric comparisons
- Auto-binding với JPA `Specification<T>`

**Thực tế FE đang dùng**: Chủ yếu `name ~ 'value'` (contains) theo xem `src/config/api.ts`.

### .NET đang làm gì?

`SpringFilterQuery.cs` parse regex thủ công:
- `GetContainsValues(filter, field)` → parse `field ~ 'value'` ✅
- `GetEqualsValues(filter, field)` → parse `field = 'value'` ✅
- `GetInValues(filter, field)` → parse `field in (...)` ✅
- Sort parsing ✅
- **Chưa có**: AND/OR logic groups, NOT, numeric comparisons, nested

### Gap phân tích

| Feature | Java (SpringFilter) | .NET hiện tại |
|---|---|---|
| `field ~ 'value'` | ✅ | ✅ |
| `field = 'value'` | ✅ | ✅ |
| `field in (...)` | ✅ | ✅ |
| AND / OR logic | ✅ | ❌ |
| NOT | ✅ | ❌ |
| `>`, `<`, `>=`, `<=` | ✅ | ❌ |
| Nested groups | ✅ | ❌ |

---

### Cần sửa

Đây là quyết định scope: giữ minimal hay nâng cấp DSL?

**Option A (Giữ minimal - khuyến nghị hiện tại):** Nếu FE chỉ dùng `~`, `=`, `in` → giữ nguyên, chấp nhận gap.

**Option B (Nâng cấp full DSL):** Viết parser thực sự cho SpringFilter DSL 
(phức tạp, cần parser recursion-descent hoặc dùng ANTLR).

**Kết luận**: Kiểm tra `src/config/api.ts` để xác định FE thực sự dùng operator gì.

---

## Tóm tắt các thay đổi cần làm (ưu tiên)

| # | Vấn đề | File cần sửa | Mức độ | Effort |
|---|---|---|---|---|
| 1 | Mail: đổi `[Authorize]` → `[AllowAnonymous]` | `MailController.cs` | High | S |
| 2 | Companies POST/PUT/DELETE: xem xét nâng lên `[HasPermission]` (nếu muốn vượt Java) | `CompaniesController.cs` | Medium | S |  
| 3 | Skills POST/PUT/DELETE: tương tự Companies | `SkillsController.cs` | Medium | S |
| 4 | FileService: enforce MaxFileSizeMb từ config | `FileService.cs` | Medium | S |
| 5 | FileService: validate folder allowlist + path traversal | `FileService.cs` | Medium | M |
| 6 | SprintFilterQuery: AND/OR nếu FE cần | `SpringFilterQuery.cs` | Low | L |

---

## Open Questions

> [!IMPORTANT]
> **Q1 (cần xác nhận):** Với Companies/Skills write ops – muốn **parity Java** (chỉ `[Authorize]`) hay **nâng cấp** (thêm `[HasPermission]`)?
> Java hiện tại whitelist toàn bộ `/companies/**` và `/skills/**` khỏi permission interceptor,
> nên về parity thì `[Authorize]` là đủ. Nâng lên `[HasPermission]` sẽ chặt hơn Java.

---

## Đã xác nhận từ FE source

> [!NOTE]
> **Q2 – Folder allowlist (đã xác nhận):** FE truyền 3 giá trị `folder`:
> - `"resume"` – từ `apply.modal.tsx`
> - `"avatar"` – từ `manage.account.tsx`
> - `"company"` – từ `modal.company.tsx`
>
> → Allowlist chính xác: `{ "resume", "avatar", "company" }`

> [!NOTE]
> **Q3 – SpringFilter DSL (đã xác nhận):** FE chỉ dùng `filter=name~'value'` (contains pattern).
> Thấy trong `modal.user.tsx`, `upsert.job.tsx`, `job.card.tsx` – không có `AND/OR`, `NOT`, hay numeric.
>
> → Không cần nâng cấp SpringFilter DSL. Parser hiện tại đủ dùng cho FE.
