using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Forms
{
    public partial class DashboardForm : BaseForm
    {
        private Panel pnlMenu;
        private Panel pnlContent;
        private Label lblWelcome;
        private Button btnCourses;
        private Button btnSubjects;
        private Button btnStudents;
        private Button btnExams;
        private Button btnMarks;
        private Button btnTimetable;
        private Button btnRooms;
        private Button btnUsers;
        private Button btnLogout;

        public DashboardForm(User currentUser) : base(currentUser)
        {
            InitializeComponent();
            SetupMenuBasedOnRole();
        }

        private void SetupMenuBasedOnRole()
        {
            if (CurrentUser == null) return;

            // Hide all buttons first
            btnCourses.Visible = false;
            btnSubjects.Visible = false;
            btnStudents.Visible = false;
            btnExams.Visible = false;
            btnMarks.Visible = false;
            btnTimetable.Visible = false;
            btnRooms.Visible = false;
            btnUsers.Visible = false;

            // Show buttons based on role
            switch (CurrentUser.Role)
            {
                case UserRole.Admin:
                    btnCourses.Visible = true;
                    btnSubjects.Visible = true;
                    btnStudents.Visible = true;
                    btnExams.Visible = true;
                    btnMarks.Visible = true;
                    btnTimetable.Visible = true;
                    btnRooms.Visible = true;
                    btnUsers.Visible = true;
                    break;

                case UserRole.Lecturer:
                    btnSubjects.Visible = true;
                    btnStudents.Visible = true;
                    btnExams.Visible = true;
                    btnMarks.Visible = true;
                    btnTimetable.Visible = true;
                    break;

                case UserRole.Staff:
                    btnCourses.Visible = true;
                    btnSubjects.Visible = true;
                    btnStudents.Visible = true;
                    btnTimetable.Visible = true;
                    btnRooms.Visible = true;
                    break;

                case UserRole.Student:
                    btnTimetable.Visible = true;
                    btnMarks.Visible = true;
                    break;
            }

            lblWelcome.Text = $"Welcome, {CurrentUser.FullName}!";
        }

        private void btnCourses_Click(object sender, EventArgs e)
        {
            OpenForm(new CourseManagementForm(CurrentUser));
        }

        private void btnSubjects_Click(object sender, EventArgs e)
        {
            OpenForm(new SubjectManagementForm(CurrentUser));
        }

        private void btnStudents_Click(object sender, EventArgs e)
        {
            OpenForm(new StudentManagementForm(CurrentUser));
        }

        private void btnExams_Click(object sender, EventArgs e)
        {
            OpenForm(new ExamManagementForm(CurrentUser));
        }

        private void btnMarks_Click(object sender, EventArgs e)
        {
            OpenForm(new MarkManagementForm(CurrentUser));
        }

        private void btnTimetable_Click(object sender, EventArgs e)
        {
            OpenForm(new TimetableForm(CurrentUser));
        }

        private void btnRooms_Click(object sender, EventArgs e)
        {
            OpenForm(new RoomManagementForm(CurrentUser));
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            OpenForm(new UserManagementForm(CurrentUser));
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (ConfirmAction("Are you sure you want to logout?"))
            {
                this.Hide();
                var loginForm = new LoginForm();
                loginForm.FormClosed += (s, args) => Application.Exit();
                loginForm.Show();
            }
        }

        private void OpenForm(Form form)
        {
            form.ShowDialog();
        }

        private void InitializeComponent()
        {
            this.pnlMenu = new Panel();
            this.pnlContent = new Panel();
            this.lblWelcome = new Label();
            this.btnCourses = new Button();
            this.btnSubjects = new Button();
            this.btnStudents = new Button();
            this.btnExams = new Button();
            this.btnMarks = new Button();
            this.btnTimetable = new Button();
            this.btnRooms = new Button();
            this.btnUsers = new Button();
            this.btnLogout = new Button();
            this.pnlMenu.SuspendLayout();
            this.SuspendLayout();

            // 
            // DashboardForm
            // 
            this.ClientSize = new Size(1000, 700);
            this.Text = "Dashboard";
            this.WindowState = FormWindowState.Maximized;

            // 
            // pnlMenu
            // 
            this.pnlMenu.BackColor = Color.FromArgb(45, 45, 48);
            this.pnlMenu.Dock = DockStyle.Left;
            this.pnlMenu.Size = new Size(250, 700);
            this.pnlMenu.Controls.Add(this.lblWelcome);
            this.pnlMenu.Controls.Add(this.btnCourses);
            this.pnlMenu.Controls.Add(this.btnSubjects);
            this.pnlMenu.Controls.Add(this.btnStudents);
            this.pnlMenu.Controls.Add(this.btnExams);
            this.pnlMenu.Controls.Add(this.btnMarks);
            this.pnlMenu.Controls.Add(this.btnTimetable);
            this.pnlMenu.Controls.Add(this.btnRooms);
            this.pnlMenu.Controls.Add(this.btnUsers);
            this.pnlMenu.Controls.Add(this.btnLogout);

            // 
            // lblWelcome
            // 
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblWelcome.ForeColor = Color.White;
            this.lblWelcome.Location = new Point(20, 20);
            this.lblWelcome.Size = new Size(200, 21);
            this.lblWelcome.Text = "Welcome!";

            // Create menu buttons
            var buttons = new[] { btnCourses, btnSubjects, btnStudents, btnExams, btnMarks, btnTimetable, btnRooms, btnUsers, btnLogout };
            var buttonTexts = new[] { "Courses", "Subjects", "Students", "Exams", "Marks", "Timetable", "Rooms", "Users", "Logout" };
            var buttonEvents = new EventHandler[] { btnCourses_Click, btnSubjects_Click, btnStudents_Click, btnExams_Click, btnMarks_Click, btnTimetable_Click, btnRooms_Click, btnUsers_Click, btnLogout_Click };

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].BackColor = Color.FromArgb(0, 122, 204);
                buttons[i].FlatStyle = FlatStyle.Flat;
                buttons[i].FlatAppearance.BorderSize = 0;
                buttons[i].Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                buttons[i].ForeColor = Color.White;
                buttons[i].Location = new Point(20, 70 + (i * 50));
                buttons[i].Size = new Size(200, 40);
                buttons[i].Text = buttonTexts[i];
                buttons[i].UseVisualStyleBackColor = false;
                buttons[i].Click += buttonEvents[i];
                
                // Special styling for logout button
                if (i == buttons.Length - 1)
                {
                    buttons[i].BackColor = Color.FromArgb(220, 53, 69);
                    buttons[i].Location = new Point(20, 600);
                }
            }

            // 
            // pnlContent
            // 
            this.pnlContent.BackColor = Color.White;
            this.pnlContent.Dock = DockStyle.Fill;
            this.pnlContent.Location = new Point(250, 0);
            this.pnlContent.Size = new Size(750, 700);

            // Add main content
            var lblMainTitle = new Label();
            lblMainTitle.AutoSize = true;
            lblMainTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblMainTitle.ForeColor = Color.FromArgb(0, 122, 204);
            lblMainTitle.Location = new Point(50, 50);
            lblMainTitle.Text = "School Management System";

            var lblDescription = new Label();
            lblDescription.Font = new Font("Segoe UI", 12F);
            lblDescription.ForeColor = Color.Gray;
            lblDescription.Location = new Point(50, 100);
            lblDescription.Size = new Size(600, 200);
            lblDescription.Text = "Welcome to the School Management System. Use the menu on the left to navigate to different modules based on your role and permissions.";

            this.pnlContent.Controls.Add(lblMainTitle);
            this.pnlContent.Controls.Add(lblDescription);

            // Add panels to form
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlMenu);

            this.pnlMenu.ResumeLayout(false);
            this.pnlMenu.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}