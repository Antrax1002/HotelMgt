using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using HotelMgt.Custom;
using WinFormsTimer = System.Windows.Forms.Timer;

namespace HotelMgt.UIStyles
{
    public static class RoomManagementViewBuilder
    {
        public const string ColEdit = "colEdit";
        public const string ColMaintenance = "colMaintenance";

        // Replace only the Build(...) method
        public static void Build(
            Control parent,
            out RoundedPanel headerPanel,
            out Label lblTitle,
            out Label lblDesc,
            out Button btnAddRoom,
            out DataGridView dgvRooms)
        {
            parent.SuspendLayout();
            parent.Controls.Clear();
            parent.BackColor = Color.FromArgb(244, 246, 250);
            parent.Dock = DockStyle.Fill;

            // --- SCROLLABLE ROOT PANEL ---
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(244, 246, 250)
            };
            parent.Controls.Add(scrollPanel);

            // Root container (for padding, not autosize)
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(244, 246, 250)
            };
            scrollPanel.Controls.Add(container);

            // Grid layout: Row 0 = header (fixed 90px), Row 1 = grid (100%)
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(244, 246, 250),
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10, 10, 10, 10)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90f));   // header height
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));   // grid fills remainder
            container.Controls.Add(layout);

            // Header
            headerPanel = new RoundedPanel
            {
                BorderRadius = 12,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 12)
            };
            AttachRoundedBorder(headerPanel, 12, Color.FromArgb(215, 220, 230));
            layout.Controls.Add(headerPanel, 0, 0);

            lblTitle = new Label
            {
                Text = "Room Management",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(15, 12)
            };
            headerPanel.Controls.Add(lblTitle);

            lblDesc = new Label
            {
                Text = "Add, edit and manage rooms",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Location = new Point(15, 42)
            };
            headerPanel.Controls.Add(lblDesc);

            // Rounded header button (match Employee look)
            btnAddRoom = new RoundedButton
            {
                Text = "Add Room",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.Black,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 34),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                UseVisualStyleBackColor = false
            };
            btnAddRoom.FlatAppearance.BorderSize = 0;
            headerPanel.Controls.Add(btnAddRoom);

            var headerLocal = headerPanel;
            var titleLocal = lblTitle;
            var descLocal = lblDesc;
            var addBtnLocal = btnAddRoom;
            headerLocal.Resize += (_, __) => PositionAddButton(headerLocal, titleLocal, descLocal, addBtnLocal);
            PositionAddButton(headerLocal, titleLocal, descLocal, addBtnLocal);
            ApplyRounded(addBtnLocal, 16);

            // Grid
            dgvRooms = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true
            };
            layout.Controls.Add(dgvRooms, 0, 1);

            ApplyEmployeeTableStyle(dgvRooms);
            EnableDoubleBuffer(dgvRooms);

            // Columns
            dgvRooms.Columns.Clear();
            var colRoomID = new DataGridViewTextBoxColumn { Name = "colRoomID", HeaderText = "ID", DataPropertyName = "RoomID", FillWeight = 60, MinimumWidth = 50, Resizable = DataGridViewTriState.False };
            var colRoomNumber = new DataGridViewTextBoxColumn
            {
                Name = "colRoomNumber",
                HeaderText = "Number",
                DataPropertyName = "RoomNumber",
                FillWeight = 95, // reduced from 115
                MinimumWidth = 70, // reduced from 90
                Resizable = DataGridViewTriState.False
            };
            var colRoomType = new DataGridViewTextBoxColumn
            {
                Name = "colRoomType",
                HeaderText = "Type",
                DataPropertyName = "RoomType",
                FillWeight = 110, // reduced from 140
                MinimumWidth = 80, // reduced from 100
                Resizable = DataGridViewTriState.False
            };
            var colFloor = new DataGridViewTextBoxColumn { Name = "colFloor", HeaderText = "Floor", DataPropertyName = "Floor", FillWeight = 75, MinimumWidth = 60, Resizable = DataGridViewTriState.False };
            var colPriceText = new DataGridViewTextBoxColumn { Name = "colPriceText", HeaderText = "Price/Night", DataPropertyName = "PriceText", FillWeight = 130, MinimumWidth = 110, Resizable = DataGridViewTriState.False };
            var colMaxOccupancy = new DataGridViewTextBoxColumn { Name = "colMaxOccupancy", HeaderText = "Max", DataPropertyName = "MaxOccupancy", FillWeight = 75, MinimumWidth = 60, Resizable = DataGridViewTriState.False };
            var colStatus = new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 105, MinimumWidth = 90, Resizable = DataGridViewTriState.False };

            dgvRooms.Columns.AddRange(new DataGridViewColumn[] {
                colRoomID, colRoomNumber, colRoomType, colFloor, colPriceText, colMaxOccupancy, colStatus
            });

            // Action columns (narrowed)
            var colEdit = new DataGridViewButtonColumn
            {
                Name = ColEdit,
                HeaderText = string.Empty,
                Text = "Edit",
                UseColumnTextForButtonValue = true,
                FillWeight = 50,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False
            };
            var colMaintenance = new DataGridViewButtonColumn
            {
                Name = ColMaintenance,
                HeaderText = string.Empty,
                Text = "Maintenance",
                UseColumnTextForButtonValue = true,
                FillWeight = 70,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False
            };
            dgvRooms.Columns.Add(colEdit);
            dgvRooms.Columns.Add(colMaintenance);

            HookMergedActionsHeader(dgvRooms);

            var gridForHandler = dgvRooms;
            gridForHandler.ColumnHeaderMouseClick += (s, e) => gridForHandler.ClearSelection();

            parent.ResumeLayout(performLayout: true);
        }

        public static Form BuildAddRoomDialog(
            out TextBox txtRoomNumber,
            out NumericUpDown nudFloor,
            out ComboBox cboRoomType,
            out NumericUpDown nudPrice,
            out NumericUpDown nudMaxGuests,
            out TextBox txtAmenities,
            out TextBox txtDescription,
            out Button btnSubmit,
            out Button btnCancel)
        {
            var dlg = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White,
                Size = new Size(560, 520),
                ShowInTaskbar = false,
                TopMost = false,
                KeyPreview = true
            };

            ApplyRounded(dlg, 12);
            AttachRoundedBorder(dlg, 12, Color.Black);

            var lblTitle = new Label
            {
                Text = "Add New Room",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(20, 18)
            };
            dlg.Controls.Add(lblTitle);

            var lblDesc = new Label
            {
                Text = "Enter room information to create a new room",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Location = new Point(20, 48)
            };
            dlg.Controls.Add(lblDesc);

            var btnClose = new Button
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
            ApplyRounded(btnClose, 15);
            dlg.Controls.Add(btnClose);

            int y = 82;
            dlg.Controls.Add(new Label { Text = "Room Number *", Font = new Font("Segoe UI", 9), Location = new Point(20, y), AutoSize = true });
            txtRoomNumber = new TextBox { Location = new Point(20, y + 18), Size = new Size(200, 25), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            dlg.Controls.Add(txtRoomNumber);

            dlg.Controls.Add(new Label { Text = "Floor *", Font = new Font("Segoe UI", 9), Location = new Point(240, y), AutoSize = true });
            nudFloor = new NumericUpDown { Location = new Point(240, y + 18), Size = new Size(80, 25), Font = new Font("Segoe UI", 10), Minimum = 1, Maximum = 100, Value = 1 };
            dlg.Controls.Add(nudFloor);

            dlg.Controls.Add(new Label { Text = "Room Type *", Font = new Font("Segoe UI", 9), Location = new Point(340, y), AutoSize = true });
            cboRoomType = new ComboBox { Location = new Point(340, y + 18), Size = new Size(190, 25), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRoomType.Items.AddRange(new object[] { "Single", "Double", "Suite", "Deluxe" });
            dlg.Controls.Add(cboRoomType);

            y += 58;
            dlg.Controls.Add(new Label { Text = "Price per Night *", Font = new Font("Segoe UI", 9), Location = new Point(20, y), AutoSize = true });
            nudPrice = new NumericUpDown { Location = new Point(20, y + 18), Size = new Size(200, 25), Font = new Font("Segoe UI", 10), DecimalPlaces = 2, Minimum = 0, Maximum = 1000000, Increment = 50 };
            dlg.Controls.Add(nudPrice);

            dlg.Controls.Add(new Label { Text = "Max Guests *", Font = new Font("Segoe UI", 9), Location = new Point(240, y), AutoSize = true });
            nudMaxGuests = new NumericUpDown { Location = new Point(240, y + 18), Size = new Size(80, 25), Font = new Font("Segoe UI", 10), Minimum = 1, Maximum = 20, Value = 1 };
            dlg.Controls.Add(nudMaxGuests);

            y += 58;
            dlg.Controls.Add(new Label { Text = "Amenities *", Font = new Font("Segoe UI", 9), Location = new Point(20, y), AutoSize = true });
            txtAmenities = new TextBox { Location = new Point(20, y + 18), Size = new Size(510, 25), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            dlg.Controls.Add(txtAmenities);

            y += 58;
            dlg.Controls.Add(new Label { Text = "Description", Font = new Font("Segoe UI", 9), Location = new Point(20, y), AutoSize = true });
            txtDescription = new TextBox { Location = new Point(20, y + 18), Size = new Size(510, 80), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, Multiline = true };
            dlg.Controls.Add(txtDescription);

            // Buttons
            btnSubmit = new Button
            {
                Text = "Add Room",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.Black,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 34),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(dlg.ClientSize.Width - 140 - 110, dlg.ClientSize.Height - 54)
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            ApplyRounded(btnSubmit, 16);
            dlg.Controls.Add(btnSubmit);

            btnCancel = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 34),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(dlg.ClientSize.Width - 100 - 20, dlg.ClientSize.Height - 54)
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 230);
            btnCancel.FlatAppearance.BorderSize = 1;
            ApplyRounded(btnCancel, 16);
            dlg.Controls.Add(btnCancel);

            btnClose.Click += (_, __) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };
            dlg.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    dlg.DialogResult = DialogResult.Cancel;
                    dlg.Close();
                }
            };

            // Keep buttons aligned on resize
            var submitLocal = btnSubmit;
            var cancelLocal = btnCancel;
            dlg.Resize += (_, __) =>
            {
                cancelLocal.Location = new Point(dlg.ClientSize.Width - cancelLocal.Width - 20, dlg.ClientSize.Height - 54);
                submitLocal.Location = new Point(cancelLocal.Left - submitLocal.Width - 20, dlg.ClientSize.Height - 54);
            };

            return dlg;
        }

        // Idempotent ensure of action columns
        public static void ConfigureActionColumns(DataGridView grid)
        {
            if (grid is null) return;
            var cols = grid.Columns;

            // Remove legacy single "Action" column if present
            DataGridViewColumn? legacyAction = null;
            foreach (DataGridViewColumn c in cols)
            {
                if (string.Equals(c.HeaderText, "Action", StringComparison.OrdinalIgnoreCase))
                {
                    legacyAction = c;
                    break;
                }
            }
            if (legacyAction != null) cols.Remove(legacyAction);

            if (!cols.Contains(ColEdit))
            {
                cols.Add(new DataGridViewButtonColumn
                {
                    Name = ColEdit,
                    HeaderText = string.Empty,
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    FillWeight = 50, // narrower
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    ReadOnly = true
                });
            }
            else if (cols[ColEdit] is DataGridViewButtonColumn editBtn)
            {
                editBtn.HeaderText = string.Empty;
                editBtn.Text = "Edit";
                editBtn.UseColumnTextForButtonValue = true;
                editBtn.SortMode = DataGridViewColumnSortMode.NotSortable;
                editBtn.ReadOnly = true;
                editBtn.FillWeight = 50; // enforce width
                editBtn.FlatStyle = FlatStyle.Standard;
            }

            if (!cols.Contains(ColMaintenance))
            {
                cols.Add(new DataGridViewButtonColumn
                {
                    Name = ColMaintenance,
                    HeaderText = string.Empty,
                    Text = "Maintenance",
                    UseColumnTextForButtonValue = true,
                    FillWeight = 70, // narrower
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    ReadOnly = true
                });
            }
            else if (cols[ColMaintenance] is DataGridViewButtonColumn maintBtn)
            {
                maintBtn.HeaderText = string.Empty;
                maintBtn.Text = "Maintenance";
                maintBtn.UseColumnTextForButtonValue = true;
                maintBtn.SortMode = DataGridViewColumnSortMode.NotSortable;
                maintBtn.ReadOnly = true;
                maintBtn.FillWeight = 70; // enforce width
                maintBtn.FlatStyle = FlatStyle.Standard;
            }
        }

        // Merged "Actions" header for the two action columns
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
            if (!cols.Contains(ColEdit) || !cols.Contains(ColMaintenance))
                return;

            var colEdit = cols[ColEdit];
            var colMaint = cols[ColMaintenance];

            if (!colEdit.Visible || !colMaint.Visible) return;

            var rectEdit = grid.GetCellDisplayRectangle(colEdit.Index, -1, true);
            var rectMaint = grid.GetCellDisplayRectangle(colMaint.Index, -1, true);

            if (rectEdit.Width <= 0 || rectMaint.Width <= 0) return;

            var mergedRect = Rectangle.FromLTRB(rectEdit.Left, rectEdit.Top, rectMaint.Right, rectEdit.Bottom);
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

        // Show dialog owned by caller and protect with click shield (Employee behavior)
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
                        // Enabled = false // <-- REMOVE this; showing a disabled Form throws
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

        // Quick top-right toast (non-activating), auto-closes
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

        // Queue toast so it shows after modal closes
        public static void ShowToastAfterDialogClose(Control context, Form dlg, string message = "Operation Successful", int durationMs = 1000)
        {
            void Handler(object? s, FormClosedEventArgs e)
            {
                dlg.FormClosed -= Handler;
                (context ?? (Control)dlg).BeginInvoke(new Action(() => ShowToast(context ?? dlg, message, durationMs)));
            }
            dlg.FormClosed += Handler;
        }

        // Click shield (owner-aware)
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

        // Simple, robust toast (no transparency on Form, centered text)
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

        // Visual helpers
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

        // Pause helper for MessageBoxes
        [ThreadStatic] private static int _pauseDepth;
        public static IDisposable PauseShield() { _pauseDepth++; return new PauseCookie(); }
        private sealed class PauseCookie : IDisposable { public void Dispose() => _pauseDepth = Math.Max(0, _pauseDepth - 1); }

        private static void ApplyEmployeeTableStyle(DataGridView grid)
        {
            // Header style
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(243, 244, 246);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft; // keep header left
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 34;

            // Cells
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft; // keep rows left
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.DefaultCellStyle.Padding = new Padding(6, 6, 6, 6);

            // Alternating rows and grid lines
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 251);
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.GridColor = Color.FromArgb(229, 231, 235);

            // Behavior
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Color.White;
            grid.RowTemplate.Height = 32;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        private static void EnableDoubleBuffer(DataGridView grid)
        {
            var prop = typeof(Control).GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            prop?.SetValue(grid, true, null);
        }


    }
}
