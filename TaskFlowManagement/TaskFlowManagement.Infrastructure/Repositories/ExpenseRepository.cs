using Microsoft.EntityFrameworkCore;
using TaskFlowManagement.Core.DTOs;
using TaskFlowManagement.Core.Entities;
using TaskFlowManagement.Core.Interfaces;
using TaskFlowManagement.Infrastructure.Data;

namespace TaskFlowManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository triển khai IExpenseRepository phục vụ UC-23 (GĐ8) và UC-Report-01 (GĐ9).
    /// Sử dụng IDbContextFactory để đảm bảo thread-safety trong môi trường WinForms.
    /// </summary>
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ExpenseRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Expense?> GetByIdAsync(int id)
        {
            using var ctx = _contextFactory.CreateDbContext();
            return await ctx.Expenses
                .AsNoTracking()
                .Include(e => e.Project)
                .Include(e => e.CreatedBy)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Expense>> GetAllAsync()
        {
            using var ctx = _contextFactory.CreateDbContext();
            return await ctx.Expenses
                .AsNoTracking()
                .Include(e => e.Project)
                .Include(e => e.CreatedBy)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        public async Task<List<Expense>> GetByProjectAsync(int projectId)
        {
            using var ctx = _contextFactory.CreateDbContext();
            return await ctx.Expenses
                .AsNoTracking()
                .Include(e => e.Project)
                .Include(e => e.CreatedBy)
                .Where(e => e.ProjectId == projectId)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalByProjectAsync(int projectId)
        {
            using var ctx = _contextFactory.CreateDbContext();
            // SUM aggregation trực tiếp trên Database – không nạp row vào bộ nhớ
            return await ctx.Expenses
                .Where(e => e.ProjectId == projectId)
                .SumAsync(e => e.Amount);
        }

        /// <summary>
        /// GĐ9 – UC-Report-01: Projection trực tiếp sang ExpenseReportDto.
        ///
        /// Chiến lược tối ưu:
        ///   1. AsNoTracking()   → không overhead theo dõi thay đổi.
        ///   2. GroupJoin        → LEFT JOIN Projects ← Expenses, tính SUM trên DB.
        ///   3. .Select()        → projection thẳng sang DTO, EF Core dịch ra SQL SELECT tối giản.
        ///   4. Lọc projectId    → nếu có, thêm WHERE ở tầng DB (không filter ở C#).
        ///
        /// SQL tương đương (pseudo):
        ///   SELECT p.Id, p.Name, p.Status, c.Name, u.FullName,
        ///          p.Budget, SUM(e.Amount) AS TotalExpense,
        ///          p.StartDate, p.PlannedEndDate
        ///   FROM Projects p
        ///   LEFT JOIN Customers c ON p.CustomerId = c.Id
        ///   INNER JOIN Users u ON p.OwnerId = u.Id
        ///   LEFT JOIN Expenses e ON e.ProjectId = p.Id
        ///   [WHERE p.Id = @projectId]
        ///   GROUP BY p.Id, p.Name, ...
        ///   ORDER BY p.Name
        /// </summary>
        public async Task<List<ExpenseReportDto>> GetExpenseReportDataAsync(int? projectId = null)
        {
            using var ctx = _contextFactory.CreateDbContext();

            // Bắt đầu từ Projects – đây là bảng chính của báo cáo
            var query = ctx.Projects
                .AsNoTracking()
                .Include(p => p.Customer)
                .Include(p => p.Owner)
                .Include(p => p.Expenses)
                .AsQueryable();

            // Áp dụng filter nếu có projectId
            if (projectId.HasValue)
            {
                query = query.Where(p => p.Id == projectId.Value);
            }

            // Projection trung gian lấy DateOnly ra khỏi Database
            var rawData = await query
                .OrderBy(p => p.Name)
                .Select(p => new 
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    ProjectStatus = p.Status,
                    CustomerName = p.Customer != null ? p.Customer.CompanyName : "—",
                    ManagerName = p.Owner.FullName,
                    Budget = p.Budget,
                    TotalExpense = p.Expenses.Sum(e => (decimal?)e.Amount) ?? 0m,
                    StartDate = p.StartDate,
                    PlannedEndDate = p.PlannedEndDate
                })
                .ToListAsync();

            // Ánh xạ sang ExpenseReportDto & convert DateOnly -> DateTime trên Client Memory
            var result = rawData.Select(p => new ExpenseReportDto
            {
                ProjectId = p.ProjectId,
                ProjectName = p.ProjectName,
                ProjectStatus = p.ProjectStatus,
                CustomerName = p.CustomerName,
                ManagerName = p.ManagerName,
                Budget = p.Budget,
                TotalExpense = p.TotalExpense,
                StartDate = p.StartDate.ToDateTime(TimeOnly.MinValue),
                PlannedEndDate = p.PlannedEndDate?.ToDateTime(TimeOnly.MinValue)
            }).ToList();

            return result;
        }

        public async Task AddAsync(Expense entity)
        {
            using var ctx = _contextFactory.CreateDbContext();
            entity.CreatedAt = DateTime.UtcNow;
            await ctx.Expenses.AddAsync(entity);
            await ctx.SaveChangesAsync();
        }

        public async Task UpdateAsync(Expense entity)
        {
            using var ctx = _contextFactory.CreateDbContext();
            ctx.Expenses.Update(entity);
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var ctx = _contextFactory.CreateDbContext();
            await ctx.Expenses.Where(e => e.Id == id).ExecuteDeleteAsync();
        }
    }
}
