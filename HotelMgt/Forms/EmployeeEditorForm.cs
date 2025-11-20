using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace HotelMgt.Forms
{
    public partial class EmployeeEditorForm : Form
    {
        private readonly TextBox txtFirstName;
        private readonly TextBox txtLastName;
        private readonly TextBox txtMiddleName;
        private readonly TextBox txtEmail;
        private readonly TextBox txtPhone;
        private readonly TextBox txtUsername;
        private readonly TextBox txtPassword;
        private readonly ComboBox cboRole;
        private readonly DateTimePicker dtpHireDate;
        private readonly Button btnOK;
        private readonly Button btnCancel;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FirstName
        {
            get => txtFirstName.Text;
            set => txtFirstName.Text = value ?? "";
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LastName
        {
            get => txtLastName.Text;
            set => txtLastName.Text = value ?? "";
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MiddleName
        {
            get => txtMiddleName.Text;
            set => txtMiddleName.Text = value ?? "";
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Email
        {
            get => txtEmail.Text;
            set => txtEmail.Text = value ?? "";
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Phone
        {
            get => txtPhone.Text;
            set => txtPhone.Text = value ?? "";
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Username
        {
            get => txtUsername.Text;
            set => txtUsername.Text = value ?? "";
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Password
        {
            get => txtPassword.Text;
            set => txtPassword.Text = value ?? "";
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Role
        {
            get => cboRole.SelectedItem?.ToString() ?? "";
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (!cboRole.Items.Contains(value))
                        cboRole.Items.Add(value);
                    cboRole.SelectedItem = value;
                }
                else
                {
                    cboRole.SelectedIndex = -1;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime HireDate
        {
            get => dtpHireDate.Value;
            set => dtpHireDate.Value = value;
        }

        /// <summary>
        /// Optional: Set this from the parent to check uniqueness.
        /// Args: email, username, currentEmployeeId (null for add, id for edit)
        /// Return: true if unique, false if duplicate found
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string, string, int?, bool>? IsEmailUsernameUnique { get; set; }

        /// <summary>
        /// Optional: Set this for edit scenarios to skip self in uniqueness check.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? CurrentEmployeeId { get; set; }

        public EmployeeEditorForm()
        {
            this.Text = "Employee Editor";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.MinimumSize = new Size(500, 360); // adjust as needed


            // --- Main Table: 3 columns, 4 rows ---
            var mainTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 4,
                Dock = DockStyle.Top,
                Padding = new Padding(18, 18, 18, 0),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
            };
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            for (int i = 0; i < 4; i++)
                mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

            // Row 0: First Name - Middle Name - Last Name
            txtFirstName = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtMiddleName = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtLastName = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            mainTable.Controls.Add(new Label { Text = "First Name:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            mainTable.Controls.Add(new Label { Text = "Middle Name:", Anchor = AnchorStyles.Left, AutoSize = true }, 1, 0);
            mainTable.Controls.Add(new Label { Text = "Last Name:", Anchor = AnchorStyles.Left, AutoSize = true }, 2, 0);
            mainTable.Controls.Add(txtFirstName, 0, 1);
            mainTable.Controls.Add(txtMiddleName, 1, 1);
            mainTable.Controls.Add(txtLastName, 2, 1);

            // Row 1: Email - Phone - Hire Date
            txtEmail = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtPhone = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            dtpHireDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Value = DateTime.Today
            };
            // Add labels
            mainTable.Controls.Add(new Label { Text = "Email:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 2);
            mainTable.Controls.Add(new Label { Text = "Phone:", Anchor = AnchorStyles.Left, AutoSize = true }, 1, 2);
            mainTable.Controls.Add(new Label { Text = "Hire Date:", Anchor = AnchorStyles.Left, AutoSize = true }, 2, 2);
            // Add controls
            mainTable.Controls.Add(txtEmail, 0, 3);
            mainTable.Controls.Add(txtPhone, 1, 3);
            mainTable.Controls.Add(dtpHireDate, 2, 3);

            // Row 2: Username - Password - Role
            txtUsername = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtPassword = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                UseSystemPasswordChar = true,
                Multiline = false
            };
            cboRole = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cboRole.Items.AddRange(new object[] { "Employee", "Admin" });

            // Add new row for Username, Password, Role labels
            mainTable.RowCount++;
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainTable.Controls.Add(new Label { Text = "Username:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 4);
            mainTable.Controls.Add(new Label { Text = "Password:", Anchor = AnchorStyles.Left, AutoSize = true }, 1, 4);
            mainTable.Controls.Add(new Label { Text = "Role:", Anchor = AnchorStyles.Left, AutoSize = true }, 2, 4);

            // Add new row for Username, Password, Role controls
            mainTable.RowCount++;
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainTable.Controls.Add(txtUsername, 0, 5);
            mainTable.Controls.Add(txtPassword, 1, 5);
            mainTable.Controls.Add(cboRole, 2, 5);


            // --- Buttons panel ---
            btnOK = new Button { Text = "OK", Width = 90, Anchor = AnchorStyles.Right };
            btnCancel = new Button { Text = "Cancel", Width = 90, DialogResult = DialogResult.Cancel, Anchor = AnchorStyles.Left };
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(0, 10, 18, 18),
                Height = 50,
                AutoSize = true
            };
            // Switch the order: Cancel first, then OK
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnOK);

            // --- Add controls to form ---
            this.Controls.Add(buttonPanel);

            var mainPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            this.Controls.Add(mainPanel);

            mainPanel.Controls.Add(mainTable);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            btnOK.Click += btnOK_Click;
            btnCancel.Click += btnCancel_Click;
        }

        private void btnOK_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateInputs()
        {
            // Required fields
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                ShowValidationError("First Name is required.", txtFirstName);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                ShowValidationError("Last Name is required.", txtLastName);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowValidationError("Email is required.", txtEmail);
                return false;
            }
            if (!IsValidEmail(txtEmail.Text))
            {
                ShowValidationError("Please enter a valid email address.", txtEmail);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowValidationError("Username is required.", txtUsername);
                return false;
            }
            if (cboRole.SelectedIndex < 0)
            {
                ShowValidationError("Role is required.", cboRole);
                return false;
            }
            if (dtpHireDate.Value.Date > DateTime.Today)
            {
                ShowValidationError("Hire Date cannot be in the future.", dtpHireDate);
                return false;
            }
            if (!string.IsNullOrWhiteSpace(txtPhone.Text) && !IsValidPhone(txtPhone.Text))
            {
                ShowValidationError("Please enter a valid phone number.", txtPhone);
                return false;
            }

            // Uniqueness check (if delegate is set)
            if (IsEmailUsernameUnique != null)
            {
                bool isUnique = IsEmailUsernameUnique(
                    txtEmail.Text.Trim(),
                    txtUsername.Text.Trim(),
                    CurrentEmployeeId
                );
                if (!isUnique)
                {
                    ShowValidationError("Email or Username already exists.", txtEmail);
                    return false;
                }
            }

            // All validations passed
            return true;
        }

        private void ShowValidationError(string message, Control controlToFocus)
        {
            MessageBox.Show(this, message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            controlToFocus.Focus();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                // Simple .NET email validation
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            // Accepts digits, spaces, dashes, parentheses, and plus sign, min 7 digits
            var digits = Regex.Replace(phone, @"\D", "");
            return digits.Length >= 7 && Regex.IsMatch(phone, @"^[\d\s\-\+\(\)]+$");
        }
    }
}