# Ke hoach migrate Java sang .NET cho JobHunter

## 1. Muc tieu

- Migrate day du nhom config, ulti, bien cau hinh trong application.properties, va authen/author tu Java Spring sang ASP.NET Core .NET 8.
- Giu tuong thich hanh vi quan trong (route, flow, cookie refresh token, mapping loi), nhung thay the cac diem khong an toan bang cach lam pho bien trong .NET.
- Trien khai theo kieu production-safe, co test bao phu va co checklist xac nhan truoc khi release.

## 2. Pham vi bat buoc (khong bo sot)

- Config: AppConfig, CorsConfig, CustomAuthenticationEntryPoint, DatabaseInitializer, DateTimeFormatConfiguration, OpenAPIConfig, PermissionInterceptor, PermissionInterceptorConfiguration, SecurityConfiguration, StaticResourcesWebConfiguration, UserDetailsCustom.
- Ulti: FormatRestResponse, SecurityUtil, ApiMessage, GenderEnum, LevelEnum, ResumeStateEnum, GlobalException, IdInvalidException, PermissionException, StorageException.
- Bootstrap: JobhunterApplication.
- Controllers: AuthController, UserController.
- Cau hinh: application.properties.

## 3. Acceptance criteria

1. Bao phu day du toan bo feature trong pham vi tren, khong thieu flow.
2. Co mapping Java -> .NET idiomatic cho tung nhom thanh phan.
3. Route va auth flow tuong thich voi client hien co.
4. Co quyet dinh hardening ve secret, JWT, cookie, CORS, seed admin, va exception payload.
5. Co test strategy unit + integration theo tung nhom hanh vi.

## 4. Feature mapping Java -> .NET

### 4.1 Configuration va startup

- AppConfig (app.version) -> Strongly typed options + route versioning config.
- CorsConfig -> AddCors named policy (origins/methods/headers/credentials/max-age theo moi truong).
- DateTimeFormatConfiguration -> mac dinh ISO 8601 cua .NET + contract tests.
- OpenAPIConfig -> AddSwaggerGen voi Bearer security scheme + metadata/servers trong config.
- StaticResourcesWebConfiguration -> UseStaticFiles + PhysicalFileProvider map /storage.
- DatabaseInitializer -> IHostedService seeding idempotent trong Infrastructure.
- JobhunterApplication (EnableAsync/EnableScheduling) -> giu bootstrap hien tai; neu can scheduler dung BackgroundService/Quartz.

### 4.2 Authen va author

- SecurityConfiguration -> AddAuthentication().AddJwtBearer() + AddAuthorization() + stateless.
- CustomAuthenticationEntryPoint -> JwtBearerEvents.OnChallenge hoac middleware tra 401 payload thong nhat.
- UserDetailsCustom -> AuthService + IUserRepository.GetByEmailAsync cho login validation.
- PermissionInterceptor + PermissionInterceptorConfiguration -> policy-based authorization (Requirement + Handler) theo permission claim/endpoint metadata; giu whitelist endpoint public.
- SecurityUtil -> tach IJwtTokenService + ICurrentUserContext; bo hardcode permissions trong token.

### 4.3 Utilities va error handling

- FormatRestResponse -> result filter/middleware de chuan hoa success envelope.
- ApiMessage annotation -> ApiMessageAttribute + filter de gan message cho response.
- GlobalException + custom exceptions -> mo rong GlobalExceptionMiddleware map 400/401/403/404/409/500 ro rang, khong lo stack trace.
- Enum parse linh hoat (Gender/Level/ResumeState) -> Json converter ho tro string case-insensitive, co the ho tro int cho backward compatibility.

### 4.4 Controllers va contract

- AuthController: migrate day du 6 flow: login, account, refresh, logout, register, change-password.
- UserController: migrate user CRUD + pagination/filter contract.
- Cookie refresh token giu ten refresh_token va flow rotation.

## 5. Mapping bien application.properties

### 5.1 Database

