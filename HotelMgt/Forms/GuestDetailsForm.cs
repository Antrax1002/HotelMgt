using System;
using System.Drawing;
using System.Windows.Forms;

namespace HotelMgt.Forms
{
    public partial class GuestDetailsForm : Form
    {
        public bool IsConfirmed { get; private set; }
        public string FirstName { get; }
        public string MiddleName { get; }
        public string LastName { get; }
        public string Email { get; }
        public string Phone { get; }
        public string IDType { get; }
        public string IDNumber { get; }

        // Label controls
        private Label lblName;
        private Label lblEmail;
        private Label lblPhone;
        private Label lblIDType;
        private Label lblIDNumber;

        public GuestDetailsForm(string firstName, string middleName, string lastName, string email, string phone, string idType, string idNumber)
        {
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            IDType = idType;
            IDNumber = idNumber;

            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Existing Guest Found";
            this.Size = new Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var font = new Font("Segoe UI", 10);

            int y = 20;
            int labelWidth = 100;
            int valueWidth = 250;

            Controls.Add(new Label { Text = "Name:", Location = new Point(20, y), Width = labelWidth, Font = font });
            lblName = new Label { Text = $"{FirstName} {MiddleName} {LastName}".Replace("  ", " "), Location = new Point(120, y), Width = valueWidth, Font = font };
            Controls.Add(lblName);

            y += 30;
            Controls.Add(new Label { Text = "Email:", Location = new Point(20, y), Width = labelWidth, Font = font });
            lblEmail = new Label { Text = Email, Location = new Point(120, y), Width = valueWidth, Font = font };
            Controls.Add(lblEmail);

            y += 30;
            Controls.Add(new Label { Text = "Phone:", Location = new Point(20, y), Width = labelWidth, Font = font });
            lblPhone = new Label { Text = Phone, Location = new Point(120, y), Width = valueWidth, Font = font };
            Controls.Add(lblPhone);

            y += 30;
            Controls.Add(new Label { Text = "ID Type:", Location = new Point(20, y), Width = labelWidth, Font = font });
            lblIDType = new Label { Text = IDType, Location = new Point(120, y), Width = valueWidth, Font = font };
            Controls.Add(lblIDType);

            y += 30;
            Controls.Add(new Label { Text = "ID Number:", Location = new Point(20, y), Width = labelWidth, Font = font });
            lblIDNumber = new Label { Text = IDNumber, Location = new Point(120, y), Width = valueWidth, Font = font };
            Controls.Add(lblIDNumber);

            var info = new Label
            {
                Text = "Double-click anywhere to confirm this is the correct guest.",
                Location = new Point(20, y + 40),
                Width = 350,
                ForeColor = Color.DimGray,
                Font = new Font(font, FontStyle.Italic)
            };
            Controls.Add(info);

            // Double-click anywhere to confirm
            this.DoubleClick += Confirm;
            foreach (Control c in this.Controls)
                c.DoubleClick += Confirm;
        }

        private void Confirm(object? sender, EventArgs e)
        {
            IsConfirmed = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}