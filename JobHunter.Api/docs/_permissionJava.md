## Flow phân quyền - 2 lớp độc lập

Request đi qua **2 lớp bảo vệ nối tiếp nhau**, không conflict nhưng có sự không nhất quán cần lưu ý:

### Lớp 1: Spring Security Filter Chain (`SecurityConfiguration`)

```
Request → Spring Security → Kiểm tra JWT/authentication
```

- `allowedPaths` → `permitAll()`: **bypass hoàn toàn** — không cần JWT
- GET `/companies/**`, `/jobs/**`, `/skills/**` → `permitAll()`  
- Còn lại → `.anyRequest().authenticated()` → **phải có JWT hợp lệ**

### Lớp 2: `PermissionInterceptor` (MVC Interceptor)

```
Request (đã qua Filter) → Interceptor → Kiểm tra Role.permissions
```

- `whiteList` trong `PermissionInterceptorConfiguration` → `.excludePathPatterns()`: **bypass interceptor**
- Nếu không trong whitelist → kiểm tra xem `role.getPermissions()` của user hiện tại có chứa `(apiPath, method)` tương ứng không

---

## Vấn đề: Conflict & Không nhất quán

### ❌ Vấn đề 1: Whitelist interceptor rộng hơn nhiều so với Security

| Path | Security (`permitAll`) | Interceptor (`excludePathPatterns`) |
|---|---|---|
| `/auth/**` | ✅ login, refresh, register | ✅ toàn bộ `/auth/**` |
| `/companies/**` | ✅ chỉ GET | ✅ **mọi method** (POST, PUT, DELETE cũng bypass!) |
| `/jobs/**` | ✅ chỉ GET | ✅ **mọi method** bypass |
| `/skills/**` | ✅ chỉ GET | ✅ **mọi method** bypass |
| `/files` | ❌ phải authenticated | ✅ **bypass interceptor** |
| `/resumes/**` | ❌ phải authenticated | ✅ **bypass interceptor** |
| `/subscribers/**` | ❌ phải authenticated | ✅ **bypass interceptor** |

**Kết quả cụ thể:**
- `POST /api/v1/companies` → Lớp 1 chặn (cần JWT) ✅ → Lớp 2 **không check permission** → Bất kỳ authenticated user nào cũng tạo được company!
- `PUT /api/v1/jobs/1` → Lớp 1 chặn (cần JWT) ✅ → Lớp 2 **không check permission** → tương tự

### ❌ Vấn đề 2: `PermissionInterceptor` không check khi `role == null`

```java
// Dòng 55-57
} else {
    throw new PermissionException(...); // OK, chặn đúng
}
```

Nhưng nếu **`user == null`** (dòng 45) → `return true` luôn → **pass qua không check!**

---

## Flow đúng nên là thế nào?

```
Request
  │
  ▼
[Layer 1 - Spring Security Filter]
  ├─ allowedPaths (login, swagger...) → PASS (không cần JWT)
  ├─ permitAll GET companies/jobs/skills → PASS
  └─ anyRequest → CẦN JWT hợp lệ
  │
  ▼ (chỉ khi đã authenticated)
[Layer 2 - PermissionInterceptor]
  ├─ excludePathPatterns (whitelist) → SKIP kiểm tra permission
  └─ còn lại → CHECK role.permissions có khớp (path + method) không?
```

**Whitelist interceptor nên đồng bộ với `permitAll` của Security:**

```java
// Đúng: whitelist interceptor chỉ nên là những path thực sự public
String[] whiteList = {
    "/",
    prefix + "/auth/**",     // public - không cần JWT
    "/storage/**",            // public - static files
    // ❌ Bỏ companies/jobs/skills: chúng GET thì public (Security xử lý)
    //    nhưng POST/PUT/DELETE phải check permission!
};
```

Với GET `/companies` public: vì `permitAll` ở Security nên request **không vào được interceptor check permission** (user không có JWT → không có email → `email.isEmpty()` → `return true`). Tức là public GET vẫn đúng, vô tình "safe" nhưng logic không rõ ràng.

---

## Tóm lại

| | Đúng không? | Ghi chú |
|---|---|---|
| Auth flow (JWT) | ✅ | Bình thường |
| Public GET (companies/jobs/skills) | ✅ | Hoạt động đúng |
| **POST/PUT/DELETE companies/jobs/skills** | ❌ | Bypass interceptor → không check permission |
| `/files`, `/resumes`, `/subscribers` | ⚠️ Partial | Cần JWT nhưng không check permission level |
| `user == null` edge case | ❌ | Nên throw exception thay vì `return true` |

**Rủi ro lớn nhất**: Bất kỳ authenticated user nào (dù role là HR hay USER) đều có thể gọi `POST/PUT/DELETE /companies` và `/jobs` mà không bị chặn ở interceptor.Viewed SecurityConfiguration.java:1-118
Viewed PermissionInterceptor.java:1-64
Viewed PermissionInterceptorConfiguration.java:1-38

