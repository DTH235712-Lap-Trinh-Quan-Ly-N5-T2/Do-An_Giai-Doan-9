using Microsoft.Reporting.WinForms;
using TaskFlowManagement.Core.Interfaces.Services;
using TaskFlowManagement.WinForms.Common;

namespace TaskFlowManagement.WinForms.Forms
{
    /// <summary>
    /// Form hiển thị báo cáo chi phí và ngân sách dự án (GĐ9 – UC-Report-01).
    ///
    /// Tích hợp Microsoft.Reporting.WinForms (ReportViewer control).
    /// Package NuGet cần thêm vào TaskFlowManagement.WinForms.csproj:
    ///   Microsoft.Reporting.WinForms (v15.x hoặc mới nhất hỗ trợ .NET 8 WinForms)
    ///
    /// Luồng hoạt động:
    ///   1. frmExpenses mở frmReportViewer, truyền vào projectId (null = tất cả).
    ///   2. Form_Load → await GetExpenseReportDataAsync() với WaitCursor.
    ///   3. Tạo ReportDataSource từ List&lt;ExpenseReportDto&gt;.
    ///   4. Truyền Parameter pReporter và pExportDate vào RDLC.
    ///   5. ReportViewer.RefreshReport() → render PDF/Excel/Word nội tuyến.
    /// </summary>
    public partial class frmReportViewer : Form
    {
        // ── Dependencies ───────────────────────────────────────────────────
        private readonly IExpenseService _expenseService;

        // ── Tham số đầu vào ───────────────────────────────────────────────
        /// <summary>
        /// null = báo cáo tổng hợp tất cả dự án.
        /// Có giá trị = báo cáo cho dự án cụ thể.
        /// </summary>
        private readonly int? _projectId;

        // ── Tên RDLC (embedded resource) ─────────────────────────────────
        private const string RDLC_NAME = "TaskFlowManagement.WinForms.Reports.ProjectExpenseReport.rdlc";

        // ── Tên DataSet trong RDLC ────────────────────────────────────────
        private const string DATASET_NAME = "dsExpenseReport";

        // ── Constructor ───────────────────────────────────────────────────

        /// <summary>
        /// Khởi tạo ReportViewer form.
        /// </summary>
        /// <param name="expenseService">Service lấy dữ liệu báo cáo (qua DI).</param>
        /// <param name="projectId">
        ///   null  → báo cáo tổng hợp tất cả dự án.
        ///   int   → báo cáo của một dự án cụ thể.
        /// </param>
        public frmReportViewer(IExpenseService expenseService, int? projectId = null)
        {
            _expenseService = expenseService;
            _projectId      = projectId;

            InitializeComponent();
            SetupFormStyle();
        }

        // ── UI Setup ──────────────────────────────────────────────────────

        private void SetupFormStyle()
        {
            this.Text            = "📊 Báo cáo Chi phí & Ngân sách Dự án – TaskFlow Management";
            this.BackColor       = UIHelper.ColorBackground;
            this.MinimumSize     = new Size(1000, 700);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.WindowState     = FormWindowState.Maximized;
        }

        // ── Sự kiện Form Load ─────────────────────────────────────────────

        private async void frmReportViewer_Load(object sender, EventArgs e)
        {
            await LoadReportAsync();
        }

        // ── Logic Nạp và Render Báo Cáo ──────────────────────────────────

        /// <summary>
        /// Nạp dữ liệu bất đồng bộ, tạo DataSource và render RDLC.
        /// Đổi con trỏ chuột sang WaitCursor khi đang tải để báo hiệu UI đang bận.
        /// </summary>
        private async Task LoadReportAsync()
        {
            // 1. Chuyển sang WaitCursor – ngăn user click linh tinh khi đang load
            this.Cursor   = Cursors.WaitCursor;
            lblStatus.Text = "⏳ Đang tải dữ liệu báo cáo...";
            lblStatus.Visible = true;

            try
            {
                // 2. Lấy dữ liệu bất đồng bộ từ Service → Repository → DB
                var reportData = await _expenseService.GetExpenseReportDataAsync(_projectId);

                if (reportData.Count == 0)
                {
                    lblStatus.Text = "ℹ️ Không có dữ liệu để hiển thị báo cáo.";
                    return;
                }

                lblStatus.Visible = false;

                // 3. Cấu hình ReportViewer – dùng chế độ Local (không cần Report Server)
                reportViewer.ProcessingMode        = ProcessingMode.Local;
                reportViewer.LocalReport.ReportEmbeddedResource = RDLC_NAME;

                // 4. Gán DataSource – tên phải khớp với DataSet name trong RDLC
                var dataSource = new ReportDataSource(DATASET_NAME, reportData);
                reportViewer.LocalReport.DataSources.Clear();
                reportViewer.LocalReport.DataSources.Add(dataSource);

                // 5. Truyền Parameters vào RDLC
                //    pReporter  → Người xuất báo cáo (lấy từ AppSession)
                //    pExportDate → Ngày xuất (DateTime.Now, format vi-VN)
                var parameters = new ReportParameter[]
                {
                    new ReportParameter(
                        "pReporter",
                        AppSession.FullName
                    ),
                    new ReportParameter(
                        "pExportDate",
                        DateTime.Now.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture)
                    )
                };
                reportViewer.LocalReport.SetParameters(parameters);

                // 6. Refresh – trigger render RDLC thành nội dung hiển thị
                reportViewer.RefreshReport();
            }
            catch (Exception ex)
            {
                // Vòng lặp này sẽ đào tận gốc rễ tất cả các InnerException đang bị giấu
                string errorDetails = "";
                Exception? currentEx = ex;
                while (currentEx != null)
                {
                    errorDetails += "👉 " + currentEx.Message + "\n\n";
                    currentEx = currentEx.InnerException;
                }

                lblStatus.Text = "❌ Lỗi định nghĩa báo cáo (xem popup)";
                MessageBox.Show(
                    $"Toàn bộ chuỗi lỗi:\n\n{errorDetails}",
                    "Khui Lỗi Báo Cáo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // 7. Khôi phục con trỏ chuột dù thành công hay lỗi
                this.Cursor = Cursors.Default;
            }
        }
    }
}
