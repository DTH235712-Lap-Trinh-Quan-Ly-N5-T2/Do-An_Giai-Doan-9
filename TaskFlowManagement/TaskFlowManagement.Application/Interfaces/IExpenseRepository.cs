using TaskFlowManagement.Core.DTOs;
using TaskFlowManagement.Core.Entities;

namespace TaskFlowManagement.Core.Interfaces
{
    /// <summary>
    /// Repository cho Expense – CRUD chi phí dự án (GĐ8: UC-23 / GĐ9: UC-Report-01).
    /// Kế thừa IRepository&lt;Expense&gt; → có sẵn GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync.
    /// </summary>
    public interface IExpenseRepository : IRepository<Expense>
    {
        /// <summary>
        /// Lấy danh sách chi phí của một dự án, kèm thông tin người tạo (CreatedBy).
        /// Sắp xếp theo ExpenseDate giảm dần (mới nhất lên đầu).
        /// </summary>
        Task<List<Expense>> GetByProjectAsync(int projectId);

        /// <summary>
        /// Tính tổng chi phí thực tế của một dự án trực tiếp trên DB (SUM aggregation).
        /// Trả về 0 nếu chưa có chi phí nào.
        /// Kết quả là decimal – KHÔNG dùng double/float.
        /// </summary>
        Task<decimal> GetTotalByProjectAsync(int projectId);

        /// <summary>
        /// Lấy dữ liệu báo cáo chi phí và ngân sách dự án (GĐ9: UC-Report-01).
        ///
        /// Kỹ thuật quan trọng:
        ///   - Dùng .AsNoTracking() → chỉ đọc, không theo dõi thay đổi Entity.
        ///   - Dùng Projection .Select() trực tiếp sang ExpenseReportDto → KHÔNG nạp Entity thừa.
        ///   - GroupJoin (LEFT JOIN) giữa Project và Expenses → tính SUM(Amount) trên DB.
        ///   - Bypass quy tắc "chỉ trả Entity" – được phép cho trường hợp Report.
        ///
        /// Tham số:
        ///   - projectId = null → tất cả dự án.
        ///   - projectId có giá trị → lọc theo dự án cụ thể.
        /// </summary>
        Task<List<ExpenseReportDto>> GetExpenseReportDataAsync(int? projectId = null);
    }
}
