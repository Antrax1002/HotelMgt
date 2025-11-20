namespace HotelMgt.Forms
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelLoginCard = new HotelMgt.Custom.RoundedPanel();
            panelLogo = new HotelMgt.Custom.RoundedPanel();
            txtUsername = new HotelMgt.Custom.RoundedTextBox();
            txtPassword = new HotelMgt.Custom.RoundedTextBox();
            btnLogin = new HotelMgt.Custom.RoundedButton();
            lbPassword = new Label();
            lblUsername = new Label();
            lblDescription = new Label();
            lblTitle = new Label();
            panelLoginCard.SuspendLayout();
            SuspendLayout();
            // 
            // panelLoginCard
            // 
            panelLoginCard.BackColor = Color.White;
            panelLoginCard.BorderRadius = 12;
            panelLoginCard.Controls.Add(panelLogo);
            panelLoginCard.Controls.Add(txtUsername);
            panelLoginCard.Controls.Add(txtPassword);
            panelLoginCard.Controls.Add(btnLogin);
            panelLoginCard.Controls.Add(lbPassword);
            panelLoginCard.Controls.Add(lblUsername);
            panelLoginCard.Controls.Add(lblDescription);
            panelLoginCard.Controls.Add(lblTitle);
            panelLoginCard.Location = new Point(33, 40);
            panelLoginCard.Name = "panelLoginCard";
            panelLoginCard.Size = new Size(420, 480);
            panelLoginCard.TabIndex = 0;
            // 
            // panelLogo
            // 
            panelLogo.BackColor = Color.FromArgb(37, 99, 235);
            panelLogo.BorderRadius = 12;
            panelLogo.Location = new Point(177, 63);
            panelLogo.Name = "panelLogo";
            panelLogo.Size = new Size(60, 60);
            panelLogo.TabIndex = 25;
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.White;
            txtUsername.BorderColor = Color.FromArgb(203, 213, 225);
            txtUsername.BorderFocusColor = Color.FromArgb(59, 130, 246);
            txtUsername.BorderRadius = 6;
            txtUsername.BorderSize = 1;
            txtUsername.Font = new Font("Segoe UI", 10F);
            txtUsername.ForeColor = Color.Black;
            txtUsername.Location = new Point(37, 245);
            txtUsername.Multiline = false;
            txtUsername.Name = "txtUsername";
            txtUsername.Padding = new Padding(10, 7, 10, 7);
            txtUsername.PasswordChar = false;
            txtUsername.PlaceholderColor = Color.Gray;
            txtUsername.PlaceholderText = "Enter your username";
            txtUsername.Size = new Size(340, 38);
            txtUsername.TabIndex = 24;
            txtUsername.UnderlinedStyle = false;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.White;
            txtPassword.BorderColor = Color.FromArgb(203, 213, 225);
            txtPassword.BorderFocusColor = Color.FromArgb(59, 130, 246);
            txtPassword.BorderRadius = 6;
            txtPassword.BorderSize = 1;
            txtPassword.Font = new Font("Segoe UI", 10F);
            txtPassword.ForeColor = Color.Black;
            txtPassword.Location = new Point(37, 320);
            txtPassword.Multiline = false;
            txtPassword.Name = "txtPassword";
            txtPassword.Padding = new Padding(10, 7, 10, 7);
            txtPassword.PasswordChar = true;
            txtPassword.PlaceholderColor = Color.Gray;
            txtPassword.PlaceholderText = "Enter your password";
            txtPassword.Size = new Size(340, 38);
            txtPassword.TabIndex = 23;
            txtPassword.UnderlinedStyle = false;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(37, 99, 235);
            btnLogin.BorderColor = Color.Transparent;
            btnLogin.BorderRadius = 8;
            btnLogin.BorderSize = 0;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(37, 378);
            btnLogin.Name = "btnLogin";
            btnLogin.RightToLeft = RightToLeft.Yes;
            btnLogin.Size = new Size(346, 40);
            btnLogin.TabIndex = 22;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // lbPassword
            // 
            lbPassword.AutoSize = true;
            lbPassword.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbPassword.ForeColor = Color.FromArgb(30, 41, 59);
            lbPassword.Location = new Point(37, 298);
            lbPassword.Name = "lbPassword";
            lbPassword.Size = new Size(66, 17);
            lbPassword.TabIndex = 21;
            lbPassword.Text = "Password";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblUsername.ForeColor = Color.FromArgb(30, 41, 59);
            lblUsername.Location = new Point(37, 223);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(133, 17);
            lblUsername.TabIndex = 20;
            lblUsername.Text = "Employee Username";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblDescription.ForeColor = Color.FromArgb(100, 116, 139);
            lblDescription.Location = new Point(37, 183);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(342, 20);
            lblDescription.TabIndex = 19;
            lblDescription.Text = "Enter your credentials to access the reception desk";
            lblDescription.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.FromArgb(30, 41, 59);
            lblTitle.Location = new Point(57, 143);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(321, 32);
            lblTitle.TabIndex = 18;
            lblTitle.Text = "Hotel Management System";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.LightGray;
            ClientSize = new Size(484, 561);
            Controls.Add(panelLoginCard);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Hotel Management System - Login";
            panelLoginCard.ResumeLayout(false);
            panelLoginCard.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Custom.RoundedPanel panelLoginCard;
        private Custom.RoundedTextBox txtUsername;
        private Custom.RoundedTextBox txtPassword;
        private Custom.RoundedButton btnLogin;
        private Label lbPassword;
        private Label lblUsername;
        private Label lblDescription;
        private Label lblTitle;
        private Panel roundedPanel1;
        private Custom.RoundedPanel panelLogo;
    }
}