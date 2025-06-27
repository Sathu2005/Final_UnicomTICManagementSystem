using System.Data.SQLite;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Repositories;

namespace SchoolManagementSystem.Forms
{
    public partial class TimetableForm : BaseForm
    {
        private readonly DatabaseContext _context;
        private readonly SubjectRepository _subjectRepository;
        private DataGridView dgvTimetable;
        private ComboBox cboSubject;
        private ComboBox cboRoom;
        private ComboBox cboDayOfWeek;
        private DateTimePicker dtpStartTime;
        private DateTimePicker dtpEndTime;
        private ComboBox cboLecturer;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private Timetable? _selectedTimetable;

        public TimetableForm(User currentUser) : base(currentUser)
        {
            _context = new DatabaseContext();
            _subjectRepository = new SubjectRepository(_context);
            InitializeComponent();
            LoadSubjects();
            LoadRooms();
            LoadLecturers();
            LoadDaysOfWeek();
            LoadTimetable();
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

        private async void LoadLecturers()
        {
            try
            {
                var lecturers = new List<User>();
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT * FROM Users WHERE Role = 2 AND IsActive = 1 ORDER BY FullName"; // Role 2 = Lecturer
                using var command = new SQLiteCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    lecturers.Add(new User
                    {
                        Id = reader.GetInt32("Id"),
                        Username = reader.GetString("Username"),
                        FullName = reader.GetString("FullName"),
                        Email = reader.GetString("Email"),
                        Role = (UserRole)reader.GetInt32("Role"),
                        IsActive = reader.GetBoolean("IsActive")
                    });
                }

                cboLecturer.DataSource = lecturers;
                cboLecturer.DisplayMember = "FullName";
                cboLecturer.ValueMember = "Id";
                cboLecturer.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load lecturers: {ex.Message}");
            }
        }

        private void LoadDaysOfWeek()
        {
            var days = Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(d => new { Value = (int)d, Name = d.ToString() })
                .ToList();

            cboDayOfWeek.DataSource = days;
            cboDayOfWeek.DisplayMember = "Name";
            cboDayOfWeek.ValueMember = "Value";
            cboDayOfWeek.SelectedIndex = -1;
        }

        private async void LoadTimetable()
        {
            try
            {
                var timetables = new List<Timetable>();
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"SELECT t.*, s.Name as SubjectName, r.Name as RoomName, 
                                     u.FullName as LecturerName, c.Name as CourseName 
                             FROM Timetables t 
                             INNER JOIN Subjects s ON t.SubjectId = s.Id 
                             INNER JOIN Rooms r ON t.RoomId = r.Id 
                             INNER JOIN Users u ON t.LecturerId = u.Id 
                             INNER JOIN Courses c ON s.CourseId = c.Id 
                             WHERE t.IsActive = 1 
                             ORDER BY t.DayOfWeek, t.StartTime";

                using var command = new SQLiteCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    timetables.Add(new Timetable
                    {
                        Id = reader.GetInt32("Id"),
                        SubjectId = reader.GetInt32("SubjectId"),
                        RoomId = reader.GetInt32("RoomId"),
                        DayOfWeek = (DayOfWeek)reader.GetInt32("DayOfWeek"),
                        StartTime = TimeSpan.Parse(reader.GetString("StartTime")),
                        EndTime = TimeSpan.Parse(reader.GetString("EndTime")),
                        LecturerId = reader.GetInt32("LecturerId"),
                        IsActive = reader.GetBoolean("IsActive"),
                        SubjectName = reader.GetString("SubjectName"),
                        RoomName = reader.GetString("RoomName"),
                        LecturerName = reader.GetString("LecturerName"),
                        CourseName = reader.GetString("CourseName")
                    });
                }

                dgvTimetable.DataSource = timetables;
                
                // Hide unnecessary columns
                if (dgvTimetable.Columns["Id"] != null)
                    dgvTimetable.Columns["Id"].Visible = false;
                if (dgvTimetable.Columns["SubjectId"] != null)
                    dgvTimetable.Columns["SubjectId"].Visible = false;
                if (dgvTimetable.Columns["RoomId"] != null)
                    dgvTimetable.Columns["RoomId"].Visible = false;
                if (dgvTimetable.Columns["LecturerId"] != null)
                    dgvTimetable.Columns["LecturerId"].Visible = false;
                if (dgvTimetable.Columns["IsActive"] != null)
                    dgvTimetable.Columns["IsActive"].Visible = false;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load timetable: {ex.Message}");
            }
        }

