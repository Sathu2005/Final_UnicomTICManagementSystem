using System.Data.SQLite;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Repositories;

namespace SchoolManagementSystem.Forms
{
    public partial class ExamManagementForm : BaseForm
    {
        private readonly DatabaseContext _context;
        private readonly SubjectRepository _subjectRepository;
        private DataGridView dgvExams;
        private TextBox txtName;
        private ComboBox cboSubject;
        private DateTimePicker dtpExamDate;
        private DateTimePicker dtpStartTime;
        private DateTimePicker dtpEndTime;
        private ComboBox cboRoom;
        private NumericUpDown nudMaxMarks;
        private TextBox txtDescription;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private Exam? _selectedExam;

        public ExamManagementForm(User currentUser) : base(currentUser)
        {
            _context = new DatabaseContext();
            _subjectRepository = new SubjectRepository(_context);
            InitializeComponent();
            LoadSubjects();
            LoadRooms();
            LoadExams();
        }

        private async void LoadSubjects()
        {
            try
            {
                var subjects = await _subjectRepository.GetAllAsync();
                cboSubject.DataSource = subjects;
                cboSubject.DisplayMember = "Name";
                cboSubject.ValueMember = "Id";
                cboSubject.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load subjects: {ex.Message}");
            }
        }

        private async void LoadRooms()
        {
            try
            {
                var rooms = new List<Room>();
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT * FROM Rooms WHERE IsActive = 1 ORDER BY Name";
                using var command = new SQLiteCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rooms.Add(new Room
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Code = reader.GetString("Code"),
                        Type = (RoomType)reader.GetInt32("Type"),
                        Capacity = reader.GetInt32("Capacity"),
                        Location = reader.IsDBNull("Location") ? "" : reader.GetString("Location"),
                        Equipment = reader.IsDBNull("Equipment") ? "" : reader.GetString("Equipment"),
                        IsActive = reader.GetBoolean("IsActive")
                    });
                }

