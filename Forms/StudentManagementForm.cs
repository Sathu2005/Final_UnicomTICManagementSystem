using System.Data.SQLite;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Repositories;

namespace SchoolManagementSystem.Forms
{
    public partial class StudentManagementForm : BaseForm
    {
        private readonly DatabaseContext _context;
        private readonly CourseRepository _courseRepository;
        private DataGridView dgvStudents;
        private TextBox txtStudentNumber;
        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private DateTimePicker dtpDateOfBirth;
        private ComboBox cboCourse;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private Student? _selectedStudent;

        public StudentManagementForm(User currentUser) : base(currentUser)
        {
            _context = new DatabaseContext();
            _courseRepository = new CourseRepository(_context);
            InitializeComponent();
            LoadCourses();
            LoadStudents();
        }

        private async void LoadCourses()
        {
            try
            {
                var courses = await _courseRepository.GetAllAsync();
                cboCourse.DataSource = courses;
                cboCourse.DisplayMember = "Name";
                cboCourse.ValueMember = "Id";
                cboCourse.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load courses: {ex.Message}");
            }
        }

        private async void LoadStudents()
        {
            try
            {
                var students = new List<Student>();
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"SELECT s.*, c.Name as CourseName 
                             FROM Students s 
                             INNER JOIN Courses c ON s.CourseId = c.Id 
                             WHERE s.IsActive = 1 
                             ORDER BY s.LastName, s.FirstName";

                using var command = new SQLiteCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    students.Add(new Student
                    {
                        Id = reader.GetInt32("Id"),
                        StudentNumber = reader.GetString("StudentNumber"),
                        FirstName = reader.GetString("FirstName"),
                        LastName = reader.GetString("LastName"),
                        Email = reader.GetString("Email"),
                        Phone = reader.IsDBNull("Phone") ? "" : reader.GetString("Phone"),
                        DateOfBirth = reader.GetDateTime("DateOfBirth"),
                        CourseId = reader.GetInt32("CourseId"),
                        EnrollmentDate = reader.GetDateTime("EnrollmentDate"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CourseName = reader.GetString("CourseName")
                    });
                }

                dgvStudents.DataSource = students;
                
                // Hide unnecessary columns
                if (dgvStudents.Columns["Id"] != null)
                    dgvStudents.Columns["Id"].Visible = false;
                if (dgvStudents.Columns["CourseId"] != null)
                    dgvStudents.Columns["CourseId"].Visible = false;
                if (dgvStudents.Columns["IsActive"] != null)
                    dgvStudents.Columns["IsActive"].Visible = false;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load students: {ex.Message}");
            }
        }

