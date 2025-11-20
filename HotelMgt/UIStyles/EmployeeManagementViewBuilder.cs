using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using HotelMgt.Custom; // RoundedPanel, RoundedButton
using WinFormsTimer = System.Windows.Forms.Timer; // <-- disambiguate Timer

namespace HotelMgt.UIStyles
{
    public static class EmployeeManagementViewBuilder
    {
        // Action column names (exported for consumers if they need by-name lookup)
        public const string ColEdit = "colEdit";
        public const string ColStatus = "colStatus";
        public const string ColRemove = "colRemove";

        public static void Build(
            Control parent,
            out RoundedPanel headerPanel,
            out Label lblTitle,
            out Label lblDesc,
            out Button btnAddEmployee,
            out DataGridView dgvEmployees)
        {
            parent.SuspendLayout();
            parent.Controls.Clear();
            parent.BackColor = Color.FromArgb(244, 246, 250);
            parent.Dock = DockStyle.Fill;

            // Root container (match Room)
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(244, 246, 250)
            };
            parent.Controls.Add(container);

            // Grid layout: Row 0 = header (fixed 90px), Row 1 = grid (100%)
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(244, 246, 250),
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10, 10, 10, 10) // 10px margin all around
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90f));   // header height
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));   // grid fills remainder
            container.Controls.Add(layout);

            // Header (rounded, full-width)
            headerPanel = new RoundedPanel
            {
                BorderRadius = 12,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 12) // space under header
            };
            AttachRoundedBorder(headerPanel, 12, Color.FromArgb(215, 220, 230));
            layout.Controls.Add(headerPanel, 0, 0);

            lblTitle = new Label
            {
                Text = "Employee Management",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(15, 12)
            };
            headerPanel.Controls.Add(lblTitle);

            lblDesc = new Label
            {
                Text = "Add, edit and manage staff",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Location = new Point(15, 42)
            };
            headerPanel.Controls.Add(lblDesc);

            // Use your RoundedButton (match Room button styling)
            btnAddEmployee = new RoundedButton
            {
                Text = "Add Employee",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.Black,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 34),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnAddEmployee.FlatAppearance.BorderSize = 0;
            headerPanel.Controls.Add(btnAddEmployee);

            // Avoid capturing out params inside lambdas: copy to locals first
            var headerLocal = headerPanel;
            var titleLocal = lblTitle;
            var descLocal = lblDesc;
            var addBtnLocal = btnAddEmployee;
            headerLocal.Resize += (_, __) => PositionAddButton(headerLocal, titleLocal, descLocal, addBtnLocal);
            PositionAddButton(headerLocal, titleLocal, descLocal, addBtnLocal);

            // FORCE ROUNDED BUTTON SHAPE (and ensure custom BackColor is used)
            addBtnLocal.UseVisualStyleBackColor = false;                  // << added
            ApplyRounded(addBtnLocal, 16);                                // << added

            // Grid (fills remaining space; internal scrollbars handle overflow)
            dgvEmployees = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true
            };
            layout.Controls.Add(dgvEmployees, 0, 1);

            // Apply the same table look/feel as RoomManagement
            ApplyEmployeeTableStyle(dgvEmployees);
            EnableDoubleBuffer(dgvEmployees);

            // Columns (keep Employee fields; left-aligned; adjusted widths)
            dgvEmployees.Columns.Clear();
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmployeeID", HeaderText = "ID", DataPropertyName = "EmployeeID", FillWeight = 60, MinimumWidth = 50 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "Name", DataPropertyName = "Name", FillWeight = 150, MinimumWidth = 120 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmail", HeaderText = "Email", DataPropertyName = "Email", FillWeight = 170, MinimumWidth = 140 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPhone", HeaderText = "Phone", DataPropertyName = "PhoneNumber", FillWeight = 120, MinimumWidth = 110 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "colUsername", HeaderText = "Username", DataPropertyName = "Username", FillWeight = 120, MinimumWidth = 110 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRole", HeaderText = "Role", DataPropertyName = "Role", FillWeight = 90, MinimumWidth = 80 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "colHireDate", HeaderText = "Hire Date", DataPropertyName = "HireDate", FillWeight = 110, MinimumWidth = 100, DefaultCellStyle = { Format = "yyyy-MM-dd" } });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatusText", HeaderText = "Status", DataPropertyName = "StatusText", FillWeight = 95, MinimumWidth = 80 });

            // Two visible action columns (mirror Room design). Keep Remove hidden by default.
            var colEdit = new DataGridViewButtonColumn
            {
                Name = ColEdit,
                HeaderText = string.Empty,
                Text = "Edit",
                UseColumnTextForButtonValue = true,
                FillWeight = 50,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                ReadOnly = true,
                FlatStyle = FlatStyle.Standard
            };
            var colStatus = new DataGridViewButtonColumn
            {
                Name = ColStatus,
                HeaderText = string.Empty,
                Text = "Status",
                UseColumnTextForButtonValue = true,
                FillWeight = 70,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                ReadOnly = true,
                FlatStyle = FlatStyle.Standard
            };
            var colRemove = new DataGridViewButtonColumn
            {
                Name = ColRemove,
                HeaderText = string.Empty,
                Text = "Remove",
                UseColumnTextForButtonValue = true,
                FillWeight = 70,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                ReadOnly = true,
                FlatStyle = FlatStyle.Standard,
                Visible = false
            };
            dgvEmployees.Columns.Add(colEdit);
            dgvEmployees.Columns.Add(colStatus);
            dgvEmployees.Columns.Add(colRemove);

            // Merged "Actions" header like Room
            HookMergedActionsHeader(dgvEmployees);

            parent.ResumeLayout(performLayout: true);
        }

        public static Form BuildAddEmployeeDialog(
            out TextBox txtFirstName,
            out TextBox txtLastName,
            out TextBox txtEmail,
            out TextBox txtPhone,
            out TextBox txtUsername,
            out TextBox txtPassword,
            out ComboBox cboRole,
            out DateTimePicker dtpHireDate,
            out Button btnSubmit,
            out Button btnCancel)
        {
            var dlg = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White,
                Size = new Size(560, 520), // match Room dialog size
                ShowInTaskbar = false,
                TopMost = false,
                KeyPreview = true
            };

            ApplyRounded(dlg, 12);
            AttachRoundedBorder(dlg, 12, Color.Black);

            var lblTitle = new Label
            {
                Text = "Add New Employee",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(20, 18)
            };
            dlg.Controls.Add(lblTitle);

            var lblDesc = new Label
            {
                Text = "Enter employee information to create a new account",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Location = new Point(20, 48)
            };
            dlg.Controls.Add(lblDesc);

            // Close (X) button as RoundedButton
            var btnClose = new RoundedButton
            {
                Text = "✕",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(dlg.ClientSize.Width - 30 - 10, 10),
                TabStop = false,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            dlg.Controls.Add(btnClose);

            int y = 82; // align with Room dialog spacing
            dlg.Controls.Add(new Label { Text = "First Name *", Font = new Font("Segoe UI", 9), Location = new Point(20, y), AutoSize = true });
            txtFirstName = new TextBox { Location = new Point(20, y + 18), Size = new Size(230, 25), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            dlg.Controls.Add(txtFirstName);

            dlg.Controls.Add(new Label { Text = "Last Name *", Font = new Font("Segoe UI", 9), Location = new Point(270, y), AutoSize = true });
            txtLastName = new TextBox { Location = new Point(270, y + 18), Size = new Size(230, 25), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            dlg.Controls.Add(txtLastName);

            y += 58;
            dlg.Controls.Add(new Label { Text = "Email *", Font = new Font("Segoe UI", 9), Location = new Point(20, y), AutoSize = true });
            txtEmail = new TextBox { Location = new Point(20, y + 18), Size = new Size(230, 25), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            dlg.Controls.Add(txtEmail);

            dlg.Controls.Add(new Label { Text = "Phone Number *", Font = new Font("Segoe UI", 9), Location = new Point(270, y), AutoSize = true });
            txtPhone = new TextBox { Location = new Point(270, y + 18), Size = new Size(230, 25), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            dlg.Controls.Add(txtPhone);

            y += 58;
            dlg.Controls.Add(new Label { Text = "Username *", Font = new Font("Segoe UI", 9), Location = new Point(20, y), AutoSize = true });
            txtUsername = new TextBox { Location = new Point(20, y + 18), Size = new Size(230, 25), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            dlg.Controls.Add(txtUsername);

            dlg.Controls.Add(new Label { Text = "Role *", Font = new Font("Segoe UI", 9), Location = new Point(270, y), AutoSize = true });
            cboRole = new ComboBox
            {
                Location = new Point(270, y + 18),
                Size = new Size(230, 25),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboRole.Items.AddRange(new object[] { "Employee", "Admin" });
            dlg.Controls.Add(cboRole);

            y += 58;
            dlg.Controls.Add(new Label { Text = "Password *", Font = new Font("Segoe UI", 9), Location = new Point(20, y), AutoSize = true });
            txtPassword = new TextBox
            {
                Location = new Point(20, y + 18),
                Size = new Size(230, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };
            dlg.Controls.Add(txtPassword);

            // Pair Hire Date on the right side of the Password row
            dlg.Controls.Add(new Label { Text = "Hire Date *", Font = new Font("Segoe UI", 9), Location = new Point(270, y), AutoSize = true });
            dtpHireDate = new DateTimePicker
            {
                Location = new Point(270, y + 18),
                Size = new Size(230, 25),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            dlg.Controls.Add(dtpHireDate);

            // Buttons (RoundedButton, match Room sizing/positioning)
            btnSubmit = new RoundedButton
            {
                Text = "Add Employee",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.Black,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 34),
                Location = new Point(dlg.ClientSize.Width - 140 - 110, dlg.ClientSize.Height - 54),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.DialogResult = DialogResult.OK;
            dlg.Controls.Add(btnSubmit);

            btnCancel = new RoundedButton
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 34),
                Location = new Point(dlg.ClientSize.Width - 100 - 20, dlg.ClientSize.Height - 54),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 230);
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.DialogResult = DialogResult.Cancel;
            dlg.Controls.Add(btnCancel);

            // FORCE ROUNDED SHAPE ON DIALOG BUTTONS
            btnClose.UseVisualStyleBackColor = false; ApplyRounded(btnClose, 15);
            btnSubmit.UseVisualStyleBackColor = false; ApplyRounded(btnSubmit, 16);
            btnCancel.UseVisualStyleBackColor = false; ApplyRounded(btnCancel, 16);

            // Close button behaves like Cancel
            btnClose.Click += (_, __) =>
            {
                dlg.DialogResult = DialogResult.Cancel;
                dlg.Close();
            };

            // ESC behaves like Cancel
            dlg.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    dlg.DialogResult = DialogResult.Cancel;
                    dlg.Close();
                }
            };

            // Keep buttons aligned on resize (match Room)
            var submitLocal = btnSubmit;
            var cancelLocal = btnCancel;
            dlg.Resize += (_, __) =>
            {
                cancelLocal.Location = new Point(dlg.ClientSize.Width - cancelLocal.Width - 20, dlg.ClientSize.Height - 54);
                submitLocal.Location = new Point(cancelLocal.Left - submitLocal.Width - 20, dlg.ClientSize.Height - 54);
            };

            // NOTE: No dialog-owned overlay here. Overlay is handled in ShowDialogOnOwner (like Room).
            return dlg;
        }

        public static void ConfigureActionColumns(DataGridView grid)
        {
            if (grid is null) return;
            var cols = grid.Columns;

            // Remove legacy single "Action" column if present (by header text)
            DataGridViewColumn? legacyAction = null;
            foreach (DataGridViewColumn c in cols)
            {
                if (string.Equals(c.HeaderText, "Action", StringComparison.OrdinalIgnoreCase))
                {
                    legacyAction = c;
                    break;
                }
            }
            if (legacyAction != null)
                cols.Remove(legacyAction);

            // Edit (mirror Room)
            if (!cols.Contains(ColEdit))
            {
                cols.Add(new DataGridViewButtonColumn
                {
                    Name = ColEdit,
                    HeaderText = string.Empty, // merged header will render "Actions"
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    FillWeight = 50,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    ReadOnly = true,
                    FlatStyle = FlatStyle.Standard
                });
            }
            else if (cols[ColEdit] is DataGridViewButtonColumn editBtn)
            {
                editBtn.HeaderText = string.Empty;
                editBtn.Text = "Edit";
                editBtn.UseColumnTextForButtonValue = true;
                editBtn.SortMode = DataGridViewColumnSortMode.NotSortable;
                editBtn.ReadOnly = true;
                editBtn.FillWeight = 50;
                editBtn.FlatStyle = FlatStyle.Standard;
            }

            // Status (second visible action column)
            if (!cols.Contains(ColStatus))
            {
                cols.Add(new DataGridViewButtonColumn
                {
                    Name = ColStatus,
                    HeaderText = string.Empty,
                    Text = "Status",
                    UseColumnTextForButtonValue = true,
                    FillWeight = 70,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    ReadOnly = true,
                    FlatStyle = FlatStyle.Standard
                });
            }
            else if (cols[ColStatus] is DataGridViewButtonColumn statusBtn)
            {
                statusBtn.HeaderText = string.Empty;
                statusBtn.Text = "Status";
                statusBtn.UseColumnTextForButtonValue = true;
                statusBtn.SortMode = DataGridViewColumnSortMode.NotSortable;
                statusBtn.ReadOnly = true;
                statusBtn.FillWeight = 70;
                statusBtn.FlatStyle = FlatStyle.Standard;
            }

            // Remove (keep hidden to mirror Room's two-action layout)
            if (cols.Contains(ColRemove) && cols[ColRemove] is DataGridViewButtonColumn removeBtnExisting)
            {
                removeBtnExisting.FlatStyle = FlatStyle.Standard;
                removeBtnExisting.Visible = false;
            }
        }

        private static void PositionAddButton(Control panel, Control title, Control desc, Control button)
        {
            int rightPadding = 15;
            int centerY = title.Top + (desc.Bottom - title.Top) / 2;
            button.Location = new Point(panel.ClientSize.Width - rightPadding - button.Width, centerY - button.Height / 2);
        }

        private static void AttachRoundedBorder(Control ctrl, int radius, Color borderColor)
        {
            ctrl.Paint -= Control_DrawRoundedBorder;
            ctrl.Paint += Control_DrawRoundedBorder;

            void Control_DrawRoundedBorder(object? sender, PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(borderColor, 1f);
                var rect = ctrl.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var path = GetRoundedRectPath(rect, radius);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private static void ApplyRounded(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0)
            {
                control.HandleCreated += (_, __) =>
                {
                    using var path2 = GetRoundedRectPath(new Rectangle(Point.Empty, control.Size), radius);
                    control.Region = new Region(path2);
                };
                return;
            }
            using var path = GetRoundedRectPath(new Rectangle(Point.Empty, control.Size), radius);
            control.Region = new Region(path);
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            var arc = new Rectangle(rect.X, rect.Y, d, d);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - d;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - d;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ---------- Designer helpers (match Room) ----------

        // Merged "Actions" header painter (idempotent hook)
        public static void HookMergedActionsHeader(DataGridView grid)
        {
            if (grid is null) return;

            grid.Paint -= Grid_PaintMergedActionsHeader;
            grid.Paint += Grid_PaintMergedActionsHeader;

            void InvalidateHeader(object? s, EventArgs e) => grid.Invalidate(grid.DisplayRectangle);

            grid.Scroll -= InvalidateHeader;
            grid.ColumnWidthChanged -= InvalidateHeader;
            grid.SizeChanged -= InvalidateHeader;
            grid.ColumnDisplayIndexChanged -= InvalidateHeader;

            grid.Scroll += InvalidateHeader;
            grid.ColumnWidthChanged += InvalidateHeader;
            grid.SizeChanged += InvalidateHeader;
            grid.ColumnDisplayIndexChanged += InvalidateHeader;
        }

        private static void Grid_PaintMergedActionsHeader(object? sender, PaintEventArgs e)
        {
            if (sender is not DataGridView grid) return;

            var cols = grid.Columns;
            if (!cols.Contains(ColEdit) || !cols.Contains(ColStatus))
                return;

            var colEdit = cols[ColEdit];
            var colStatus = cols[ColStatus];

            if (!colEdit.Visible || !colStatus.Visible) return;

            var rectEdit = grid.GetCellDisplayRectangle(colEdit.Index, -1, true);
            var rectStatus = grid.GetCellDisplayRectangle(colStatus.Index, -1, true);

            if (rectEdit.Width <= 0 || rectStatus.Width <= 0) return;

            var mergedRect = Rectangle.FromLTRB(rectEdit.Left, rectEdit.Top, rectStatus.Right, rectEdit.Bottom);
            mergedRect.Inflate(-1, -1);

            var headerStyle = grid.ColumnHeadersDefaultCellStyle;
            var backColor = headerStyle.BackColor.IsEmpty ? SystemColors.Control : headerStyle.BackColor;
            var foreColor = headerStyle.ForeColor.IsEmpty ? SystemColors.ControlText : headerStyle.ForeColor;
            var font = headerStyle.Font ?? grid.Font;

            using var backBrush = new SolidBrush(backColor);
            using var textBrush = new SolidBrush(foreColor);
            using var borderPen = new Pen(grid.GridColor);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            e.Graphics.FillRectangle(backBrush, mergedRect);
            e.Graphics.DrawRectangle(borderPen, mergedRect);
            e.Graphics.DrawString("Actions", font, textBrush, mergedRect, sf);
        }

        // ---------- Protected modal show + click shield (match Room) ----------

        public static void ShowDialogOnOwner(Control context, Form dlg)
        {
            var childForm = context?.FindForm();
            var ownerForm = childForm?.MdiParent ?? childForm;

            Form? overlay = null;
            EventHandler? syncOverlay = null;
            EventHandler? ownerActivated = null;
            EventHandler? dlgDeactivate = null;

            try
            {
                dlg.ShowInTaskbar = false;
                dlg.StartPosition = ownerForm is not null
                    ? FormStartPosition.CenterParent
                    : FormStartPosition.CenterScreen;

                // Keep dialog above overlay (but not above other apps)
                dlg.TopMost = true;

                dlg.Shown += (_, __) =>
                {
                    dlg.Activate();
                    dlg.BringToFront();
                };

                dlgDeactivate = (_, __) =>
                {
                    if (!dlg.Visible) return;
                    dlg.BeginInvoke(new Action(() =>
                    {
                        dlg.BringToFront();
                        dlg.Activate();
                    }));
                };
                dlg.Deactivate += dlgDeactivate;

                using var shield = new ModalClickShield(dlg);

                if (ownerForm is not null)
                {
                    // Create the gray overlay that tracks the owner
                    overlay = new Form
                    {
                        FormBorderStyle = FormBorderStyle.None,
                        ShowInTaskbar = false,
                        StartPosition = FormStartPosition.Manual,
                        BackColor = Color.Black,
                        Opacity = 0.35,
                        Bounds = ownerForm.Bounds,
                        Owner = ownerForm,
                        TopMost = false
                        // Enabled = false // removed to avoid InvalidOperationException when showing a disabled form
                    };

                    overlay.Show(ownerForm);

                    void Sync(object? s, EventArgs e)
                    {
                        if (overlay is { IsDisposed: false })
                        {
                            overlay.Bounds = ownerForm.Bounds;
                        }
                    }

                    // Keep overlay aligned with its owner
                    syncOverlay = Sync;
                    ownerForm.Move += Sync;
                    ownerForm.SizeChanged += Sync;
                    ownerForm.LocationChanged += Sync;

                    ownerActivated = (_, __) =>
                    {
                        if (dlg.Visible)
                        {
                            dlg.BeginInvoke(new Action(() =>
                            {
                                dlg.BringToFront();
                                dlg.Activate();
                            }));
                        }
                    };
                    ownerForm.Activated += ownerActivated;

                    // Ensure overlay cleanup on dialog close
                    dlg.FormClosed += (_, __) =>
                    {
                        if (ownerForm != null && syncOverlay != null)
                        {
                            ownerForm.Move -= syncOverlay;
                            ownerForm.SizeChanged -= syncOverlay;
                            ownerForm.LocationChanged -= syncOverlay;
                        }

                        overlay?.Close();
                        overlay?.Dispose();
                        overlay = null;
                    };

                    dlg.ShowDialog(ownerForm);
                }
                else
                {
                    dlg.ShowDialog();
                }
            }
            finally
            {
                if (ownerForm is not null && ownerActivated is not null)
                    ownerForm.Activated -= ownerActivated;
                if (dlgDeactivate is not null)
                    dlg.Deactivate -= dlgDeactivate;

                // Fallback cleanup if exception occurred before FormClosed ran
                if (overlay is not null)
                {
                    if (ownerForm != null && syncOverlay != null)
                    {
                        ownerForm.Move -= syncOverlay;
                        ownerForm.SizeChanged -= syncOverlay;
                        ownerForm.LocationChanged -= syncOverlay;
                    }
                    overlay.Close();
                    overlay.Dispose();
                    overlay = null;
                }

                dlg.Dispose();
            }
        }

        // ---------- Quick toast helpers (match Room positioning) ----------

        public static void ShowToast(Control context, string message = "Operation Successful", int durationMs = 1000)
        {
            var ownerForm = context?.FindForm()?.MdiParent ?? context?.FindForm() ?? Form.ActiveForm;

            var toast = new ToastPopup(message, durationMs);
            toast.FormClosed += (_, __) => toast.Dispose();

            if (ownerForm is not null)
            {
                var size = toast.GetPreferredSize(Size.Empty);
                var clientTopRight = new Point(ownerForm.ClientSize.Width - size.Width - 10, 10);
                var screenPoint = ownerForm.PointToScreen(clientTopRight);
                toast.Location = screenPoint;
                toast.Show(ownerForm);
            }
            else
            {
                var wa = Screen.PrimaryScreen?.WorkingArea ?? Screen.GetWorkingArea(Point.Empty);
                var size = toast.GetPreferredSize(Size.Empty);
                toast.Location = new Point(wa.Right - size.Width - 10, wa.Top + 10);
                toast.Show();
            }
        }

        // Queue toast to appear after modal is closed (owner re-enabled)
        public static void ShowToastAfterDialogClose(Control context, Form dlg, string message = "Operation Successful", int durationMs = 1000)
        {
            void Handler(object? s, FormClosedEventArgs e)
            {
                dlg.FormClosed -= Handler;
                (context ?? (Control)dlg).BeginInvoke(new Action(() => ShowToast(context ?? dlg, message, durationMs)));
            }
            dlg.FormClosed += Handler;
        }

        // Non-activating, top-most toast window (consistent with Room)
        private sealed class ToastPopup : Form
        {
            private readonly WinFormsTimer _timer;
            private readonly string _message;
            private readonly Font _textFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            private readonly Padding _pad;
            private const int MaxTextWidth = 320; // px
            private const int CornerRadius = 10;  // px

            public ToastPopup(string message, int durationMs)
            {
                _message = $"✔  {message}";
                _pad = ScalePadding(new Padding(14, 10, 16, 12));

                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                TopMost = true;
                StartPosition = FormStartPosition.Manual;
                DoubleBuffered = true;
                BackColor = Color.FromArgb(36, 36, 36);
                ForeColor = Color.White;
                Padding = _pad;
                Opacity = 0.98;

                var label = new Label
                {
                    Dock = DockStyle.Fill,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    ForeColor = Color.White,
                    Text = _message,
                    Font = _textFont,
                    Padding = new Padding(0)
                };
                Controls.Add(label);

                ClientSize = GetPreferredSize(Size.Empty);

                _timer = new WinFormsTimer { Interval = Math.Max(250, durationMs) };
                _timer.Tick += (_, __) => { _timer.Stop(); Close(); };
            }

            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    const int WS_EX_TOPMOST = 0x00000008;
                    const int WS_EX_TOOLWINDOW = 0x00000080;
                    const int WS_EX_NOACTIVATE = 0x08000000;
                    var cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_TOPMOST | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                    return cp;
                }
            }

            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);
                UpdateWindowRegion();
                _timer.Start();
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);
                UpdateWindowRegion();
                Invalidate();
            }

            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                try { _timer.Stop(); _timer.Dispose(); } finally { base.OnFormClosed(e); }
            }

            // Ensure the caller can position using toast.PreferredSize before Show()
            public override Size GetPreferredSize(Size proposedSize)
            {
                var textSize = TextRenderer.MeasureText(
                    _message,
                    _textFont,
                    new Size(ScaleInt(MaxTextWidth), int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

                return new Size(textSize.Width + _pad.Horizontal, textSize.Height + _pad.Vertical);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var rect = ClientRectangle;
                if (rect.Width <= 0 || rect.Height <= 0) return;
                rect.Inflate(-1, -1);

                using (var shadow = BuildRoundedPath(new Rectangle(rect.X + ScaleInt(2), rect.Y + ScaleInt(3), rect.Width, rect.Height), CornerRadius + 1))
                using (var shadowBrush = new SolidBrush(Color.FromArgb(85, 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, shadow);
                }

                using var path = BuildRoundedPath(rect, CornerRadius);
                using var baseBrush = new SolidBrush(Color.FromArgb(40, 40, 40));
                using var overlay = new SolidBrush(Color.FromArgb(150, 20, 20, 20));
                using var borderPen = new Pen(Color.FromArgb(185, 185, 185), 1f);

                g.FillPath(baseBrush, path);
                g.FillPath(overlay, path);
                g.DrawPath(borderPen, path);
            }

            private void UpdateWindowRegion()
            {
                var rect = ClientRectangle;
                if (rect.Width <= 0 || rect.Height <= 0) return;
                rect.Inflate(-1, -1);
                using var path = BuildRoundedPath(rect, CornerRadius);
                Region?.Dispose();
                Region = new Region(path);
            }

            // Helpers
            private static Padding ScalePadding(Padding p)
            {
                using var g = Graphics.FromHwnd(IntPtr.Zero);
                var s = g.DpiX / 96f;
                return new Padding(
                    (int)Math.Round(p.Left * s),
                    (int)Math.Round(p.Top * s),
                    (int)Math.Round(p.Right * s),
                    (int)Math.Round(p.Bottom * s));
            }

            private static int ScaleInt(int v)
            {
                using var g = Graphics.FromHwnd(IntPtr.Zero);
                return (int)Math.Round(v * (g.DpiX / 96f));
            }

            private static GraphicsPath BuildRoundedPath(Rectangle r, int radius)
            {
                var path = new GraphicsPath();
                int d = radius * 2;
                var arc = new Rectangle(r.X, r.Y, d, d);
                path.AddArc(arc, 180, 90);       // TL
                arc.X = r.Right - d;
                path.AddArc(arc, 270, 90);       // TR
                arc.Y = r.Bottom - d;
                path.AddArc(arc, 0, 90);         // BR
                arc.X = r.Left;
                path.AddArc(arc, 90, 90);        // BL
                path.CloseFigure();
                return path;
            }
        }

        // ---------- Pause shield for MessageBoxes (kept) ----------

        [ThreadStatic] private static int _pauseDepth;
        public static IDisposable PauseShield() { _pauseDepth++; return new PauseCookie(); }
        private sealed class PauseCookie : IDisposable { public void Dispose() => _pauseDepth = Math.Max(0, _pauseDepth - 1); }

        // Unified table style to match RoomManagement look (centralized)
        private static void ApplyEmployeeTableStyle(DataGridView grid)
        {
            GridTheme.ApplyStandard(grid);
        }

        // Reduce flicker when resizing/scrolling (protected DoubleBuffered)
        private static void EnableDoubleBuffer(DataGridView grid)
        {
            var prop = typeof(Control).GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            prop?.SetValue(grid, true, null);
        }

        // Owner-aware click shield (match Room)
        private sealed class ModalClickShield : IMessageFilter, IDisposable
        {
            private readonly Form _dlg;
            private const int WM_LBUTTONDOWN = 0x0201;
            private const int WM_RBUTTONDOWN = 0x0204;
            private const int WM_MBUTTONDOWN = 0x0207;
            private const int WM_NCLBUTTONDOWN = 0x00A1;
            private const int WM_NCRBUTTONDOWN = 0x00A4;
            private const int WM_NCMBUTTONDOWN = 0x00A7;

            private const uint GA_ROOT = 2;
            private const uint GW_OWNER = 4;

            [DllImport("user32.dll")] private static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);
            [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

            public ModalClickShield(Form dlg)
            {
                _dlg = dlg;
                Application.AddMessageFilter(this);
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (!_dlg.Visible) return false;

                if (m.Msg != WM_LBUTTONDOWN && m.Msg != WM_RBUTTONDOWN && m.Msg != WM_MBUTTONDOWN &&
                    m.Msg != WM_NCLBUTTONDOWN && m.Msg != WM_NCRBUTTONDOWN && m.Msg != WM_NCMBUTTONDOWN)
                    return false;

                if (IsOwnedByDialogFamily(m.HWnd))
                    return false;

                _dlg.BeginInvoke(new Action(() =>
                {
                    _dlg.BringToFront();
                    _dlg.Activate();
                }));
                return true;
            }

            private bool IsOwnedByDialogFamily(IntPtr target)
            {
                if (target == IntPtr.Zero) return false;

                var top = GetAncestor(target, GA_ROOT);
                if (top == IntPtr.Zero) return false;

                var w = top;
                while (w != IntPtr.Zero)
                {
                    if (w == _dlg.Handle) return true;
                    w = GetWindow(w, GW_OWNER);
                }
                return false;
            }

            public void Dispose()
            {
                Application.RemoveMessageFilter(this);
            }
        }
    }
}