        private void dgvTimetable_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvTimetable.CurrentRow?.DataBoundItem is Timetable timetable)
            {
                _selectedTimetable = timetable;
                cboSubject.SelectedValue = timetable.SubjectId;
                cboRoom.SelectedValue = timetable.RoomId;
                cboDayOfWeek.SelectedValue = (int)timetable.DayOfWeek;
                dtpStartTime.Value = DateTime.Today.Add(timetable.StartTime);
                dtpEndTime.Value = DateTime.Today.Add(timetable.EndTime);
                cboLecturer.SelectedValue = timetable.LecturerId;
                
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

                var query = @"INSERT INTO Timetables (SubjectId, RoomId, DayOfWeek, StartTime, EndTime, LecturerId) 
                             VALUES (@subjectId, @roomId, @dayOfWeek, @startTime, @endTime, @lecturerId)";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@subjectId", (int)cboSubject.SelectedValue);
                command.Parameters.AddWithValue("@roomId", (int)cboRoom.SelectedValue);
                command.Parameters.AddWithValue("@dayOfWeek", (int)cboDayOfWeek.SelectedValue);
                command.Parameters.AddWithValue("@startTime", dtpStartTime.Value.TimeOfDay.ToString());
                command.Parameters.AddWithValue("@endTime", dtpEndTime.Value.TimeOfDay.ToString());
                command.Parameters.AddWithValue("@lecturerId", (int)cboLecturer.SelectedValue);

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    ShowSuccess("Timetable entry added successfully!");
                    ClearForm();
                    LoadTimetable();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to add timetable entry: {ex.Message}");
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedTimetable == null || !ValidateInput()) return;

                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"UPDATE Timetables 
                             SET SubjectId = @subjectId, RoomId = @roomId, DayOfWeek = @dayOfWeek, 
                                 StartTime = @startTime, EndTime = @endTime, LecturerId = @lecturerId 
                             WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@subjectId", (int)cboSubject.SelectedValue);
                command.Parameters.AddWithValue("@roomId", (int)cboRoom.SelectedValue);
                command.Parameters.AddWithValue("@dayOfWeek", (int)cboDayOfWeek.SelectedValue);
                command.Parameters.AddWithValue("@startTime", dtpStartTime.Value.TimeOfDay.ToString());
                command.Parameters.AddWithValue("@endTime", dtpEndTime.Value.TimeOfDay.ToString());
                command.Parameters.AddWithValue("@lecturerId", (int)cboLecturer.SelectedValue);
                command.Parameters.AddWithValue("@id", _selectedTimetable.Id);

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    ShowSuccess("Timetable entry updated successfully!");
                    ClearForm();
                    LoadTimetable();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update timetable entry: {ex.Message}");
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedTimetable == null) return;

                if (ConfirmAction("Are you sure you want to delete this timetable entry?"))
                {
                    using var connection = _context.GetConnection();
                    await connection.OpenAsync();

                    var query = "UPDATE Timetables SET IsActive = 0 WHERE Id = @id";

                    using var command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@id", _selectedTimetable.Id);

                    var result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        ShowSuccess("Timetable entry deleted successfully!");
                        ClearForm();
                        LoadTimetable();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to delete timetable entry: {ex.Message}");
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            cboSubject.SelectedIndex = -1;
            cboRoom.SelectedIndex = -1;
            cboDayOfWeek.SelectedIndex = -1;
            dtpStartTime.Value = DateTime.Today.AddHours(9);
            dtpEndTime.Value = DateTime.Today.AddHours(10);
            cboLecturer.SelectedIndex = -1;
            _selectedTimetable = null;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            dgvTimetable.ClearSelection();
        }

        private bool ValidateInput()
        {
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

            if (cboDayOfWeek.SelectedValue == null)
            {
                ShowWarning("Please select day of week.");
                cboDayOfWeek.Focus();
                return false;
            }

            if (dtpStartTime.Value >= dtpEndTime.Value)
            {
                ShowWarning("End time must be after start time.");
                dtpEndTime.Focus();
                return false;
            }

            if (cboLecturer.SelectedValue == null)
            {
                ShowWarning("Please select a lecturer.");
                cboLecturer.Focus();
                return false;
            }

            return true;
        }

        private void InitializeComponent()
        {
            this.dgvTimetable = new DataGridView();
            this.cboSubject = new ComboBox();
            this.cboRoom = new ComboBox();
            this.cboDayOfWeek = new ComboBox();
            this.dtpStartTime = new DateTimePicker();
            this.dtpEndTime = new DateTimePicker();
            this.cboLecturer = new ComboBox();
            this.btnAdd = new Button();
            this.btnUpdate = new Button();
            this.btnDelete = new Button();
            this.btnClear = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTimetable)).BeginInit();
            this.SuspendLayout();

            // 
            // TimetableForm
            // 
            this.ClientSize = new Size(1100, 700);
            this.Text = "Timetable Management";

            // 
            // dgvTimetable
            // 
            this.dgvTimetable.AllowUserToAddRows = false;
            this.dgvTimetable.AllowUserToDeleteRows = false;
            this.dgvTimetable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTimetable.BackgroundColor = Color.White;
            this.dgvTimetable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTimetable.Location = new Point(20, 20);
            this.dgvTimetable.MultiSelect = false;
            this.dgvTimetable.ReadOnly = true;
            this.dgvTimetable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvTimetable.Size = new Size(750, 400);
            this.dgvTimetable.TabIndex = 0;
            this.dgvTimetable.SelectionChanged += new EventHandler(this.dgvTimetable_SelectionChanged);

            // Form controls
            var lblSubject = new Label() { Text = "Subject:", Location = new Point(800, 50), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.cboSubject.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboSubject.Location = new Point(800, 75);
            this.cboSubject.Size = new Size(250, 23);
            this.cboSubject.Font = new Font("Segoe UI", 10F);

            var lblRoom = new Label() { Text = "Room:", Location = new Point(800, 110), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.cboRoom.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboRoom.Location = new Point(800, 135);
            this.cboRoom.Size = new Size(250, 23);
            this.cboRoom.Font = new Font("Segoe UI", 10F);

            var lblDayOfWeek = new Label() { Text = "Day:", Location = new Point(800, 170), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.cboDayOfWeek.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboDayOfWeek.Location = new Point(800, 195);
            this.cboDayOfWeek.Size = new Size(250, 23);
            this.cboDayOfWeek.Font = new Font("Segoe UI", 10F);

            var lblStartTime = new Label() { Text = "Start Time:", Location = new Point(800, 230), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.dtpStartTime.Format = DateTimePickerFormat.Time;
            this.dtpStartTime.ShowUpDown = true;
            this.dtpStartTime.Location = new Point(800, 255);
            this.dtpStartTime.Size = new Size(120, 23);
            this.dtpStartTime.Font = new Font("Segoe UI", 10F);
            this.dtpStartTime.Value = DateTime.Today.AddHours(9);

            var lblEndTime = new Label() { Text = "End Time:", Location = new Point(930, 230), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.dtpEndTime.Format = DateTimePickerFormat.Time;
            this.dtpEndTime.ShowUpDown = true;
            this.dtpEndTime.Location = new Point(930, 255);
            this.dtpEndTime.Size = new Size(120, 23);
            this.dtpEndTime.Font = new Font("Segoe UI", 10F);
            this.dtpEndTime.Value = DateTime.Today.AddHours(10);

            var lblLecturer = new Label() { Text = "Lecturer:", Location = new Point(800, 290), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.cboLecturer.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboLecturer.Location = new Point(800, 315);
            this.cboLecturer.Size = new Size(250, 23);
            this.cboLecturer.Font = new Font("Segoe UI", 10F);

            // Buttons
            this.btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnAdd.ForeColor = Color.White;
            this.btnAdd.Location = new Point(800, 360);
            this.btnAdd.Size = new Size(80, 35);
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);

            this.btnUpdate.BackColor = Color.FromArgb(0, 123, 255);
            this.btnUpdate.Enabled = false;
            this.btnUpdate.FlatStyle = FlatStyle.Flat;
            this.btnUpdate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnUpdate.ForeColor = Color.White;
            this.btnUpdate.Location = new Point(890, 360);
            this.btnUpdate.Size = new Size(80, 35);
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new EventHandler(this.btnUpdate_Click);

            this.btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            this.btnDelete.Enabled = false;
            this.btnDelete.FlatStyle = FlatStyle.Flat;
            this.btnDelete.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.Location = new Point(980, 360);
            this.btnDelete.Size = new Size(80, 35);
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            this.btnClear.BackColor = Color.FromArgb(108, 117, 125);
            this.btnClear.FlatStyle = FlatStyle.Flat;
            this.btnClear.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnClear.ForeColor = Color.White;
            this.btnClear.Location = new Point(800, 410);
            this.btnClear.Size = new Size(80, 35);
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // Add controls to form
            this.Controls.Add(this.dgvTimetable);
            this.Controls.Add(lblSubject);
            this.Controls.Add(this.cboSubject);
            this.Controls.Add(lblRoom);
            this.Controls.Add(this.cboRoom);
            this.Controls.Add(lblDayOfWeek);
            this.Controls.Add(this.cboDayOfWeek);
            this.Controls.Add(lblStartTime);
            this.Controls.Add(this.dtpStartTime);
            this.Controls.Add(lblEndTime);
            this.Controls.Add(this.dtpEndTime);
            this.Controls.Add(lblLecturer);
            this.Controls.Add(this.cboLecturer);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnClear);

            ((System.ComponentModel.ISupportInitialize)(this.dgvTimetable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}