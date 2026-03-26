namespace TaskFlowManagement.Core.DTOs
{
    /// <summary>
    /// DTO chuyên biệt cho báo cáo Chi phí &amp; Ngân sách (Giai đoạn 9 – UC-Report-01).
    ///
    /// Thiết kế "Flat DTO":
    ///   - Kết hợp dữ liệu từ Project, Customer, Owner (Manager) và SUM(Expense.Amount)
    ///     thành một đối tượng phẳng, không có navigation property.
    ///   - Được tạo trực tiếp bằng Projection (.Select()) trong ExpenseRepository
    ///     → Tránh nạp toàn bộ Entity vào bộ nhớ (N+1, eager-loading thừa).
    ///
    /// Constraint Vàng: Tất cả trường tiền tệ PHẢI dùng decimal (KHÔNG dùng double/float).
    /// </summary>
    public class ExpenseReportDto
    {
        // ── Thông tin Dự án ──────────────────────────────────────────────────

        /// <summary>ID dự án – dùng để group hoặc filter ở tầng Service.</summary>
        public int ProjectId { get; set; }

        /// <summary>Tên dự án.</summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>Trạng thái dự án: NotStarted | InProgress | OnHold | Completed | Cancelled.</summary>
        public string ProjectStatus { get; set; } = string.Empty;

        // ── Thông tin Khách hàng ─────────────────────────────────────────────

        /// <summary>Tên khách hàng. Trả về "—" nếu dự án chưa gán khách hàng.</summary>
        public string CustomerName { get; set; } = "—";

        // ── Thông tin Quản lý dự án (Owner) ─────────────────────────────────

        /// <summary>Họ tên đầy đủ của người quản lý dự án (Project.Owner.FullName).</summary>
        public string ManagerName { get; set; } = string.Empty;

        // ── Tài chính (decimal – Constraint Vàng) ────────────────────────────

        /// <summary>Ngân sách ban đầu của dự án (Project.Budget). Kiểu decimal(18,2).</summary>
        public decimal Budget { get; set; }

        /// <summary>
        /// Tổng chi phí thực tế = SUM(Expense.Amount) của tất cả chi phí thuộc dự án.
        /// Kiểu decimal – tổng hợp tại tầng Database qua GroupBy + Sum.
        /// </summary>
        public decimal TotalExpense { get; set; }

        /// <summary>Số tiền còn lại = Budget – TotalExpense. Tính tại computed property.</summary>
        public decimal Remaining => Budget - TotalExpense;

        /// <summary>
        /// Tỷ lệ sử dụng ngân sách (%) = TotalExpense / Budget * 100.
        /// Làm tròn 1 chữ số thập phân. Trả về 0 nếu Budget = 0 (tránh chia zero).
        /// Kiểu decimal – KHÔNG dùng double.
        /// </summary>
        public decimal UsagePercent => Budget > 0
            ? Math.Round((TotalExpense / Budget) * 100m, 1)
            : 0m;

        /// <summary>Cờ cảnh báo: TotalExpense vượt quá Budget.</summary>
        public bool IsOverBudget => TotalExpense > Budget && Budget > 0;

        // ── Metadata thời gian ───────────────────────────────────────────────

        /// <summary>Ngày bắt đầu dự án.</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Ngày kết thúc dự kiến của dự án.</summary>
        public DateTime? PlannedEndDate { get; set; }
    }
}
