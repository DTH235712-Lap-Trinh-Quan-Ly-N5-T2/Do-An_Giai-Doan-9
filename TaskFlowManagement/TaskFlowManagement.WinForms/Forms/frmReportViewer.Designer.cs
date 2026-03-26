namespace TaskFlowManagement.WinForms.Forms
{
    partial class frmReportViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // ── Controls ──────────────────────────────────────────────────────
        private Microsoft.Reporting.WinForms.ReportViewer reportViewer;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel panelContent;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support – do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components   = new System.ComponentModel.Container();
            this.panelHeader  = new System.Windows.Forms.Panel();
            this.lblTitle     = new System.Windows.Forms.Label();
            this.lblSubtitle  = new System.Windows.Forms.Label();
            this.panelContent = new System.Windows.Forms.Panel();
            this.lblStatus    = new System.Windows.Forms.Label();
            this.reportViewer = new Microsoft.Reporting.WinForms.ReportViewer();

            this.panelHeader.SuspendLayout();
            this.panelContent.SuspendLayout();
            this.SuspendLayout();

            // ── panelHeader ───────────────────────────────────────────────
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(15, 23, 42); // #0F1722
            this.panelHeader.Controls.Add(this.lblSubtitle);
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock     = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height   = 72;
            this.panelHeader.Name     = "panelHeader";
            this.panelHeader.TabIndex = 0;

            // ── lblTitle ──────────────────────────────────────────────────
            this.lblTitle.AutoSize  = false;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location  = new System.Drawing.Point(20, 10);
            this.lblTitle.Name      = "lblTitle";
            this.lblTitle.Size      = new System.Drawing.Size(900, 30);
            this.lblTitle.Text      = "📊  BÁO CÁO CHI PHÍ & NGÂN SÁCH DỰ ÁN";

            // ── lblSubtitle ───────────────────────────────────────────────
            this.lblSubtitle.AutoSize  = false;
            this.lblSubtitle.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184); // slate-400
            this.lblSubtitle.Location  = new System.Drawing.Point(22, 44);
            this.lblSubtitle.Name      = "lblSubtitle";
            this.lblSubtitle.Size      = new System.Drawing.Size(900, 18);
            this.lblSubtitle.Text      = "TaskFlow Management  ·  Giai đoạn 9  ·  Hệ thống Báo cáo RDLC";

            // ── panelContent ──────────────────────────────────────────────
            this.panelContent.Controls.Add(this.reportViewer);
            this.panelContent.Controls.Add(this.lblStatus);
            this.panelContent.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Name     = "panelContent";
            this.panelContent.Padding  = new System.Windows.Forms.Padding(0);
            this.panelContent.TabIndex = 1;

            // ── lblStatus ─────────────────────────────────────────────────
            this.lblStatus.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblStatus.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblStatus.Height    = 40;
            this.lblStatus.Name      = "lblStatus";
            this.lblStatus.Padding   = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.lblStatus.Text      = "⏳ Đang khởi tạo báo cáo...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.Visible   = true;

            // ── reportViewer ──────────────────────────────────────────────
            this.reportViewer.Dock                        = System.Windows.Forms.DockStyle.Fill;
            this.reportViewer.Name                        = "reportViewer";
            this.reportViewer.TabIndex                    = 0;
            this.reportViewer.ZoomMode                    = Microsoft.Reporting.WinForms.ZoomMode.PageWidth;
            this.reportViewer.ShowParameterPrompts        = false;
            this.reportViewer.ShowCredentialPrompts       = false;

            // ── frmReportViewer ───────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelHeader);
            this.MinimumSize         = new System.Drawing.Size(1000, 700);
            this.Name                = "frmReportViewer";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.WindowState         = System.Windows.Forms.FormWindowState.Maximized;
            this.Load               += new System.EventHandler(this.frmReportViewer_Load);

            this.panelHeader.ResumeLayout(false);
            this.panelContent.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