Đây là phân tích toàn bộ flow phân quyền, từng lớp một:

---

## Flow phân quyền - 2 lớp độc lập

Request đi qua **2 lớp bảo vệ nối tiếp nhau**, không conflict nhưng có sự không nhất quán cần lưu ý:

### Lớp 1: Spring Security Filter Chain (`SecurityConfiguration`)

```
Request → Spring Security → Kiểm tra JWT/authentication
```

- `allowedPaths` → `permitAll()`: **bypass hoàn toàn** — không cần JWT
- GET `/companies/**`, `/jobs/**`, `/skills/**` → `permitAll()`  
- Còn lại → `.anyRequest().authenticated()` → **phải có JWT hợp lệ**

### Lớp 2: `PermissionInterceptor` (MVC Interceptor)

```
Request (đã qua Filter) → Interceptor → Kiểm tra Role.permissions
```

- `whiteList` trong `PermissionInterceptorConfiguration` → `.excludePathPatterns()`: **bypass interceptor**
- Nếu không trong whitelist → kiểm tra xem `role.getPermissions()` của user hiện tại có chứa `(apiPath, method)` tương ứng không

---

## Vấn đề: Conflict & Không nhất quán

### ❌ Vấn đề 1: Whitelist interceptor rộng hơn nhiều so với Security

| Path | Security (`permitAll`) | Interceptor (`excludePathPatterns`) |
|---|---|---|
| `/auth/**` | ✅ login, refresh, register | ✅ toàn bộ `/auth/**` |
| `/companies/**` | ✅ chỉ GET | ✅ **mọi method** (POST, PUT, DELETE cũng bypass!) |
| `/jobs/**` | ✅ chỉ GET | ✅ **mọi method** bypass |
| `/skills/**` | ✅ chỉ GET | ✅ **mọi method** bypass |
| `/files` | ❌ phải authenticated | ✅ **bypass interceptor** |
| `/resumes/**` | ❌ phải authenticated | ✅ **bypass interceptor** |
| `/subscribers/**` | ❌ phải authenticated | ✅ **bypass interceptor** |

**Kết quả cụ thể:**
- `POST /api/v1/companies` → Lớp 1 chặn (cần JWT) ✅ → Lớp 2 **không check permission** → Bất kỳ authenticated user nào cũng tạo được company!
- `PUT /api/v1/jobs/1` → Lớp 1 chặn (cần JWT) ✅ → Lớp 2 **không check permission** → tương tự

### ❌ Vấn đề 2: `PermissionInterceptor` không check khi `role == null`

```java
// Dòng 55-57
} else {
    throw new PermissionException(...); // OK, chặn đúng
}
```

Nhưng nếu **`user == null`** (dòng 45) → `return true` luôn → **pass qua không check!**

---

## Flow đúng nên là thế nào?

```
Request
  │
  ▼
[Layer 1 - Spring Security Filter]
  ├─ allowedPaths (login, swagger...) → PASS (không cần JWT)
  ├─ permitAll GET companies/jobs/skills → PASS
  └─ anyRequest → CẦN JWT hợp lệ
  │
  ▼ (chỉ khi đã authenticated)
[Layer 2 - PermissionInterceptor]
  ├─ excludePathPatterns (whitelist) → SKIP kiểm tra permission
  └─ còn lại → CHECK role.permissions có khớp (path + method) không?
```

**Whitelist interceptor nên đồng bộ với `permitAll` của Security:**

```java
// Đúng: whitelist interceptor chỉ nên là những path thực sự public
String[] whiteList = {
    "/",
    prefix + "/auth/**",     // public - không cần JWT
    "/storage/**",            // public - static files
    // ❌ Bỏ companies/jobs/skills: chúng GET thì public (Security xử lý)
    //    nhưng POST/PUT/DELETE phải check permission!
};
```

Với GET `/companies` public: vì `permitAll` ở Security nên request **không vào được interceptor check permission** (user không có JWT → không có email → `email.isEmpty()` → `return true`). Tức là public GET vẫn đúng, vô tình "safe" nhưng logic không rõ ràng.

---

## Tóm lại

| | Đúng không? | Ghi chú |
|---|---|---|
| Auth flow (JWT) | ✅ | Bình thường |
| Public GET (companies/jobs/skills) | ✅ | Hoạt động đúng |
| **POST/PUT/DELETE companies/jobs/skills** | ❌ | Bypass interceptor → không check permission |
| `/files`, `/resumes`, `/subscribers` | ⚠️ Partial | Cần JWT nhưng không check permission level |
| `user == null` edge case | ❌ | Nên throw exception thay vì `return true` |

**Rủi ro lớn nhất**: Bất kỳ authenticated user nào (dù role là HR hay USER) đều có thể gọi `POST/PUT/DELETE /companies` và `/jobs` mà không bị chặn ở interceptor.