- spring.datasource.url/username/password -> ConnectionStrings + secret store.
- spring.jpa.hibernate.ddl-auto=update -> thay bang EF Core migrations (khong auto update schema production).
- spring.jpa.show-sql -> logging config theo environment.

### 5.2 JWT

- phunn.jwt.base64-secret -> JwtOptions.Secret trong secure source.
- phunn.jwt.access-token-validity-in-seconds -> AccessTokenLifetimeSeconds.
- phunn.jwt.refresh-token-validity-in-seconds -> RefreshTokenLifetimeSeconds.

### 5.3 Upload

- spring.servlet.multipart.max-file-size, max-request-size -> FormOptions/Kestrel limits.
- phunn.upload-file.base-uri -> StorageOptions.BasePath.

### 5.4 Pagination

- spring.data.web.pageable.one-indexed-parameters=true -> bo sung paging binder/mapper de page bat dau tu 1 neu can parity.

### 5.5 App va swagger

- app.version -> AppOptions.Version.
- springdoc.swagger-ui.persist-authorization=true -> Swagger UI config tuong duong.

### 5.6 Mail

- spring.mail.\* -> MailOptions + secure secrets.

## 6. Cac quyet dinh hardening bat buoc

1. Khong hardcode secret DB/JWT/SMTP trong file commit.
2. Khong seed admin voi mat khau tinh trong production.
3. Chot chinh sach cookie refresh: HttpOnly + Secure + SameSite theo mo hinh frontend.
4. CORS theo allowlist theo moi truong; khong mo rong qua muc khi allow credentials.
5. Khong tra chi tiet exception noi bo cho client.
6. Bo hardcode permission trong JWT, cap permission theo role/permission du lieu thuc.

## 7. Ke hoach trien khai theo pha

### Pha 0: Contract freeze

- Chot route/payload auth-user can giu.
- Chot chien luoc response envelope va loi.

### Pha 1: Config foundation

- Tao options classes cho App/Jwt/Cors/Storage/Mail/Pagination.
- Them options validation fail-fast.

### Pha 2: Authentication

- Hoan thien login/account/refresh/logout/register/change-password.
- Chuan hoa tao/validate access-refresh token + refresh rotation.

### Pha 3: Authorization

- Trien khai permission requirement/handler.
- Map endpoint public/protected tuong duong Java.

### Pha 4: Utility + errors

- Them ApiMessageAttribute + response filter.
- Nang cap GlobalExceptionMiddleware map status code nhat quan.

### Pha 5: User behavior parity

- Dong bo user CRUD, validation, conflict, pagination/filter.

### Pha 6: Infra extras

- Seeding permissions/roles/admin idempotent.
- Static files /storage.
- Swagger metadata + bearer auth.

### Pha 7: Verification

- dotnet restore
- dotnet build
- Chay contract tests cho auth/authz/error payload.

## 8. Test strategy

- Unit tests:
  - Jwt token service (create/validate/expiry/claims).
  - Authorization handler theo permission.
  - User/Auth services cho edge cases (duplicate email, wrong password, missing user).
- Integration tests:
  - Auth endpoints: login/account/refresh/logout/register/change-password.
  - 401 vs 403 cho endpoint public/protected.
  - Exception mapping payload.
  - Pagination/filter contract.
  - Seeding idempotent.

## 9. Rui ro va cau hoi can chot truoc khi code

1. Co can tuong thich tuyet doi response wrapper cu hay cho phep transition sang ProblemDetails cho loi?
2. Co can chap nhan token Java cu trong giai doan chuyen tiep?
3. Frontend cung domain hay cross-site de chot SameSite/CORS?
4. Filter users can parity toi dau so voi SpringFilter DSL?
5. Permission claim chot theo key nghiep vu hay method:path?

## 10. Definition of done

- Day du feature trong pham vi migrate da duoc implement.
- Tat ca test pass va khong lo secret.
- Authen/author flow khop contract da chot.
- Da co tai lieu migration + checklist verification de release.
