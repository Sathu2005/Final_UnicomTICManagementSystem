using System.Data.SQLite;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Forms
{
    public partial class MarkManagementForm : BaseForm
    {
        private readonly DatabaseContext _context;
        private DataGridView dgvMarks;
        private ComboBox cboExam;
        private ComboBox cboStudent;
        private NumericUpDown nudMarksObtained;
        private TextBox txtGrade;
        private TextBox txtRemarks;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private Mark? _selectedMark;

        public MarkManagementForm(User currentUser) : base(currentUser)
        {
            _context = new DatabaseContext();
            InitializeComponent();
            LoadExams();
            LoadStudents();
            LoadMarks();
        }

        private async void LoadExams()
        {
            try
            {
                var exams = new List<Exam>();
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"SELECT e.*, s.Name as SubjectName 
                             FROM Exams e 
                             INNER JOIN Subjects s ON e.SubjectId = s.Id 
                             WHERE e.IsActive = 1 
                             ORDER BY e.ExamDate DESC";

                using var command = new SQLiteCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    exams.Add(new Exam
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        SubjectId = reader.GetInt32("SubjectId"),
                        ExamDate = reader.GetDateTime("ExamDate"),
                        MaxMarks = reader.GetInt32("MaxMarks"),
                        SubjectName = reader.GetString("SubjectName")
                    });
                }

                cboExam.DataSource = exams;
                cboExam.DisplayMember = "Name";
                cboExam.ValueMember = "Id";
                cboExam.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load exams: {ex.Message}");
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
                        CourseId = reader.GetInt32("CourseId"),
                        CourseName = reader.GetString("CourseName")
                    });
                }

                cboStudent.DataSource = students;
                cboStudent.DisplayMember = "FullName";
                cboStudent.ValueMember = "Id";
                cboStudent.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load students: {ex.Message}");
            }
        }

        private async void LoadMarks()
        {
            try
            {
                var marks = new List<Mark>();
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"SELECT m.*, s.FirstName + ' ' + s.LastName as StudentName, 
                                     e.Name as ExamName, sub.Name as SubjectName, e.MaxMarks 
                             FROM Marks m 
                             INNER JOIN Students s ON m.StudentId = s.Id 
                             INNER JOIN Exams e ON m.ExamId = e.Id 
                             INNER JOIN Subjects sub ON e.SubjectId = sub.Id 
                             ORDER BY m.RecordedDate DESC";

                using var command = new SQLiteCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    marks.Add(new Mark
                    {
                        Id = reader.GetInt32("Id"),
                        StudentId = reader.GetInt32("StudentId"),
                        ExamId = reader.GetInt32("ExamId"),
                        MarksObtained = reader.GetDecimal("MarksObtained"),
                        Grade = reader.IsDBNull("Grade") ? "" : reader.GetString("Grade"),
                        Remarks = reader.IsDBNull("Remarks") ? "" : reader.GetString("Remarks"),
                        RecordedDate = reader.GetDateTime("RecordedDate"),
                        RecordedBy = reader.GetInt32("RecordedBy"),
                        StudentName = reader.GetString("StudentName"),
                        ExamName = reader.GetString("ExamName"),
                        SubjectName = reader.GetString("SubjectName"),
                        MaxMarks = reader.GetDecimal("MaxMarks")
                    });
                }

                dgvMarks.DataSource = marks;
                
                // Hide unnecessary columns
                if (dgvMarks.Columns["Id"] != null)
                    dgvMarks.Columns["Id"].Visible = false;
                if (dgvMarks.Columns["StudentId"] != null)
                    dgvMarks.Columns["StudentId"].Visible = false;
                if (dgvMarks.Columns["ExamId"] != null)
                    dgvMarks.Columns["ExamId"].Visible = false;
                if (dgvMarks.Columns["RecordedBy"] != null)
                    dgvMarks.Columns["RecordedBy"].Visible = false;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load marks: {ex.Message}");
            }
        }

        private void dgvMarks_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMarks.CurrentRow?.DataBoundItem is Mark mark)
            {
                _selectedMark = mark;
                cboExam.SelectedValue = mark.ExamId;
                cboStudent.SelectedValue = mark.StudentId;
                nudMarksObtained.Value = mark.MarksObtained;
                txtGrade.Text = mark.Grade;
                txtRemarks.Text = mark.Remarks;
                
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

                var query = @"INSERT INTO Marks (StudentId, ExamId, MarksObtained, Grade, Remarks, RecordedBy) 
                             VALUES (@studentId, @examId, @marksObtained, @grade, @remarks, @recordedBy)";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", (int)cboStudent.SelectedValue);
                command.Parameters.AddWithValue("@examId", (int)cboExam.SelectedValue);
                command.Parameters.AddWithValue("@marksObtained", nudMarksObtained.Value);
                command.Parameters.AddWithValue("@grade", CalculateGrade(nudMarksObtained.Value, GetMaxMarks()));
                command.Parameters.AddWithValue("@remarks", txtRemarks.Text.Trim());
                command.Parameters.AddWithValue("@recordedBy", CurrentUser?.Id ?? 1);

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    ShowSuccess("Mark added successfully!");
                    ClearForm();
                    LoadMarks();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to add mark: {ex.Message}");
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedMark == null || !ValidateInput()) return;

                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"UPDATE Marks 
                             SET StudentId = @studentId, ExamId = @examId, MarksObtained = @marksObtained, 
                                 Grade = @grade, Remarks = @remarks 
                             WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", (int)cboStudent.SelectedValue);
                command.Parameters.AddWithValue("@examId", (int)cboExam.SelectedValue);
                command.Parameters.AddWithValue("@marksObtained", nudMarksObtained.Value);
                command.Parameters.AddWithValue("@grade", CalculateGrade(nudMarksObtained.Value, GetMaxMarks()));
                command.Parameters.AddWithValue("@remarks", txtRemarks.Text.Trim());
                command.Parameters.AddWithValue("@id", _selectedMark.Id);

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    ShowSuccess("Mark updated successfully!");
                    ClearForm();
                    LoadMarks();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update mark: {ex.Message}");
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedMark == null) return;

                if (ConfirmAction($"Are you sure you want to delete this mark record?"))
                {
                    using var connection = _context.GetConnection();
                    await connection.OpenAsync();

                    var query = "DELETE FROM Marks WHERE Id = @id";

                    using var command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@id", _selectedMark.Id);

                    var result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        ShowSuccess("Mark deleted successfully!");
                        ClearForm();
                        LoadMarks();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to delete mark: {ex.Message}");
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            cboExam.SelectedIndex = -1;
            cboStudent.SelectedIndex = -1;
            nudMarksObtained.Value = 0;
            txtGrade.Clear();
            txtRemarks.Clear();
            _selectedMark = null;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            dgvMarks.ClearSelection();
        }

        private bool ValidateInput()
        {
            if (cboExam.SelectedValue == null)
            {
                ShowWarning("Please select an exam.");
                cboExam.Focus();
                return false;
            }

            if (cboStudent.SelectedValue == null)
            {
                ShowWarning("Please select a student.");
                cboStudent.Focus();
                return false;
            }

            var maxMarks = GetMaxMarks();
            if (nudMarksObtained.Value > maxMarks)
            {
                ShowWarning($"Marks obtained cannot exceed maximum marks ({maxMarks}).");
                nudMarksObtained.Focus();
                return false;
            }

            return true;
        }

        private decimal GetMaxMarks()
        {
            if (cboExam.SelectedItem is Exam exam)
            {
                return exam.MaxMarks;
            }
            return 100;
        }

        private string CalculateGrade(decimal marksObtained, decimal maxMarks)
        {
            var percentage = (marksObtained / maxMarks) * 100;
            
            return percentage switch
            {
                >= 90 => "A+",
                >= 80 => "A",
                >= 70 => "B+",
                >= 60 => "B",
                >= 50 => "C+",
                >= 40 => "C",
                >= 30 => "D",
                _ => "F"
            };
        }

        private void cboExam_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboExam.SelectedItem is Exam exam)
            {
                nudMarksObtained.Maximum = exam.MaxMarks;
                txtGrade.Text = CalculateGrade(nudMarksObtained.Value, exam.MaxMarks);
            }
        }

        private void nudMarksObtained_ValueChanged(object sender, EventArgs e)
        {
            txtGrade.Text = CalculateGrade(nudMarksObtained.Value, GetMaxMarks());
        }

        private void InitializeComponent()
        {
            this.dgvMarks = new DataGridView();
            this.cboExam = new ComboBox();
            this.cboStudent = new ComboBox();
            this.nudMarksObtained = new NumericUpDown();
            this.txtGrade = new TextBox();
            this.txtRemarks = new TextBox();
            this.btnAdd = new Button();
            this.btnUpdate = new Button();
            this.btnDelete = new Button();
            this.btnClear = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMarks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarksObtained)).BeginInit();
            this.SuspendLayout();

            // 
            // MarkManagementForm
            // 
            this.ClientSize = new Size(1000, 650);
            this.Text = "Mark Management";

            // 
            // dgvMarks
            // 
            this.dgvMarks.AllowUserToAddRows = false;
            this.dgvMarks.AllowUserToDeleteRows = false;
            this.dgvMarks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMarks.BackgroundColor = Color.White;
            this.dgvMarks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMarks.Location = new Point(20, 20);
            this.dgvMarks.MultiSelect = false;
            this.dgvMarks.ReadOnly = true;
            this.dgvMarks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvMarks.Size = new Size(650, 400);
            this.dgvMarks.TabIndex = 0;
            this.dgvMarks.SelectionChanged += new EventHandler(this.dgvMarks_SelectionChanged);

            // Form controls
            var lblExam = new Label() { Text = "Exam:", Location = new Point(700, 50), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.cboExam.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboExam.Location = new Point(700, 75);
            this.cboExam.Size = new Size(250, 23);
            this.cboExam.Font = new Font("Segoe UI", 10F);
            this.cboExam.SelectedIndexChanged += new EventHandler(this.cboExam_SelectedIndexChanged);

            var lblStudent = new Label() { Text = "Student:", Location = new Point(700, 110), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.cboStudent.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboStudent.Location = new Point(700, 135);
            this.cboStudent.Size = new Size(250, 23);
            this.cboStudent.Font = new Font("Segoe UI", 10F);

            var lblMarksObtained = new Label() { Text = "Marks Obtained:", Location = new Point(700, 170), Size = new Size(120, 23), Font = new Font("Segoe UI", 10F) };
            this.nudMarksObtained.Location = new Point(700, 195);
            this.nudMarksObtained.Size = new Size(100, 23);
            this.nudMarksObtained.Minimum = 0;
            this.nudMarksObtained.Maximum = 1000;
            this.nudMarksObtained.DecimalPlaces = 2;
            this.nudMarksObtained.Font = new Font("Segoe UI", 10F);
            this.nudMarksObtained.ValueChanged += new EventHandler(this.nudMarksObtained_ValueChanged);

            var lblGrade = new Label() { Text = "Grade:", Location = new Point(820, 170), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtGrade.Location = new Point(820, 195);
            this.txtGrade.Size = new Size(80, 23);
            this.txtGrade.ReadOnly = true;
            this.txtGrade.Font = new Font("Segoe UI", 10F);

            var lblRemarks = new Label() { Text = "Remarks:", Location = new Point(700, 230), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtRemarks.Location = new Point(700, 255);
            this.txtRemarks.Size = new Size(250, 60);
            this.txtRemarks.Multiline = true;
            this.txtRemarks.Font = new Font("Segoe UI", 10F);

            // Buttons
            this.btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnAdd.ForeColor = Color.White;
            this.btnAdd.Location = new Point(700, 340);
            this.btnAdd.Size = new Size(80, 35);
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);

            this.btnUpdate.BackColor = Color.FromArgb(0, 123, 255);
            this.btnUpdate.Enabled = false;
            this.btnUpdate.FlatStyle = FlatStyle.Flat;
            this.btnUpdate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnUpdate.ForeColor = Color.White;
            this.btnUpdate.Location = new Point(790, 340);
            this.btnUpdate.Size = new Size(80, 35);
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new EventHandler(this.btnUpdate_Click);

            this.btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            this.btnDelete.Enabled = false;
            this.btnDelete.FlatStyle = FlatStyle.Flat;
            this.btnDelete.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.Location = new Point(880, 340);
            this.btnDelete.Size = new Size(80, 35);
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            this.btnClear.BackColor = Color.FromArgb(108, 117, 125);
            this.btnClear.FlatStyle = FlatStyle.Flat;
            this.btnClear.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnClear.ForeColor = Color.White;
            this.btnClear.Location = new Point(700, 390);
            this.btnClear.Size = new Size(80, 35);
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // Add controls to form
            this.Controls.Add(this.dgvMarks);
            this.Controls.Add(lblExam);
            this.Controls.Add(this.cboExam);
            this.Controls.Add(lblStudent);
            this.Controls.Add(this.cboStudent);
            this.Controls.Add(lblMarksObtained);
            this.Controls.Add(this.nudMarksObtained);
            this.Controls.Add(lblGrade);
            this.Controls.Add(this.txtGrade);
            this.Controls.Add(lblRemarks);
            this.Controls.Add(this.txtRemarks);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnClear);

            ((System.ComponentModel.ISupportInitialize)(this.dgvMarks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarksObtained)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}