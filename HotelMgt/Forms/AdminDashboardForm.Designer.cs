namespace HotelMgt.Forms
{
    partial class AdminDashboardForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            tabControl = new TabControl();
            tabOverview = new TabPage();
            tabCheckIn = new TabPage();
            tabCheckOut = new TabPage();
            tabReservations = new TabPage();
            tabAvailableRooms = new TabPage();
            tabGuestSearch = new TabPage();
            tabEmployeeManagement = new TabPage();
            tabRoomManagement = new TabPage();
            tabRevenueReport = new TabPage();
            panelHeader = new Panel();
            btnLogout = new HotelMgt.Custom.RoundedButton();
            lblWelcome = new Label();
            lblTitle = new Label();
            tabControl.SuspendLayout();
            panelHeader.SuspendLayout();
            SuspendLayout();
            this.WindowState = FormWindowState.Maximized;
            this.MaximizeBox = false; // disables the maximize/restore button            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabOverview);
            tabControl.Controls.Add(tabCheckIn);
            tabControl.Controls.Add(tabCheckOut);
            tabControl.Controls.Add(tabReservations);
            tabControl.Controls.Add(tabAvailableRooms);
            tabControl.Controls.Add(tabGuestSearch);
            tabControl.Controls.Add(tabEmployeeManagement);
            tabControl.Controls.Add(tabRoomManagement);
            tabControl.Controls.Add(tabRevenueReport);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tabControl.Location = new Point(0, 70);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1200, 630);
            tabControl.TabIndex = 1;
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
            // 
            // tabOverview
            // 
            tabOverview.Location = new Point(4, 26);
            tabOverview.Name = "tabOverview";
            tabOverview.Size = new Size(1192, 600);
            tabOverview.TabIndex = 9;
            tabOverview.Text = "Overview";
            tabOverview.UseVisualStyleBackColor = true;
            // 
            // tabCheckIn
            // 
            tabCheckIn.Location = new Point(4, 26);
            tabCheckIn.Name = "tabCheckIn";
            tabCheckIn.Padding = new Padding(3);
            tabCheckIn.Size = new Size(1376, 761);
            tabCheckIn.TabIndex = 0;
            tabCheckIn.Text = "Check-In";
            tabCheckIn.UseVisualStyleBackColor = true;
            // 
            // tabCheckOut
            // 
            tabCheckOut.Location = new Point(4, 26);
            tabCheckOut.Name = "tabCheckOut";
            tabCheckOut.Padding = new Padding(3);
            tabCheckOut.Size = new Size(1376, 761);
            tabCheckOut.TabIndex = 1;
            tabCheckOut.Text = "Check-Out";
            tabCheckOut.UseVisualStyleBackColor = true;
            // 
            // tabReservations
            // 
            tabReservations.Location = new Point(4, 26);
            tabReservations.Name = "tabReservations";
            tabReservations.Size = new Size(1376, 761);
            tabReservations.TabIndex = 2;
            tabReservations.Text = "Reservations";
            tabReservations.UseVisualStyleBackColor = true;
            // 
            // tabAvailableRooms
            // 
            tabAvailableRooms.Location = new Point(4, 26);
            tabAvailableRooms.Name = "tabAvailableRooms";
            tabAvailableRooms.Size = new Size(1376, 761);
            tabAvailableRooms.TabIndex = 3;
            tabAvailableRooms.Text = "Rooms";
            tabAvailableRooms.UseVisualStyleBackColor = true;
            // 
            // tabGuestSearch
            // 
            tabGuestSearch.Location = new Point(4, 26);
            tabGuestSearch.Name = "tabGuestSearch";
            tabGuestSearch.Size = new Size(1376, 761);
            tabGuestSearch.TabIndex = 4;
            tabGuestSearch.Text = "Guest Search";
            tabGuestSearch.UseVisualStyleBackColor = true;
            // 
            // tabEmployeeManagement
            // 
            tabEmployeeManagement.Location = new Point(4, 26);
            tabEmployeeManagement.Name = "tabEmployeeManagement";
            tabEmployeeManagement.Size = new Size(1376, 761);
            tabEmployeeManagement.TabIndex = 5;
            tabEmployeeManagement.Text = "Employees";
            tabEmployeeManagement.UseVisualStyleBackColor = true;
            // 
            // tabRoomManagement
            // 
            tabRoomManagement.Location = new Point(4, 26);
            tabRoomManagement.Name = "tabRoomManagement";
            tabRoomManagement.Size = new Size(1376, 761);
            tabRoomManagement.TabIndex = 6;
            tabRoomManagement.Text = "Rooms Mgmt";
            tabRoomManagement.UseVisualStyleBackColor = true;
            // 
            // tabRevenueReport
            // 
            tabRevenueReport.Location = new Point(4, 26);
            tabRevenueReport.Name = "tabRevenueReport";
            tabRevenueReport.Size = new Size(1376, 761);
            tabRevenueReport.TabIndex = 7;
            tabRevenueReport.Text = "Reports";
            tabRevenueReport.UseVisualStyleBackColor = true;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.Navy;
            panelHeader.Controls.Add(btnLogout);
            panelHeader.Controls.Add(lblWelcome);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1200, 70);
            panelHeader.TabIndex = 0;
            // 
            // btnLogout
            // 
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.BackColor = Color.FromArgb(239, 68, 68);
            btnLogout.BorderColor = Color.Transparent;
            btnLogout.BorderRadius = 8;
            btnLogout.BorderSize = 0;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLogout.ForeColor = Color.White;
            btnLogout.Location = new Point(1066, 15);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(120, 40);
            btnLogout.TabIndex = 3;
            btnLogout.Text = "Logout";
            btnLogout.UseVisualStyleBackColor = false;
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.ForeColor = Color.FromArgb(200, 220, 255);
            lblWelcome.Location = new Point(20, 42);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(103, 15);
            lblWelcome.TabIndex = 1;
            lblWelcome.Text = "Welcome, [Name]";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.Snow;
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Golden Courtyard";
            // 
            // AdminDashboardForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 244, 248);
            ClientSize = new Size(1200, 700);
            Controls.Add(tabControl);
            Controls.Add(panelHeader);
            MinimumSize = new Size(900, 600);
            Name = "AdminDashboardForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Hotel Management - Admin Dashboard";
            WindowState = FormWindowState.Maximized;
            FormClosing += AdminDashboardForm_FormClosing;
            Load += AdminDashboardForm_Load;
            tabControl.ResumeLayout(false);
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabCheckIn;
        private TabPage tabCheckOut;
        private TabPage tabReservations;
        private TabPage tabAvailableRooms;
        private TabPage tabGuestSearch;
        private TabPage tabEmployeeManagement;
        private TabPage tabRoomManagement;
        private TabPage tabRevenueReport;
        private TabPage tabOverview;
        private Panel panelHeader;
        private Label lblWelcome;
        private Label lblTitle;
        private Custom.RoundedButton btnLogout;
    }
}