        private void dgvStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStudents.CurrentRow?.DataBoundItem is Student student)
            {
                _selectedStudent = student;
                txtStudentNumber.Text = student.StudentNumber;
                txtFirstName.Text = student.FirstName;
                txtLastName.Text = student.LastName;
                txtEmail.Text = student.Email;
                txtPhone.Text = student.Phone;
                dtpDateOfBirth.Value = student.DateOfBirth;
                cboCourse.SelectedValue = student.CourseId;
                
                btnUpdate.Enabled = true;
                btnDelete.Enabled = HasPermission(UserRole.Admin);
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"INSERT INTO Students (StudentNumber, FirstName, LastName, Email, Phone, DateOfBirth, CourseId) 
                             VALUES (@studentNumber, @firstName, @lastName, @email, @phone, @dateOfBirth, @courseId)";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@studentNumber", txtStudentNumber.Text.Trim());
                command.Parameters.AddWithValue("@firstName", txtFirstName.Text.Trim());
                command.Parameters.AddWithValue("@lastName", txtLastName.Text.Trim());
                command.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                command.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                command.Parameters.AddWithValue("@dateOfBirth", dtpDateOfBirth.Value.Date);
                command.Parameters.AddWithValue("@courseId", (int)cboCourse.SelectedValue);

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    ShowSuccess("Student added successfully!");
                    ClearForm();
                    LoadStudents();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to add student: {ex.Message}");
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedStudent == null || !ValidateInput()) return;

                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"UPDATE Students 
                             SET StudentNumber = @studentNumber, FirstName = @firstName, LastName = @lastName, 
                                 Email = @email, Phone = @phone, DateOfBirth = @dateOfBirth, CourseId = @courseId 
                             WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@studentNumber", txtStudentNumber.Text.Trim());
                command.Parameters.AddWithValue("@firstName", txtFirstName.Text.Trim());
                command.Parameters.AddWithValue("@lastName", txtLastName.Text.Trim());
                command.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                command.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                command.Parameters.AddWithValue("@dateOfBirth", dtpDateOfBirth.Value.Date);
                command.Parameters.AddWithValue("@courseId", (int)cboCourse.SelectedValue);
                command.Parameters.AddWithValue("@id", _selectedStudent.Id);

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    ShowSuccess("Student updated successfully!");
                    ClearForm();
                    LoadStudents();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update student: {ex.Message}");
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedStudent == null) return;

                if (ConfirmAction($"Are you sure you want to delete student '{_selectedStudent.FullName}'?"))
                {
                    using var connection = _context.GetConnection();
                    await connection.OpenAsync();

                    var query = "UPDATE Students SET IsActive = 0 WHERE Id = @id";

                    using var command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@id", _selectedStudent.Id);

                    var result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        ShowSuccess("Student deleted successfully!");
                        ClearForm();
                        LoadStudents();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to delete student: {ex.Message}");
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtStudentNumber.Clear();
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            dtpDateOfBirth.Value = DateTime.Now.AddYears(-18);
            cboCourse.SelectedIndex = -1;
            _selectedStudent = null;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            dgvStudents.ClearSelection();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtStudentNumber.Text))
            {
                ShowWarning("Please enter student number.");
                txtStudentNumber.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                ShowWarning("Please enter first name.");
                txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                ShowWarning("Please enter last name.");
                txtLastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowWarning("Please enter email address.");
                txtEmail.Focus();
                return false;
            }

            if (cboCourse.SelectedValue == null)
            {
                ShowWarning("Please select a course.");
                cboCourse.Focus();
                return false;
            }

            if (dtpDateOfBirth.Value >= DateTime.Now.AddYears(-10))
            {
                ShowWarning("Please enter a valid date of birth.");
                dtpDateOfBirth.Focus();
                return false;
            }

            return true;
        }

        private void InitializeComponent()
        {
            this.dgvStudents = new DataGridView();
            this.txtStudentNumber = new TextBox();
            this.txtFirstName = new TextBox();
            this.txtLastName = new TextBox();
            this.txtEmail = new TextBox();
            this.txtPhone = new TextBox();
            this.dtpDateOfBirth = new DateTimePicker();
            this.cboCourse = new ComboBox();
            this.btnAdd = new Button();
            this.btnUpdate = new Button();
            this.btnDelete = new Button();
            this.btnClear = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStudents)).BeginInit();
            this.SuspendLayout();

            // 
            // StudentManagementForm
            // 
            this.ClientSize = new Size(1000, 650);
            this.Text = "Student Management";

            // 
            // dgvStudents
            // 
            this.dgvStudents.AllowUserToAddRows = false;
            this.dgvStudents.AllowUserToDeleteRows = false;
            this.dgvStudents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvStudents.BackgroundColor = Color.White;
            this.dgvStudents.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStudents.Location = new Point(20, 20);
            this.dgvStudents.MultiSelect = false;
            this.dgvStudents.ReadOnly = true;
            this.dgvStudents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvStudents.Size = new Size(650, 400);
            this.dgvStudents.TabIndex = 0;
            this.dgvStudents.SelectionChanged += new EventHandler(this.dgvStudents_SelectionChanged);

            // Form controls
            var lblStudentNumber = new Label() { Text = "Student Number:", Location = new Point(700, 50), Size = new Size(120, 23), Font = new Font("Segoe UI", 10F) };
            this.txtStudentNumber.Location = new Point(700, 75);
            this.txtStudentNumber.Size = new Size(250, 23);
            this.txtStudentNumber.Font = new Font("Segoe UI", 10F);

            var lblFirstName = new Label() { Text = "First Name:", Location = new Point(700, 110), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtFirstName.Location = new Point(700, 135);
            this.txtFirstName.Size = new Size(250, 23);
            this.txtFirstName.Font = new Font("Segoe UI", 10F);

            var lblLastName = new Label() { Text = "Last Name:", Location = new Point(700, 170), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtLastName.Location = new Point(700, 195);
            this.txtLastName.Size = new Size(250, 23);
            this.txtLastName.Font = new Font("Segoe UI", 10F);

            var lblEmail = new Label() { Text = "Email:", Location = new Point(700, 230), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtEmail.Location = new Point(700, 255);
            this.txtEmail.Size = new Size(250, 23);
            this.txtEmail.Font = new Font("Segoe UI", 10F);

            var lblPhone = new Label() { Text = "Phone:", Location = new Point(700, 290), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtPhone.Location = new Point(700, 315);
            this.txtPhone.Size = new Size(250, 23);
            this.txtPhone.Font = new Font("Segoe UI", 10F);

            var lblDateOfBirth = new Label() { Text = "Date of Birth:", Location = new Point(700, 350), Size = new Size(100, 23), Font = new Font("Segoe UI", 10F) };
            this.dtpDateOfBirth.Location = new Point(700, 375);
            this.dtpDateOfBirth.Size = new Size(200, 23);
            this.dtpDateOfBirth.Font = new Font("Segoe UI", 10F);
            this.dtpDateOfBirth.Value = DateTime.Now.AddYears(-18);

            var lblCourse = new Label() { Text = "Course:", Location = new Point(700, 410), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.cboCourse.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboCourse.Location = new Point(700, 435);
            this.cboCourse.Size = new Size(250, 23);
            this.cboCourse.Font = new Font("Segoe UI", 10F);

            // Buttons
            this.btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnAdd.ForeColor = Color.White;
            this.btnAdd.Location = new Point(700, 480);
            this.btnAdd.Size = new Size(80, 35);
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);

            this.btnUpdate.BackColor = Color.FromArgb(0, 123, 255);
            this.btnUpdate.Enabled = false;
            this.btnUpdate.FlatStyle = FlatStyle.Flat;
            this.btnUpdate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnUpdate.ForeColor = Color.White;
            this.btnUpdate.Location = new Point(790, 480);
            this.btnUpdate.Size = new Size(80, 35);
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new EventHandler(this.btnUpdate_Click);

            this.btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            this.btnDelete.Enabled = false;
            this.btnDelete.FlatStyle = FlatStyle.Flat;
            this.btnDelete.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.Location = new Point(880, 480);
            this.btnDelete.Size = new Size(80, 35);
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            this.btnClear.BackColor = Color.FromArgb(108, 117, 125);
            this.btnClear.FlatStyle = FlatStyle.Flat;
            this.btnClear.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnClear.ForeColor = Color.White;
            this.btnClear.Location = new Point(700, 530);
            this.btnClear.Size = new Size(80, 35);
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // Add controls to form
            this.Controls.Add(this.dgvStudents);
            this.Controls.Add(lblStudentNumber);
            this.Controls.Add(this.txtStudentNumber);
            this.Controls.Add(lblFirstName);
            this.Controls.Add(this.txtFirstName);
            this.Controls.Add(lblLastName);
            this.Controls.Add(this.txtLastName);
            this.Controls.Add(lblEmail);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(lblPhone);
            this.Controls.Add(this.txtPhone);
            this.Controls.Add(lblDateOfBirth);
            this.Controls.Add(this.dtpDateOfBirth);
            this.Controls.Add(lblCourse);
            this.Controls.Add(this.cboCourse);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnClear);

            ((System.ComponentModel.ISupportInitialize)(this.dgvStudents)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}