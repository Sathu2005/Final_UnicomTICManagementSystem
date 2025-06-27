using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Forms
{
    public partial class ExamManagementForm : BaseForm
    {
        public ExamManagementForm(User currentUser) : base(currentUser)
        {
            InitializeComponent();
            ShowPlaceholderMessage();
        }

        private void ShowPlaceholderMessage()
        {
            var lblMessage = new Label();
            lblMessage.Text = "Exam Management functionality will be implemented here.\n\nFeatures to include:\n- Schedule exams by subject\n- Set exam dates and times\n- Assign rooms to exams\n- Manage exam details";
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
            // ExamManagementForm
            // 
            this.ClientSize = new Size(800, 600);
            this.Text = "Exam Management";
            this.ResumeLayout(false);
        }
    }
}