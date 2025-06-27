using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Forms
{
    public partial class BaseForm : Form
    {
        protected User? CurrentUser { get; set; }

        public BaseForm()
        {
            InitializeComponent();
            SetupBaseForm();
        }

        public BaseForm(User currentUser) : this()
        {
            CurrentUser = currentUser;
            SetupUserContext();
        }

        private void SetupBaseForm()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.White;
        }

        private void SetupUserContext()
        {
            if (CurrentUser != null)
            {
                this.Text += $" - {CurrentUser.FullName} ({CurrentUser.Role})";
            }
        }

        protected virtual void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected virtual void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected virtual void ShowWarning(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        protected virtual bool ConfirmAction(string message)
        {
            return MessageBox.Show(message, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        protected virtual bool HasPermission(UserRole requiredRole)
        {
            if (CurrentUser == null) return false;

            return CurrentUser.Role switch
            {
                UserRole.Admin => true,
                UserRole.Lecturer => requiredRole == UserRole.Lecturer || requiredRole == UserRole.Student,
                UserRole.Staff => requiredRole == UserRole.Staff || requiredRole == UserRole.Student,
                UserRole.Student => requiredRole == UserRole.Student,
                _ => false
            };
        }

        private void BaseForm_Load(object sender, EventArgs e)
        {
            // Base form load event
        }
    }
}