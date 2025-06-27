using System.Data.SQLite;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Forms
{
    public partial class UserManagementForm : BaseForm
    {
        private readonly DatabaseContext _context;
        private DataGridView dgvUsers;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtFullName;
        private TextBox txtEmail;
        private ComboBox cboRole;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private Button btnResetPassword;
        private User? _selectedUser;

        public UserManagementForm(User currentUser) : base(currentUser)
        {
            _context = new DatabaseContext();
            InitializeComponent();
            LoadRoles();
            LoadUsers();
        }

        private void LoadRoles()
        {
            var roles = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(r => new { Value = (int)r, Name = r.ToString() })
                .ToList();

            cboRole.DataSource = roles;
            cboRole.DisplayMember = "Name";
            cboRole.ValueMember = "Value";
            cboRole.SelectedIndex = -1;
        }

        private async void LoadUsers()
        {
            try
            {
                var users = new List<User>();
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT Id, Username, FullName, Email, Role, CreatedDate, IsActive FROM Users WHERE IsActive = 1 ORDER BY FullName";

                using var command = new SQLiteCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        Id = reader.GetInt32("Id"),
                        Username = reader.GetString("Username"),
                        FullName = reader.GetString("FullName"),
                        Email = reader.GetString("Email"),
                        Role = (UserRole)reader.GetInt32("Role"),
                        CreatedDate = reader.GetDateTime("CreatedDate"),
                        IsActive = reader.GetBoolean("IsActive")
                    });
                }

                dgvUsers.DataSource = users;
                
                // Hide unnecessary columns
                if (dgvUsers.Columns["Id"] != null)
                    dgvUsers.Columns["Id"].Visible = false;
                if (dgvUsers.Columns["Password"] != null)
                    dgvUsers.Columns["Password"].Visible = false;
                if (dgvUsers.Columns["IsActive"] != null)
                    dgvUsers.Columns["IsActive"].Visible = false;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load users: {ex.Message}");
            }
        }

        private void dgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow?.DataBoundItem is User user)
            {
                _selectedUser = user;
                txtUsername.Text = user.Username;
                txtPassword.Clear(); // Don't show password
                txtFullName.Text = user.FullName;
                txtEmail.Text = user.Email;
                cboRole.SelectedValue = (int)user.Role;
                
                btnUpdate.Enabled = true;
                btnDelete.Enabled = HasPermission(UserRole.Admin) && user.Id != CurrentUser?.Id;
                btnResetPassword.Enabled = true;
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"INSERT INTO Users (Username, Password, FullName, Email, Role) 
                             VALUES (@username, @password, @fullName, @email, @role)";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                command.Parameters.AddWithValue("@password", txtPassword.Text);
                command.Parameters.AddWithValue("@fullName", txtFullName.Text.Trim());
                command.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                command.Parameters.AddWithValue("@role", (int)cboRole.SelectedValue);

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    ShowSuccess("User added successfully!");
                    ClearForm();
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to add user: {ex.Message}");
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedUser == null || !ValidateInput(false)) return;

                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"UPDATE Users 
                             SET Username = @username, FullName = @fullName, Email = @email, Role = @role 
                             WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                command.Parameters.AddWithValue("@fullName", txtFullName.Text.Trim());
                command.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                command.Parameters.AddWithValue("@role", (int)cboRole.SelectedValue);
                command.Parameters.AddWithValue("@id", _selectedUser.Id);

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    ShowSuccess("User updated successfully!");
                    ClearForm();
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update user: {ex.Message}");
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedUser == null) return;

                if (_selectedUser.Id == CurrentUser?.Id)
                {
                    ShowWarning("You cannot delete your own account.");
                    return;
                }

                if (ConfirmAction($"Are you sure you want to delete user '{_selectedUser.FullName}'?"))
                {
                    using var connection = _context.GetConnection();
                    await connection.OpenAsync();

                    var query = "UPDATE Users SET IsActive = 0 WHERE Id = @id";

                    using var command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@id", _selectedUser.Id);

                    var result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        ShowSuccess("User deleted successfully!");
                        ClearForm();
                        LoadUsers();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to delete user: {ex.Message}");
            }
        }

        private async void btnResetPassword_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedUser == null) return;

                var newPassword = "password123"; // Default password
                if (ConfirmAction($"Are you sure you want to reset password for '{_selectedUser.FullName}' to '{newPassword}'?"))
                {
                    using var connection = _context.GetConnection();
                    await connection.OpenAsync();

                    var query = "UPDATE Users SET Password = @password WHERE Id = @id";

                    using var command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@password", newPassword);
                    command.Parameters.AddWithValue("@id", _selectedUser.Id);

                    var result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        ShowSuccess($"Password reset successfully! New password: {newPassword}");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to reset password: {ex.Message}");
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtFullName.Clear();
            txtEmail.Clear();
            cboRole.SelectedIndex = -1;
            _selectedUser = null;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            btnResetPassword.Enabled = false;
            dgvUsers.ClearSelection();
        }

        private bool ValidateInput(bool isNewUser = true)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowWarning("Please enter username.");
                txtUsername.Focus();
                return false;
            }

            if (isNewUser && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowWarning("Please enter password.");
                txtPassword.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowWarning("Please enter full name.");
                txtFullName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowWarning("Please enter email address.");
                txtEmail.Focus();
                return false;
            }

            if (cboRole.SelectedValue == null)
            {
                ShowWarning("Please select a role.");
                cboRole.Focus();
                return false;
            }

            return true;
        }

        private void InitializeComponent()
        {
            this.dgvUsers = new DataGridView();
            this.txtUsername = new TextBox();
            this.txtPassword = new TextBox();
            this.txtFullName = new TextBox();
            this.txtEmail = new TextBox();
            this.cboRole = new ComboBox();
            this.btnAdd = new Button();
            this.btnUpdate = new Button();
            this.btnDelete = new Button();
            this.btnClear = new Button();
            this.btnResetPassword = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
            this.SuspendLayout();

            // 
            // UserManagementForm
            // 
            this.ClientSize = new Size(1000, 650);
            this.Text = "User Management";

            // 
            // dgvUsers
            // 
            this.dgvUsers.AllowUserToAddRows = false;
            this.dgvUsers.AllowUserToDeleteRows = false;
            this.dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsers.BackgroundColor = Color.White;
            this.dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsers.Location = new Point(20, 20);
            this.dgvUsers.MultiSelect = false;
            this.dgvUsers.ReadOnly = true;
            this.dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsers.Size = new Size(650, 400);
            this.dgvUsers.TabIndex = 0;
            this.dgvUsers.SelectionChanged += new EventHandler(this.dgvUsers_SelectionChanged);

            // Form controls
            var lblUsername = new Label() { Text = "Username:", Location = new Point(700, 50), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtUsername.Location = new Point(700, 75);
            this.txtUsername.Size = new Size(250, 23);
            this.txtUsername.Font = new Font("Segoe UI", 10F);

            var lblPassword = new Label() { Text = "Password:", Location = new Point(700, 110), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtPassword.Location = new Point(700, 135);
            this.txtPassword.Size = new Size(250, 23);
            this.txtPassword.Font = new Font("Segoe UI", 10F);
            this.txtPassword.UseSystemPasswordChar = true;

            var lblFullName = new Label() { Text = "Full Name:", Location = new Point(700, 170), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtFullName.Location = new Point(700, 195);
            this.txtFullName.Size = new Size(250, 23);
            this.txtFullName.Font = new Font("Segoe UI", 10F);

            var lblEmail = new Label() { Text = "Email:", Location = new Point(700, 230), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtEmail.Location = new Point(700, 255);
            this.txtEmail.Size = new Size(250, 23);
            this.txtEmail.Font = new Font("Segoe UI", 10F);

            var lblRole = new Label() { Text = "Role:", Location = new Point(700, 290), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboRole.Location = new Point(700, 315);
            this.cboRole.Size = new Size(250, 23);
            this.cboRole.Font = new Font("Segoe UI", 10F);

            // Buttons
            this.btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnAdd.ForeColor = Color.White;
            this.btnAdd.Location = new Point(700, 360);
            this.btnAdd.Size = new Size(80, 35);
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);

            this.btnUpdate.BackColor = Color.FromArgb(0, 123, 255);
            this.btnUpdate.Enabled = false;
            this.btnUpdate.FlatStyle = FlatStyle.Flat;
            this.btnUpdate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnUpdate.ForeColor = Color.White;
            this.btnUpdate.Location = new Point(790, 360);
            this.btnUpdate.Size = new Size(80, 35);
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new EventHandler(this.btnUpdate_Click);

            this.btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            this.btnDelete.Enabled = false;
            this.btnDelete.FlatStyle = FlatStyle.Flat;
            this.btnDelete.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.Location = new Point(880, 360);
            this.btnDelete.Size = new Size(80, 35);
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            this.btnResetPassword.BackColor = Color.FromArgb(255, 193, 7);
            this.btnResetPassword.Enabled = false;
            this.btnResetPassword.FlatStyle = FlatStyle.Flat;
            this.btnResetPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnResetPassword.ForeColor = Color.Black;
            this.btnResetPassword.Location = new Point(700, 410);
            this.btnResetPassword.Size = new Size(120, 35);
            this.btnResetPassword.Text = "Reset Password";
            this.btnResetPassword.UseVisualStyleBackColor = false;
            this.btnResetPassword.Click += new EventHandler(this.btnResetPassword_Click);

            this.btnClear.BackColor = Color.FromArgb(108, 117, 125);
            this.btnClear.FlatStyle = FlatStyle.Flat;
            this.btnClear.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnClear.ForeColor = Color.White;
            this.btnClear.Location = new Point(830, 410);
            this.btnClear.Size = new Size(80, 35);
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // Add controls to form
            this.Controls.Add(this.dgvUsers);
            this.Controls.Add(lblUsername);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(lblFullName);
            this.Controls.Add(this.txtFullName);
            this.Controls.Add(lblEmail);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(lblRole);
            this.Controls.Add(this.cboRole);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnResetPassword);
            this.Controls.Add(this.btnClear);

            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}