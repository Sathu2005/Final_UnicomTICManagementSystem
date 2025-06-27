using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Forms
{
    public partial class UserManagementForm : BaseForm
    {
        public UserManagementForm(User currentUser) : base(currentUser)
        {
            InitializeComponent();
            ShowPlaceholderMessage();
        }

        private void ShowPlaceholderMessage()
        {
            var lblMessage = new Label();
            lblMessage.Text = "User Management functionality will be implemented here.\n\nFeatures to include:\n- Add/Edit/Delete users\n- Manage user roles (Admin, Lecturer, Staff, Student)\n- Reset passwords\n- User activity management";
            lblMessage.Font = new Font("Segoe UI", 12F);
            lblMessage.ForeColor = Color.Gray;
            lblMessage.Location = new Point(50, 50);
            lblMessage.Size = new Size(500, 200);
            this.Controls.Add(lblMessage);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // UserManagementForm
            // 
            this.ClientSize = new Size(800, 600);
            this.Text = "User Management";
            this.ResumeLayout(false);
        }
    }
}