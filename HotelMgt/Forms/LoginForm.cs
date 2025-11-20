using HotelMgt.Services;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HotelMgt.Utilities;

namespace HotelMgt.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthenticationService _authService;
        private PictureBox _logoPb = null!; // initialized in CreateLogoHost

        public LoginForm()
        {
            InitializeComponent();
            _authService = new AuthenticationService();
            CreateLogoHost();
            this.Load += LoginForm_Load; // ensure hooked
        }

        private void CreateLogoHost()
        {
            _logoPb = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            panelLogo.BackgroundImage = null;
            panelLogo.Controls.Clear();
            panelLogo.Controls.Add(_logoPb);
        }

        private void LoginForm_Load(object? sender, EventArgs e)
        {
            // Test database connection
            var dbService = new DatabaseService();
            if (!dbService.TestConnection())
            {
                MessageBox.Show(
                    "Unable to connect to database. Please check your connection string.",
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            // Try embedded resource first (add panelLogo.png to Resources as "panelLogo" if available)
            var resObj = Properties.Resources.ResourceManager.GetObject("panelLogo");
            if (resObj is Image resImage)
            {
                _logoPb.Image = new Bitmap(resImage);
                return;
            }

            // Fallback: load from output Images folder
            string imagePath = Path.Combine(AppContext.BaseDirectory, "Images", "panelLogo.png");
            if (File.Exists(imagePath))
            {
                using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var temp = Image.FromStream(fs))
                {
                    _logoPb.Image = new Bitmap(temp);
                }
                return;
            }

#if DEBUG
            string devPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\Images\panelLogo.png"));
            if (File.Exists(devPath))
            {
                using (var fs2 = new FileStream(devPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var temp2 = Image.FromStream(fs2))
                {
                    _logoPb.Image = new Bitmap(temp2);
                }
                return;
            }
#endif
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show(
                    "Please enter both username and password.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Logging in...";
            Cursor = Cursors.WaitCursor;

            try
            {
                var employee = _authService.AuthenticateUser(username, password);

                if (employee != null)
                {
                    this.Hide();

                    if (employee.Role == Constants.RoleAdmin)
                    {
                        var adminDashboard = new AdminDashboardForm();
                        adminDashboard.FormClosed += (s, args) => this.Close();
                        adminDashboard.Show();
                    }
                    else
                    {
                        var employeeDashboard = new EmployeeDashboardForm();
                        employeeDashboard.FormClosed += (s, args) => this.Close();
                        employeeDashboard.Show();
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Invalid username or password.",
                        "Login Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Login error: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Login";
                Cursor = Cursors.Default;
            }
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }
    }
}