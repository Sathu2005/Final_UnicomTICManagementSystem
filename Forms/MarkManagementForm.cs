using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Forms
{
    public partial class MarkManagementForm : BaseForm
    {
        public MarkManagementForm(User currentUser) : base(currentUser)
        {
            InitializeComponent();
            ShowPlaceholderMessage();
        }

        private void ShowPlaceholderMessage()
        {
            var lblMessage = new Label();
            lblMessage.Text = "Mark Management functionality will be implemented here.\n\nFeatures to include:\n- Enter marks for students\n- View marks by subject/student\n- Calculate grades and percentages\n- Generate mark reports";
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
            // MarkManagementForm
            // 
            this.ClientSize = new Size(800, 600);
            this.Text = "Mark Management";
            this.ResumeLayout(false);
        }
    }
}