using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Forms
{
    public partial class RoomManagementForm : BaseForm
    {
        public RoomManagementForm(User currentUser) : base(currentUser)
        {
            InitializeComponent();
            ShowPlaceholderMessage();
        }

        private void ShowPlaceholderMessage()
        {
            var lblMessage = new Label();
            lblMessage.Text = "Room Management functionality will be implemented here.\n\nFeatures to include:\n- Add/Edit/Delete rooms\n- Differentiate room types (Labs, Lecture Halls, etc.)\n- Manage room capacity and equipment\n- Room availability tracking";
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
            // RoomManagementForm
            // 
            this.ClientSize = new Size(800, 600);
            this.Text = "Room Management";
            this.ResumeLayout(false);
        }
    }
}