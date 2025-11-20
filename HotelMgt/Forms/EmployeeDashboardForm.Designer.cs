namespace HotelMgt.Forms
{
    partial class EmployeeDashboardForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblWelcome;
        private HotelMgt.Custom.RoundedButton btnLogout; // use RoundedButton
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabOverview;
        private System.Windows.Forms.TabPage tabCheckIn;
        private System.Windows.Forms.TabPage tabCheckOut;
        private System.Windows.Forms.TabPage tabReservations;
        private System.Windows.Forms.TabPage tabAvailableRooms;
        private System.Windows.Forms.TabPage tabGuestSearch;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tabControl = new TabControl();
            tabOverview = new TabPage();
            tabCheckIn = new TabPage();
            tabCheckOut = new TabPage();
            tabReservations = new TabPage();
            tabAvailableRooms = new TabPage();
            tabGuestSearch = new TabPage();
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
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tabControl.Location = new Point(0, 70);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1200, 630);
            tabControl.TabIndex = 1;
            // 
            // tabOverview
            // 
            tabOverview.Location = new Point(4, 26);
            tabOverview.Name = "tabOverview";
            tabOverview.Size = new Size(1192, 600);
            tabOverview.TabIndex = 0;
            tabOverview.Text = "Overview";
            tabOverview.UseVisualStyleBackColor = true;
            // 
            // tabCheckIn
            // 
            tabCheckIn.Location = new Point(4, 26);
            tabCheckIn.Name = "tabCheckIn";
            tabCheckIn.Size = new Size(1376, 761);
            tabCheckIn.TabIndex = 1;
            tabCheckIn.Text = "Check-In";
            tabCheckIn.UseVisualStyleBackColor = true;
            // 
            // tabCheckOut
            // 
            tabCheckOut.Location = new Point(4, 26);
            tabCheckOut.Name = "tabCheckOut";
            tabCheckOut.Size = new Size(1376, 761);
            tabCheckOut.TabIndex = 2;
            tabCheckOut.Text = "Check-Out";
            tabCheckOut.UseVisualStyleBackColor = true;
            // 
            // tabReservations
            // 
            tabReservations.Location = new Point(4, 26);
            tabReservations.Name = "tabReservations";
            tabReservations.Size = new Size(1376, 761);
            tabReservations.TabIndex = 3;
            tabReservations.Text = "Reservations";
            tabReservations.UseVisualStyleBackColor = true;
            // 
            // tabAvailableRooms
            // 
            tabAvailableRooms.Location = new Point(4, 26);
            tabAvailableRooms.Name = "tabAvailableRooms";
            tabAvailableRooms.Size = new Size(1376, 761);
            tabAvailableRooms.TabIndex = 4;
            tabAvailableRooms.Text = "Available Rooms";
            tabAvailableRooms.UseVisualStyleBackColor = true;
            // 
            // tabGuestSearch
            // 
            tabGuestSearch.Location = new Point(4, 26);
            tabGuestSearch.Name = "tabGuestSearch";
            tabGuestSearch.Size = new Size(1376, 761);
            tabGuestSearch.TabIndex = 5;
            tabGuestSearch.Text = "Guest Search";
            tabGuestSearch.UseVisualStyleBackColor = true;
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
            panelHeader.Resize += PanelHeader_Resize;
            // 
            // btnLogout
            // 
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.BackColor = Color.FromArgb(239, 68, 68);
            btnLogout.BorderColor = Color.Transparent;
            btnLogout.BorderRadius = 8;
            btnLogout.BorderSize = 0;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, 56, 56);
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 64, 64);
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
            lblTitle.Size = new Size(233, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Golden Courtyard";
            // 
            // EmployeeDashboardForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 244, 248);
            ClientSize = new Size(1200, 700);
            Controls.Add(tabControl);
            Controls.Add(panelHeader);
            MinimumSize = new Size(900, 600);
            Name = "EmployeeDashboardForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Hotel Management - Employee Dashboard";
            Load += EmployeeDashboardForm_Load;
            tabControl.ResumeLayout(false);
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
        }
    }
}