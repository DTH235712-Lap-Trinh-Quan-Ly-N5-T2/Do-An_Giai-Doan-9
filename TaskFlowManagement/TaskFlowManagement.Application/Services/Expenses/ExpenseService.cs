using TaskFlowManagement.Core.DTOs;
using TaskFlowManagement.Core.Entities;
using TaskFlowManagement.Core.Interfaces;
using TaskFlowManagement.Core.Interfaces.Services;

namespace TaskFlowManagement.Core.Services.Expenses
{
    /// <summary>
    /// Triển khai nghiệp vụ Quản lý Chi phí (GĐ8: UC-23 / GĐ9: UC-Report-01).
    /// Tuân thủ quy tắc Financial Precision và Audit Trail.
    /// </summary>
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly ITaskService _taskService;

        public ExpenseService(IExpenseRepository expenseRepo, IProjectRepository projectRepo, ITaskService taskService)
        {
            _expenseRepo = expenseRepo;
            _projectRepo = projectRepo;
            _taskService = taskService;
        }

        public async Task<List<Expense>> GetByProjectAsync(int projectId)
        {
            return await _expenseRepo.GetByProjectAsync(projectId);
        }

        public async Task<(bool Success, string Message)> AddExpenseAsync(Expense expense)
        {
            // 1. Validation
            if (expense.Amount <= 0)
                return (false, "Số tiền phải lớn hơn 0.");

            if (expense.ExpenseDate > DateOnly.FromDateTime(DateTime.Today))
                return (false, "Ngày chi phí không được vượt quá ngày hiện tại.");

            if (expense.ProjectId <= 0)
                return (false, "Vui lòng chọn dự án hợp lệ.");

            if (string.IsNullOrWhiteSpace(expense.ExpenseType))
                return (false, "Vui lòng chọn loại chi phí.");

            try
            {
                // Audit Trail: Thời điểm phát sinh
                expense.CreatedAt = DateTime.UtcNow;

                await _expenseRepo.AddAsync(expense);
                _taskService.NotifyDataChanged(); // Đồng bộ Dashboard
                return (true, "Thêm chi phí thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi server: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateExpenseAsync(Expense expense)
        {
            if (expense.Amount <= 0)
                return (false, "Số tiền phải lớn hơn 0.");

            if (expense.ExpenseDate > DateOnly.FromDateTime(DateTime.Today))
                return (false, "Ngày chi phí không được vượt quá ngày hiện tại.");

            try
            {
                var existing = await _expenseRepo.GetByIdAsync(expense.Id);
                if (existing == null) return (false, "Không tìm thấy khoản chi phí này.");

                // Cập nhật các trường cho phép
                existing.ProjectId   = expense.ProjectId;
                existing.ExpenseType = expense.ExpenseType;
                existing.Amount      = expense.Amount;
                existing.ExpenseDate = expense.ExpenseDate;
                existing.Note        = expense.Note;

                await _expenseRepo.UpdateAsync(existing);
                _taskService.NotifyDataChanged();
                return (true, "Cập nhật chi phí thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi server: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteExpenseAsync(int expenseId)
        {
            try
            {
                var existing = await _expenseRepo.GetByIdAsync(expenseId);
                if (existing == null) return (false, "Không tìm thấy khoản chi phí này.");

                await _expenseRepo.DeleteAsync(expenseId);
                _taskService.NotifyDataChanged();
                return (true, "Xóa chi phí thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi server: {ex.Message}");
            }
        }

        public async Task<ProjectBudgetSummaryDto?> GetProjectBudgetSummaryAsync(int projectId)
        {
            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project == null) return null;

            var totalExpense = await _expenseRepo.GetTotalByProjectAsync(projectId);

            return new ProjectBudgetSummaryDto
            {
                ProjectId    = project.Id,
                ProjectName  = project.Name,
                Budget       = project.Budget,
                TotalExpense = totalExpense
            };
        }

        /// <summary>
        /// GĐ9 – UC-Report-01: Lấy dữ liệu báo cáo chi phí và ngân sách.
        ///
        /// Service chỉ làm nhiệm vụ điều phối (orchestration):
        ///   - Ủy thác toàn bộ truy vấn xuống Repository (tuân thủ SRP).
        ///   - Repository chịu trách nhiệm Projection, AsNoTracking, GroupBy.
        ///   - Service bắt exception và bọc lại thành list rỗng thay vì để UI crash.
        /// </summary>
        public async Task<List<ExpenseReportDto>> GetExpenseReportDataAsync(int? projectId = null)
        {
            return await _expenseRepo.GetExpenseReportDataAsync(projectId);
        }
    }
}
