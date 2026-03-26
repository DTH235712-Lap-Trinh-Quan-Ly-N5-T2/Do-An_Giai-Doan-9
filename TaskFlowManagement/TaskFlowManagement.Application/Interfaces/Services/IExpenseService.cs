using TaskFlowManagement.Core.DTOs;
using TaskFlowManagement.Core.Entities;

namespace TaskFlowManagement.Core.Interfaces.Services
{
    /// <summary>
    /// Service nghiệp vụ quản lý Chi phí dự án (GĐ8: UC-23 / GĐ9: UC-Report-01).
    /// Tất cả phương thức đều là async để không chặn UI thread.
    /// </summary>
    public interface IExpenseService
    {
        // ── Danh sách ─────────────────────────────────────────────────────

        /// <summary>
        /// Lấy danh sách chi phí của một dự án (kèm CreatedBy).
        /// Sắp xếp theo ExpenseDate giảm dần.
        /// </summary>
        Task<List<Expense>> GetByProjectAsync(int projectId);

        // ── CRUD ──────────────────────────────────────────────────────────

        /// <summary>
        /// Thêm khoản chi phí mới.
        /// - Validation: Amount phải > 0; ExpenseDate không được lớn hơn ngày hiện tại.
        /// - Audit: CreatedById được gán tự động từ AppSession.UserId.
        /// - CreatedAt được gán tự động DateTime.UtcNow.
        /// </summary>
        Task<(bool Success, string Message)> AddExpenseAsync(Expense expense);

        /// <summary>
        /// Cập nhật thông tin khoản chi phí đã có.
        /// Validation tương tự AddExpenseAsync (Amount > 0, ExpenseDate hợp lệ).
        /// </summary>
        Task<(bool Success, string Message)> UpdateExpenseAsync(Expense expense);

        /// <summary>
        /// Xóa khoản chi phí theo ID (hard delete).
        /// Trả về lỗi nếu không tìm thấy bản ghi.
        /// </summary>
        Task<(bool Success, string Message)> DeleteExpenseAsync(int expenseId);

        // ── Budget Summary ─────────────────────────────────────────────────

        /// <summary>
        /// Trả về tóm tắt ngân sách của một dự án:
        /// Budget (từ Project), TotalExpense (SUM từ Expense), Remaining, UsagePercent.
        /// Tất cả tính toán tiền tệ dùng decimal – KHÔNG dùng double/float.
        /// Trả về null nếu dự án không tồn tại.
        /// </summary>
        Task<ProjectBudgetSummaryDto?> GetProjectBudgetSummaryAsync(int projectId);

        // ── Báo cáo (GĐ9: UC-Report-01) ──────────────────────────────────

        /// <summary>
        /// Lấy dữ liệu báo cáo chi phí và ngân sách dự án (ExpenseReportDto).
        ///
        /// Thiết kế:
        ///   - Khi projectId = null → trả về tất cả dự án (báo cáo tổng hợp).
        ///   - Khi projectId có giá trị → chỉ trả về dữ liệu của dự án đó.
        ///   - Dữ liệu được "phẳng hóa" qua Projection .Select() tại Repository
        ///     → không nạp Entity rác vào bộ nhớ.
        ///   - Trả về list rỗng nếu không có dữ liệu (không throw exception).
        ///   - Mọi trường tiền tệ trong ExpenseReportDto đều là decimal.
        /// </summary>
        /// <param name="projectId">
        ///   null = tất cả dự án;
        ///   có giá trị = lọc theo dự án cụ thể.
        /// </param>
        Task<List<ExpenseReportDto>> GetExpenseReportDataAsync(int? projectId = null);
    }
}
