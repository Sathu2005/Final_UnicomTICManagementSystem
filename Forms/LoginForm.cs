using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Services;

namespace SchoolManagementSystem.Forms
{
    public partial class LoginForm : BaseForm
    {
        private readonly UserService _userService;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;

        public LoginForm()
        {
            var context = new DatabaseContext();
            _userService = new UserService(context);
            InitializeComponent();
            InitializeDatabase();
        }

        private async void InitializeDatabase()
        {
            try
            {
                var context = new DatabaseContext();
                await context.InitializeDatabaseAsync();
            }
            catch (Exception ex)
            {
                ShowError($"Database initialization failed: {ex.Message}");
                Application.Exit();
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    ShowWarning("Please enter both username and password.");
                    return;
                }

                btnLogin.Enabled = false;
                btnLogin.Text = "Logging in...";

                var user = await _userService.AuthenticateAsync(txtUsername.Text.Trim(), txtPassword.Text);

                if (user != null)
                {
                    this.Hide();
                    var dashboardForm = new DashboardForm(user);
                    dashboardForm.FormClosed += (s, args) => this.Close();
                    dashboardForm.Show();
                }
                else
                {
                    ShowError("Invalid username or password.");
                    txtPassword.Clear();
                    txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Login failed: {ex.Message}");
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Login";
            }
        }

        private void InitializeComponent()
        {
            this.txtUsername = new TextBox();
            this.txtPassword = new TextBox();
            this.btnLogin = new Button();
            this.lblTitle = new Label();
            this.lblUsername = new Label();
            this.lblPassword = new Label();
            this.SuspendLayout();
            
            // 
            // LoginForm
            // 
            this.ClientSize = new Size(400, 300);
            this.Text = "School Management System - Login";
            this.BackColor = Color.White;
            
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.FromArgb(0, 122, 204);
            this.lblTitle.Location = new Point(50, 30);
            this.lblTitle.Size = new Size(300, 30);
            this.lblTitle.Text = "School Management System";
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new Font("Segoe UI", 10F);
            this.lblUsername.Location = new Point(50, 100);
            this.lblUsername.Size = new Size(71, 19);
            this.lblUsername.Text = "Username:";
            
            // 
            // txtUsername
            // 
            this.txtUsername.Font = new Font("Segoe UI", 10F);
            this.txtUsername.Location = new Point(50, 125);
            this.txtUsername.Size = new Size(300, 25);
            this.txtUsername.TabIndex = 0;
            
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new Font("Segoe UI", 10F);
            this.lblPassword.Location = new Point(50, 160);
            this.lblPassword.Size = new Size(70, 19);
            this.lblPassword.Text = "Password:";
            
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new Font("Segoe UI", 10F);
            this.txtPassword.Location = new Point(50, 185);
            this.txtPassword.Size = new Size(300, 25);
            this.txtPassword.TabIndex = 1;
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.KeyPress += (s, e) => {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    btnLogin_Click(btnLogin, EventArgs.Empty);
                }
            };
            
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = Color.FromArgb(0, 122, 204);
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.Location = new Point(150, 230);
            this.btnLogin.Size = new Size(100, 35);
            this.btnLogin.TabIndex = 2;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new EventHandler(this.btnLogin_Click);
            
            // Add controls to form
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.btnLogin);
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}