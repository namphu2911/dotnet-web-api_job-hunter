# BE Alignment Plan (Keep Existing FE Unchanged)

## 1) Muc tieu

- Dieu chinh API .NET hien tai de tuong thich voi FE contract dang duoc su dung.
- Khong thay doi route, payload, response ma FE dang goi.
- Giu thay doi toi thieu, an toan production, uu tien backward-compatible.

## 2) Scope can chinh sua tren BE

### 2.1 Response envelope va pagination

- Bo sung contract pagination theo FE:
	- Tu dang `Page/PageSize/Total/Items` sang dang FE ky vong: `meta: { page, pageSize, pages, total }, result: []`.
	- Co the giu dong thoi 2 dang trong giai doan chuyen doi (de tranh pha vo client khac).
- Dieu chinh envelope de FE doc on dinh:
	- `message` can chap nhan string thong thuong.
	- Van ho tro loi validation (mang object) nhung can dam bao FE khong vo do parse.

### 2.2 Module Companies/Skills/Permissions/Roles

- GET list cua cac module nay hien tra ve danh sach thuong.
- FE dang goi theo query string va ky vong du lieu phan trang.
- Ke hoach BE:
	- Ho tro query params `current`, `pageSize`, `qs` (hoac alias tuong ung).
	- Tra ve data dang phan trang theo FE shape.
	- Van cho phep goi cu khong query (fallback page=1, pageSize mac dinh).

### 2.3 Module Subscribers

- FE can cac API:
	- GET `/api/v1/subscribers?{query}`
	- GET `/api/v1/subscribers/{id}`
	- DELETE `/api/v1/subscribers/{id}`
	- POST `/api/v1/subscribers/skills`
- BE hien tai chua expose GET by id/list va DELETE route tuong ung.
- Ke hoach BE:
	- Them endpoint GET list co pagination contract FE.
	- Them endpoint GET by id.
	- Them endpoint DELETE by id.
	- Giu endpoint POST `skills` nhu hien tai.

### 2.4 Module Resumes

- FE gui create payload dang:
	- `user: { id }`, `job: { id }`, `status`, `email`, `url`.
- BE hien dang nhan `User` va `Job` la `long` truc tiep.
- Ke hoach BE:
	- Mo rong `ReqCreateResumeDto` de chap nhan ca 2 format:
		- Format hien tai (`user`, `job` la so).
		- Format FE (`user.id`, `job.id`).
	- Mapping ve domain id chung trong service layer.

### 2.5 Module Jobs

- FE gui:
	- `skills` la mang object skill hoac id.
	- `company` la object `{ id, name }` hoac id.
- BE hien yeu cau `skills: long[]`, `company: long`.
- Ke hoach BE:
	- Mo rong create/update request de chap nhan union payload:
		- `skills`: long[] hoac object[] (co field id).
		- `company`: long hoac object co id.
	- Response de FE de dung:
		- Xem xet bo sung thong tin company trong job response neu FE su dung.

### 2.6 Module Roles va Subscribers input

- FE co xu huong gui `permissions` va `skills` theo string[]/object[] o mot so luong.
- BE yeu cau long[].
- Ke hoach BE:
	- Cho phep parse da dang input (string number, object co id).
	- Chuan hoa ve long[] truoc khi xu ly service.

### 2.7 Auth account shape

- FE mong doi `user.role.permissions[]` trong account/login object.
- BE hien role trong login/account chi co id + name.
- Ke hoach BE:
	- Bo sung permissions trong role response cua `login`, `account`, `refresh`.
	- Dam bao khong lo thong tin nhay cam.

### 2.8 File upload response

- FE hien doc `fileName`.
- BE tra `fileName` + `uploadedAt`.
- Khong bat buoc doi, nhung ke hoach:
	- Giu nguyen output hien tai (khong break FE).

## 3) Trinh tu trien khai de giam rui ro

1. Tao adapter contracts moi (request/response DTO cho FE shape).
2. Cap nhat mapping tai controller/service, uu tien non-breaking.
3. Bo sung endpoint thieu cho Subscribers.
4. Chinh pagination cho cac module list.
5. Mo rong Auth response (role.permissions).
6. Regression test toan bo route FE dang dung.

## 4) Chi tiet implementation theo tang

### 4.1 Contract layer (Application.Contracts)

- Them cac DTO request linh hoat (union-like) cho Jobs/Resumes/Roles/Subscribers.
- Them DTO pagination theo FE (`meta/result`).
- Khong xoa DTO cu de tranh pha vo call noi bo.

### 4.2 Controller layer (Api/Controllers)

- Companies/Skills/Permissions/Roles:
	- Nang cap GET list co query + pagination response FE.
- Subscribers:
	- Them GET list, GET by id, DELETE by id.
- Jobs/Resumes:
	- Nhan payload da dang va mapping ve request noi bo.
- Auth:
	- Tra role kem permissions.

### 4.3 Service layer (Application/Services)

- Them helper parse id linh hoat (long/string/object id).
- Validate input va tra loi ro rang khi payload sai shape.
- Khong dung null-forgiving, tuan thu nullable.

## 5) Acceptance criteria

- FE giu nguyen code van goi duoc tat ca API hien co.
- Khong con 4xx do sai shape payload o Jobs/Resumes/Roles/Subscribers.
- Companies/Skills/Permissions/Roles tra duoc danh sach dang FE pagination.
- Subscribers co du CRUD ma FE dang goi.
- Auth account/login/refresh tra role.permissions theo ky vong FE.
- Build xanh va khong them warning nghiem trong.

## 6) Test strategy

- Unit tests:
	- Mapping/parsing payload da dang (id tu number/string/object).
	- Pagination mapper meta/result.
- Integration/API tests:
	- Happy path cho tung endpoint FE su dung.
	- 1-2 failure path cho payload invalid.
	- Xac minh envelope response du key (`statusCode`, `message`, `data`).

## 7) Rui ro va giam thieu

- Rui ro 1: Break client khac dang dung shape cu.
	- Giam thieu: support song song shape cu + moi trong 1-2 release.
- Rui ro 2: Parsing input da dang tang do phuc tap.
	- Giam thieu: tach helper parse rieng, test bao phu.
- Rui ro 3: Performance khi them permissions vao auth response.
	- Giam thieu: toi uu query include va cache claims neu can.

## 8) Verification gates

Chay tai root solution:

```bash
dotnet restore
dotnet build
```

Neu co test project:

```bash
dotnet test
```

## 9) Deliverables

- Cac endpoint BE tuong thich FE contract hien tai.
- Test case moi/duoc cap nhat cho contract moi.
- Tai lieu API cap nhat trong docs sau khi merge.

