# Changelog – TaskFlow Management

Format: [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

---

## [v9.0.0] – 2026-03-30### Added / Fixed
- **UC-Report-01**: Báo cáo Chi phí & Ngân sách Dự án dùng công nghệ Microsoft RDLC.
- Dứt điểm tương thích RDLC trên .NET 8 (Fix Schema 2008, DateOnly Mapping và Dependency Pinning).
- **[NEW]** `DTOs/ExpenseReportDto.cs` – DTO phẳng hóa dữ liệu cho báo cáo:
  - Gom dữ liệu từ Project, Customer, Owner và SUM(Expense) thành 1 object
  - Computed properties: `Remaining`, `UsagePercent` (decimal), `IsOverBudget`
  - **Constraint Vàng**: 100% trường tiền tệ dùng `decimal` (không có `double/float`)
- **[MODIFY]** `Interfaces/Services/IExpenseService.cs` – Bổ sung:
  - `Task<List<ExpenseReportDto>> GetExpenseReportDataAsync(int? projectId = null);`
- **[MODIFY]** `Interfaces/IExpenseRepository.cs` – Bổ sung:
  - `Task<List<ExpenseReportDto>> GetExpenseReportDataAsync(int? projectId = null);`

### ✅ Tầng Infrastructure (Data Access)
- **[MODIFY]** `Repositories/ExpenseRepository.cs` – Triển khai `GetExpenseReportDataAsync`:
  - `AsNoTracking()` toàn bộ truy vấn báo cáo
  - Projection `.Select()` trực tiếp từ Entity sang `ExpenseReportDto` (bypass Entity rule)
  - LEFT JOIN Project → Customer, Owner, Expenses với `SUM(Amount)` trên DB
  - `(decimal?)e.Amount ?? 0m` – xử lý NULL-safe cho dự án chưa có chi phí

### ✅ Tầng Presentation (WinForms – UI & RDLC)
- **[NEW]** `Forms/frmReportViewer.cs` – Form hiển thị báo cáo dùng chung:
  - `async/await` load data với `WaitCursor` khi đang tải
  - `ReportViewer` mode Local, nhúng RDLC qua `EmbeddedResource`
  - Truyền `pReporter` (AppSession.FullName) và `pExportDate` (DateTime.Now vi-VN)
- **[NEW]** `Forms/frmReportViewer.Designer.cs` – Layout: Header navy #0F1722, ReportViewer Dock Fill
- **[MODIFY]** `Forms/frmExpenses.cs` – Thêm nút **📊 Xuất báo cáo** (Purple):
  - Truyền projectId từ ComboBox lọc (null → báo cáo tổng hợp)
  - Mở `frmReportViewer` bằng `new` trực tiếp (exception pattern hợp lệ cho runtime param)

### ✅ Báo cáo RDLC
- **[NEW]** `Reports/ProjectExpenseReport.rdlc` – Toàn bộ XML báo cáo:
  - Header: màu `#0F1722` (navy đậm), chữ trắng, Segoe UI 16pt Bold
  - Bảng 9 cột: STT, Tên dự án, Khách hàng, Quản lý, Ngân sách, Chi phí TT, Còn lại, Tỷ lệ%, Trạng thái
  - Định dạng N0 + ký hiệu ` ₫`, ngôn ngữ vi-VN
  - Zebra-stripe rows (trắng / #F8FAFC xen kẽ)
  - Cảnh báo màu đỏ `#DC2626` khi chi phí vượt ngân sách (`IsOverBudget`)
  - Cảnh báo màu vàng `#D97706` khi UsagePercent > 80%
  - Dòng tổng cộng SUM (Budget, TotalExpense, Remaining)
  - Footer: **"Người xuất: [pReporter] – Ngày xuất: [pExportDate]"** + số trang
  - Khổ A4 Landscape (29.7 × 21 cm), margin 1.2cm
- **[NEW]** `Reports/dsTaskFlow.xsd` – Typed DataSet Schema:
  - Ánh xạ chính xác 12 field → `ExpenseReportDto` properties
  - Khai báo type: `xs:decimal` cho Budget/TotalExpense/Remaining/UsagePercent
  - Hỗ trợ nullable cho `PlannedEndDate` (`nillable="true"`)

### ✅ Cấu hình dự án
- **[MODIFY]** `TaskFlowManagement.WinForms.csproj`:
  - Thêm `<PackageReference Include="Microsoft.Reporting.WinForms" Version="15.2.1" />`
  - Thêm `<EmbeddedResource Include="Reports\ProjectExpenseReport.rdlc" />`
  - Thêm `<None Include="Reports\dsTaskFlow.xsd" />`
- **[MODIFY]** `Program.cs` – Ghi chú exception pattern cho `frmReportViewer` (dùng `new` thay DI)

### 🔧 Kỹ thuật nổi bật
- **Projection Pattern** – Repository truy vấn thẳng sang DTO, không nạp Entity vào RAM
- **Async Report Loading** – Không block UI thread khi truy vấn DB
- **Parameter Injection** – Audit trail tự động qua `AppSession.FullName`
- **Conditional Formatting** – RDLC tự tô màu theo ngưỡng ngân sách (xanh/vàng/đỏ)
- **Zero-safe Division** – `UsagePercent` trả về `0m` nếu `Budget = 0`

---

## [v1.0.0] – 2026-03-05 · Giai đoạn 1: Foundation

### ✅ Kiến trúc
- **Clean Architecture 4 tầng**: Application → Infrastructure → WinForms (+ Service Layer)
- **Dependency Injection** với `Microsoft.Extensions.DependencyInjection`
- **Repository Pattern** với generic `IRepository<T>` + 4 repository chuyên biệt
- **Service Layer**: `AuthService`, `UserService`, `ProjectService`, `TaskService`
- **DTO Pattern**: `LoginResult`, `UserSessionDto` – Form không nhận Entity thô

### ✅ Database
- **Entity Framework Core 8** + SQL Server Express 2022
- **15 Entity/Bảng** với Fluent API config đầy đủ
- **Auto-migration + Auto-seed** khi chạy lần đầu
- **Performance indexes**: 11 indexes đơn + 1 composite index `(ProjectId, StatusId)`
- **Soft delete** cho User (`IsActive = false`)

### ✅ Bảo mật
- **BCrypt** password hashing (WorkFactor 12) – thay thế SHA-256
- Salt tự động per-user – cùng password khác hash

### ✅ UI/UX
- Form Login **2 panel**: branding xanh trái + form trắng phải
- **Eye icon** (👁) toggle hiện/ẩn mật khẩu
- Menu bar + Status bar **navy đậm** (#0F1722)
- **Real-time clock** màu xanh lá trên status bar
- Phân quyền menu tự ẩn/hiện theo Role

### ✅ Tài liệu
- `README.md` với hướng dẫn `git clone` + setup đầy đủ
- `docs/architecture.md` – sơ đồ kiến trúc + request flow
- `docs/erd.md` – ERD 15 bảng + indexes
- `docs/usecase.md` – Use Case + ma trận quyền
- `CHANGELOG.md` – lịch sử thay đổi

### 🔧 Kỹ thuật nổi bật
- `IDbContextFactory` – mỗi Repository tạo DbContext riêng
- `AsNoTracking()` mặc định cho tất cả SELECT
- `ExecuteUpdateAsync` / `ExecuteDeleteAsync` – tránh SELECT-then-UPDATE
- `GetPagedAsync` – phân trang + optional filter chain
- Retry policy 3 lần khi SQL Server tạm lỗi
- Async/Await Synchronized `UpdateLastLoginAsync` (đảm bảo tính nhất quán)

---

## [v2.0.0] – Giai đoạn 2 (Sắp có)

### 📋 Dự kiến
- [ ] Form CRUD Users (quản lý tài khoản)
- [ ] Form CRUD Projects (tạo/sửa/xóa dự án)
- [ ] Form CRUD Tasks (tạo/sửa/xóa task)
- [ ] Form CRUD Customers
- [ ] Đổi mật khẩu (BCrypt sẵn rồi)
- [ ] Quản lý thành viên dự án
