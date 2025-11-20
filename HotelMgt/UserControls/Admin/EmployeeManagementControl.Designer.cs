using System.Windows.Forms;
using System.Drawing;
using HotelMgt.Custom;

namespace HotelMgt.UserControls.Admin
{
    partial class EmployeeManagementControl
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

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            _headerPanel = new RoundedPanel();
            _lblTitle = new Label();
            _lblDesc = new Label();
            _btnAddEmployee = new Button();
            _dgvEmployees = new DataGridView();

            SuspendLayout();

            // this (UserControl)
            BackColor = Color.FromArgb(244, 246, 250);
            Dock = DockStyle.Fill;

            // _headerPanel
            _headerPanel.BackColor = Color.White;
            _headerPanel.BorderRadius = 12;
            _headerPanel.Location = new Point(20, 20);
            _headerPanel.Size = new Size(Width - 40, 90);
            _headerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // _lblTitle
            _lblTitle.Text = "Employee Management";
            _lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            _lblTitle.ForeColor = Color.FromArgb(30, 41, 59);
            _lblTitle.AutoSize = true;
            _lblTitle.Location = new Point(15, 12);

            // _lblDesc
            _lblDesc.Text = "Add, edit and manage staff";
            _lblDesc.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            _lblDesc.ForeColor = Color.FromArgb(100, 116, 139);
            _lblDesc.AutoSize = true;
            _lblDesc.Location = new Point(15, 42);

            // _btnAddEmployee
            _btnAddEmployee.Text = "Add Employee";
            _btnAddEmployee.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            _btnAddEmployee.BackColor = Color.Black;
            _btnAddEmployee.ForeColor = Color.White;
            _btnAddEmployee.FlatStyle = FlatStyle.Flat;
            _btnAddEmployee.FlatAppearance.BorderSize = 0;
            _btnAddEmployee.Size = new Size(150, 34);
            _btnAddEmployee.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnAddEmployee.Location = new Point(_headerPanel.Width - _btnAddEmployee.Width - 15, 28);

            // keep button right-aligned when header panel resizes
            _headerPanel.Resize += (_, __) =>
            {
                _btnAddEmployee.Location = new Point(_headerPanel.ClientSize.Width - _btnAddEmployee.Width - 15, 28);
            };

            _headerPanel.Controls.AddRange(new Control[]
            {
                _lblTitle, _lblDesc, _btnAddEmployee
            });

            // _dgvEmployees
            _dgvEmployees.Location = new Point(20, _headerPanel.Bottom + 12);
            _dgvEmployees.Size = new Size(Width - 40, Height - _headerPanel.Bottom - 32);
            _dgvEmployees.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            _dgvEmployees.BackgroundColor = Color.White;
            _dgvEmployees.BorderStyle = BorderStyle.None;
            _dgvEmployees.ReadOnly = true;
            _dgvEmployees.AllowUserToAddRows = false;
            _dgvEmployees.AllowUserToDeleteRows = false;
            _dgvEmployees.AllowUserToResizeRows = false;
            _dgvEmployees.RowHeadersVisible = false;
            _dgvEmployees.MultiSelect = false;
            _dgvEmployees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvEmployees.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvEmployees.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            _dgvEmployees.EnableHeadersVisualStyles = false;
            _dgvEmployees.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(243, 244, 246);
            _dgvEmployees.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            _dgvEmployees.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            _dgvEmployees.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            _dgvEmployees.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            _dgvEmployees.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            _dgvEmployees.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            _dgvEmployees.GridColor = Color.FromArgb(215, 220, 230);
            _dgvEmployees.AllowUserToOrderColumns = false;
            _dgvEmployees.AutoGenerateColumns = false;

            // Data columns (same order as before)
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "EmployeeID", FillWeight = 60, Name = "colId" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", DataPropertyName = "Name", FillWeight = 140, Name = "colName" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = "Email", FillWeight = 160, Name = "colEmail" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phone", DataPropertyName = "PhoneNumber", FillWeight = 120, Name = "colPhone" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Username", DataPropertyName = "Username", FillWeight = 120, Name = "colUsername" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Role", DataPropertyName = "Role", FillWeight = 90, Name = "colRole" });

            var hireCol = new DataGridViewTextBoxColumn { HeaderText = "Hire Date", DataPropertyName = "HireDate", FillWeight = 110, Name = "colHireDate" };
            hireCol.DefaultCellStyle.Format = "yyyy-MM-dd";
            _dgvEmployees.Columns.Add(hireCol);

            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "StatusText", FillWeight = 90, Name = "colStatusText" });

            // Add to control
            Controls.Add(_headerPanel);
            Controls.Add(_dgvEmployees);

            ResumeLayout(false);
        }
    }
}