                cboRoom.DataSource = rooms;
                cboRoom.DisplayMember = "Name";
                cboRoom.ValueMember = "Id";
                cboRoom.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load rooms: {ex.Message}");
            }
        }

        private async void LoadExams()
        {
            try
            {
                var exams = new List<Exam>();
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"SELECT e.*, s.Name as SubjectName, r.Name as RoomName 
                             FROM Exams e 
                             INNER JOIN Subjects s ON e.SubjectId = s.Id 
                             INNER JOIN Rooms r ON e.RoomId = r.Id 
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
                        StartTime = TimeSpan.Parse(reader.GetString("StartTime")),
                        EndTime = TimeSpan.Parse(reader.GetString("EndTime")),
                        RoomId = reader.GetInt32("RoomId"),
                        MaxMarks = reader.GetInt32("MaxMarks"),
                        Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                        IsActive = reader.GetBoolean("IsActive"),
                        SubjectName = reader.GetString("SubjectName"),
                        RoomName = reader.GetString("RoomName")
                    });
                }

                dgvExams.DataSource = exams;
                
                // Hide unnecessary columns
                if (dgvExams.Columns["Id"] != null)
                    dgvExams.Columns["Id"].Visible = false;
                if (dgvExams.Columns["SubjectId"] != null)
                    dgvExams.Columns["SubjectId"].Visible = false;
                if (dgvExams.Columns["RoomId"] != null)
                    dgvExams.Columns["RoomId"].Visible = false;
                if (dgvExams.Columns["IsActive"] != null)
                    dgvExams.Columns["IsActive"].Visible = false;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load exams: {ex.Message}");
            }
        }

        private void dgvExams_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvExams.CurrentRow?.DataBoundItem is Exam exam)
            {
                _selectedExam = exam;
                txtName.Text = exam.Name;
                cboSubject.SelectedValue = exam.SubjectId;
                dtpExamDate.Value = exam.ExamDate;
                dtpStartTime.Value = DateTime.Today.Add(exam.StartTime);
                dtpEndTime.Value = DateTime.Today.Add(exam.EndTime);
                cboRoom.SelectedValue = exam.RoomId;
                nudMaxMarks.Value = exam.MaxMarks;
                txtDescription.Text = exam.Description;
                
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

                var query = @"INSERT INTO Exams (Name, SubjectId, ExamDate, StartTime, EndTime, RoomId, MaxMarks, Description) 
                             VALUES (@name, @subjectId, @examDate, @startTime, @endTime, @roomId, @maxMarks, @description)";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@name", txtName.Text.Trim());
                command.Parameters.AddWithValue("@subjectId", (int)cboSubject.SelectedValue);
                command.Parameters.AddWithValue("@examDate", dtpExamDate.Value.Date);
                command.Parameters.AddWithValue("@startTime", dtpStartTime.Value.TimeOfDay.ToString());
                command.Parameters.AddWithValue("@endTime", dtpEndTime.Value.TimeOfDay.ToString());
                command.Parameters.AddWithValue("@roomId", (int)cboRoom.SelectedValue);
                command.Parameters.AddWithValue("@maxMarks", (int)nudMaxMarks.Value);
                command.Parameters.AddWithValue("@description", txtDescription.Text.Trim());

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    ShowSuccess("Exam added successfully!");
                    ClearForm();
                    LoadExams();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to add exam: {ex.Message}");
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedExam == null || !ValidateInput()) return;

                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"UPDATE Exams 
                             SET Name = @name, SubjectId = @subjectId, ExamDate = @examDate, 
                                 StartTime = @startTime, EndTime = @endTime, RoomId = @roomId, 
                                 MaxMarks = @maxMarks, Description = @description 
                             WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@name", txtName.Text.Trim());
                command.Parameters.AddWithValue("@subjectId", (int)cboSubject.SelectedValue);
                command.Parameters.AddWithValue("@examDate", dtpExamDate.Value.Date);
                command.Parameters.AddWithValue("@startTime", dtpStartTime.Value.TimeOfDay.ToString());
                command.Parameters.AddWithValue("@endTime", dtpEndTime.Value.TimeOfDay.ToString());
                command.Parameters.AddWithValue("@roomId", (int)cboRoom.SelectedValue);
                command.Parameters.AddWithValue("@maxMarks", (int)nudMaxMarks.Value);
                command.Parameters.AddWithValue("@description", txtDescription.Text.Trim());
                command.Parameters.AddWithValue("@id", _selectedExam.Id);

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    ShowSuccess("Exam updated successfully!");
                    ClearForm();
                    LoadExams();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update exam: {ex.Message}");
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedExam == null) return;

                if (ConfirmAction($"Are you sure you want to delete the exam '{_selectedExam.Name}'?"))
                {
                    using var connection = _context.GetConnection();
                    await connection.OpenAsync();

                    var query = "UPDATE Exams SET IsActive = 0 WHERE Id = @id";

                    using var command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@id", _selectedExam.Id);

                    var result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        ShowSuccess("Exam deleted successfully!");
                        ClearForm();
                        LoadExams();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to delete exam: {ex.Message}");
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtName.Clear();
            cboSubject.SelectedIndex = -1;
            dtpExamDate.Value = DateTime.Now.AddDays(7);
            dtpStartTime.Value = DateTime.Today.AddHours(9);
            dtpEndTime.Value = DateTime.Today.AddHours(12);
            cboRoom.SelectedIndex = -1;
            nudMaxMarks.Value = 100;
            txtDescription.Clear();
            _selectedExam = null;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            dgvExams.ClearSelection();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowWarning("Please enter exam name.");
                txtName.Focus();
                return false;
            }

            if (cboSubject.SelectedValue == null)
            {
                ShowWarning("Please select a subject.");
                cboSubject.Focus();
                return false;
            }

            if (cboRoom.SelectedValue == null)
            {
                ShowWarning("Please select a room.");
                cboRoom.Focus();
                return false;
            }

            if (dtpStartTime.Value >= dtpEndTime.Value)
            {
                ShowWarning("End time must be after start time.");
                dtpEndTime.Focus();
                return false;
            }

            if (nudMaxMarks.Value <= 0)
            {
                ShowWarning("Please enter valid maximum marks.");
                nudMaxMarks.Focus();
                return false;
            }

            return true;
        }

        private void InitializeComponent()
        {
            this.dgvExams = new DataGridView();
            this.txtName = new TextBox();
            this.cboSubject = new ComboBox();
            this.dtpExamDate = new DateTimePicker();
            this.dtpStartTime = new DateTimePicker();
            this.dtpEndTime = new DateTimePicker();
            this.cboRoom = new ComboBox();
            this.nudMaxMarks = new NumericUpDown();
            this.txtDescription = new TextBox();
            this.btnAdd = new Button();
            this.btnUpdate = new Button();
            this.btnDelete = new Button();
            this.btnClear = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvExams)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxMarks)).BeginInit();
            this.SuspendLayout();

            // 
            // ExamManagementForm
            // 
            this.ClientSize = new Size(1100, 700);
            this.Text = "Exam Management";

            // 
            // dgvExams
            // 
            this.dgvExams.AllowUserToAddRows = false;
            this.dgvExams.AllowUserToDeleteRows = false;
            this.dgvExams.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvExams.BackgroundColor = Color.White;
            this.dgvExams.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvExams.Location = new Point(20, 20);
            this.dgvExams.MultiSelect = false;
            this.dgvExams.ReadOnly = true;
            this.dgvExams.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvExams.Size = new Size(700, 400);
            this.dgvExams.TabIndex = 0;
            this.dgvExams.SelectionChanged += new EventHandler(this.dgvExams_SelectionChanged);

            // Form controls
            var lblName = new Label() { Text = "Exam Name:", Location = new Point(750, 50), Size = new Size(100, 23), Font = new Font("Segoe UI", 10F) };
            this.txtName.Location = new Point(750, 75);
            this.txtName.Size = new Size(300, 23);
            this.txtName.Font = new Font("Segoe UI", 10F);

            var lblSubject = new Label() { Text = "Subject:", Location = new Point(750, 110), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.cboSubject.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboSubject.Location = new Point(750, 135);
            this.cboSubject.Size = new Size(300, 23);
            this.cboSubject.Font = new Font("Segoe UI", 10F);

            var lblExamDate = new Label() { Text = "Exam Date:", Location = new Point(750, 170), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.dtpExamDate.Location = new Point(750, 195);
            this.dtpExamDate.Size = new Size(200, 23);
            this.dtpExamDate.Font = new Font("Segoe UI", 10F);
            this.dtpExamDate.Value = DateTime.Now.AddDays(7);

            var lblStartTime = new Label() { Text = "Start Time:", Location = new Point(750, 230), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.dtpStartTime.Format = DateTimePickerFormat.Time;
            this.dtpStartTime.ShowUpDown = true;
            this.dtpStartTime.Location = new Point(750, 255);
            this.dtpStartTime.Size = new Size(140, 23);
            this.dtpStartTime.Font = new Font("Segoe UI", 10F);
            this.dtpStartTime.Value = DateTime.Today.AddHours(9);

            var lblEndTime = new Label() { Text = "End Time:", Location = new Point(910, 230), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.dtpEndTime.Format = DateTimePickerFormat.Time;
            this.dtpEndTime.ShowUpDown = true;
            this.dtpEndTime.Location = new Point(910, 255);
            this.dtpEndTime.Size = new Size(140, 23);
            this.dtpEndTime.Font = new Font("Segoe UI", 10F);
            this.dtpEndTime.Value = DateTime.Today.AddHours(12);

            var lblRoom = new Label() { Text = "Room:", Location = new Point(750, 290), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.cboRoom.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboRoom.Location = new Point(750, 315);
            this.cboRoom.Size = new Size(300, 23);
            this.cboRoom.Font = new Font("Segoe UI", 10F);

            var lblMaxMarks = new Label() { Text = "Max Marks:", Location = new Point(750, 350), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.nudMaxMarks.Location = new Point(750, 375);
            this.nudMaxMarks.Size = new Size(100, 23);
            this.nudMaxMarks.Minimum = 1;
            this.nudMaxMarks.Maximum = 1000;
            this.nudMaxMarks.Value = 100;
            this.nudMaxMarks.Font = new Font("Segoe UI", 10F);

            var lblDescription = new Label() { Text = "Description:", Location = new Point(750, 410), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtDescription.Location = new Point(750, 435);
            this.txtDescription.Size = new Size(300, 60);
            this.txtDescription.Multiline = true;
            this.txtDescription.Font = new Font("Segoe UI", 10F);

            // Buttons
            this.btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnAdd.ForeColor = Color.White;
            this.btnAdd.Location = new Point(750, 520);
            this.btnAdd.Size = new Size(80, 35);
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);

            this.btnUpdate.BackColor = Color.FromArgb(0, 123, 255);
            this.btnUpdate.Enabled = false;
            this.btnUpdate.FlatStyle = FlatStyle.Flat;
            this.btnUpdate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnUpdate.ForeColor = Color.White;
            this.btnUpdate.Location = new Point(840, 520);
            this.btnUpdate.Size = new Size(80, 35);
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new EventHandler(this.btnUpdate_Click);

            this.btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            this.btnDelete.Enabled = false;
            this.btnDelete.FlatStyle = FlatStyle.Flat;
            this.btnDelete.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.Location = new Point(930, 520);
            this.btnDelete.Size = new Size(80, 35);
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            this.btnClear.BackColor = Color.FromArgb(108, 117, 125);
            this.btnClear.FlatStyle = FlatStyle.Flat;
            this.btnClear.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnClear.ForeColor = Color.White;
            this.btnClear.Location = new Point(750, 570);
            this.btnClear.Size = new Size(80, 35);
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // Add controls to form
            this.Controls.Add(this.dgvExams);
            this.Controls.Add(lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(lblSubject);
            this.Controls.Add(this.cboSubject);
            this.Controls.Add(lblExamDate);
            this.Controls.Add(this.dtpExamDate);
            this.Controls.Add(lblStartTime);
            this.Controls.Add(this.dtpStartTime);
            this.Controls.Add(lblEndTime);
            this.Controls.Add(this.dtpEndTime);
            this.Controls.Add(lblRoom);
            this.Controls.Add(this.cboRoom);
            this.Controls.Add(lblMaxMarks);
            this.Controls.Add(this.nudMaxMarks);
            this.Controls.Add(lblDescription);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnClear);

            ((System.ComponentModel.ISupportInitialize)(this.dgvExams)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxMarks)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}