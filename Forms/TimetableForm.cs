using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Forms
{
    public partial class TimetableForm : BaseForm
    {
        public TimetableForm(User currentUser) : base(currentUser)
        {
            InitializeComponent();
            ShowPlaceholderMessage();
        }

        private void ShowPlaceholderMessage()
        {
            var lblMessage = new Label();
            lblMessage.Text = "Timetable Management functionality will be implemented here.\n\nFeatures to include:\n- View timetable in grid format\n- Assign subjects to time slots\n- Assign rooms to classes\n- Manage lecturer schedules";
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
            // TimetableForm
            // 
            this.ClientSize = new Size(800, 600);
            this.Text = "Timetable Management";
            this.ResumeLayout(false);
        }
    }
}