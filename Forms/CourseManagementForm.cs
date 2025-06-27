using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Repositories;

namespace SchoolManagementSystem.Forms
{
    public partial class CourseManagementForm : BaseForm
    {
        private readonly CourseRepository _courseRepository;
        private DataGridView dgvCourses;
        private TextBox txtName;
        private TextBox txtCode;
        private TextBox txtDescription;
        private NumericUpDown nudDuration;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private Course? _selectedCourse;

        public CourseManagementForm(User currentUser) : base(currentUser)
        {
            var context = new DatabaseContext();
            _courseRepository = new CourseRepository(context);
            InitializeComponent();
            LoadCourses();
        }

        private async void LoadCourses()
        {
            try
            {
                var courses = await _courseRepository.GetAllAsync();
                dgvCourses.DataSource = courses;
                
                // Hide unnecessary columns
                if (dgvCourses.Columns["Id"] != null)
                    dgvCourses.Columns["Id"].Visible = false;
                if (dgvCourses.Columns["IsActive"] != null)
                    dgvCourses.Columns["IsActive"].Visible = false;
                if (dgvCourses.Columns["CreatedDate"] != null)
                    dgvCourses.Columns["CreatedDate"].Visible = false;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load courses: {ex.Message}");
            }
        }

        private void dgvCourses_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCourses.CurrentRow?.DataBoundItem is Course course)
            {
                _selectedCourse = course;
                txtName.Text = course.Name;
                txtCode.Text = course.Code;
                txtDescription.Text = course.Description;
                nudDuration.Value = course.Duration;
                
                btnUpdate.Enabled = true;
                btnDelete.Enabled = HasPermission(UserRole.Admin);
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                var course = new Course
                {
                    Name = txtName.Text.Trim(),
                    Code = txtCode.Text.Trim(),
                    Description = txtDescription.Text.Trim(),
                    Duration = (int)nudDuration.Value
                };

                var result = await _courseRepository.AddAsync(course);
                if (result > 0)
                {
                    ShowSuccess("Course added successfully!");
                    ClearForm();
                    LoadCourses();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to add course: {ex.Message}");
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedCourse == null || !ValidateInput()) return;

                _selectedCourse.Name = txtName.Text.Trim();
                _selectedCourse.Code = txtCode.Text.Trim();
                _selectedCourse.Description = txtDescription.Text.Trim();
                _selectedCourse.Duration = (int)nudDuration.Value;

                var result = await _courseRepository.UpdateAsync(_selectedCourse);
                if (result)
                {
                    ShowSuccess("Course updated successfully!");
                    ClearForm();
                    LoadCourses();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update course: {ex.Message}");
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedCourse == null) return;

                if (ConfirmAction($"Are you sure you want to delete the course '{_selectedCourse.Name}'?"))
                {
                    var result = await _courseRepository.DeleteAsync(_selectedCourse.Id);
                    if (result)
                    {
                        ShowSuccess("Course deleted successfully!");
                        ClearForm();
                        LoadCourses();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to delete course: {ex.Message}");
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtName.Clear();
            txtCode.Clear();
            txtDescription.Clear();
            nudDuration.Value = 12;
            _selectedCourse = null;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            dgvCourses.ClearSelection();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowWarning("Please enter course name.");
                txtName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                ShowWarning("Please enter course code.");
                txtCode.Focus();
                return false;
            }

            if (nudDuration.Value <= 0)
            {
                ShowWarning("Please enter a valid duration.");
                nudDuration.Focus();
                return false;
            }

            return true;
        }

        private void InitializeComponent()
        {
            this.dgvCourses = new DataGridView();
            this.txtName = new TextBox();
            this.txtCode = new TextBox();
            this.txtDescription = new TextBox();
            this.nudDuration = new NumericUpDown();
            this.btnAdd = new Button();
            this.btnUpdate = new Button();
            this.btnDelete = new Button();
            this.btnClear = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCourses)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDuration)).BeginInit();
            this.SuspendLayout();

            // 
            // CourseManagementForm
            // 
            this.ClientSize = new Size(900, 600);
            this.Text = "Course Management";

            // 
            // dgvCourses
            // 
            this.dgvCourses.AllowUserToAddRows = false;
            this.dgvCourses.AllowUserToDeleteRows = false;
            this.dgvCourses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCourses.BackgroundColor = Color.White;
            this.dgvCourses.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCourses.Location = new Point(20, 20);
            this.dgvCourses.MultiSelect = false;
            this.dgvCourses.ReadOnly = true;
            this.dgvCourses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvCourses.Size = new Size(550, 400);
            this.dgvCourses.TabIndex = 0;
            this.dgvCourses.SelectionChanged += new EventHandler(this.dgvCourses_SelectionChanged);

            // Form controls
            var lblName = new Label() { Text = "Name:", Location = new Point(600, 50), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtName.Location = new Point(600, 75);
            this.txtName.Size = new Size(250, 23);
            this.txtName.Font = new Font("Segoe UI", 10F);

            var lblCode = new Label() { Text = "Code:", Location = new Point(600, 110), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtCode.Location = new Point(600, 135);
            this.txtCode.Size = new Size(250, 23);
            this.txtCode.Font = new Font("Segoe UI", 10F);

            var lblDescription = new Label() { Text = "Description:", Location = new Point(600, 170), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtDescription.Location = new Point(600, 195);
            this.txtDescription.Size = new Size(250, 60);
            this.txtDescription.Multiline = true;
            this.txtDescription.Font = new Font("Segoe UI", 10F);

            var lblDuration = new Label() { Text = "Duration (months):", Location = new Point(600, 270), Size = new Size(120, 23), Font = new Font("Segoe UI", 10F) };
            this.nudDuration.Location = new Point(600, 295);
            this.nudDuration.Size = new Size(100, 23);
            this.nudDuration.Minimum = 1;
            this.nudDuration.Maximum = 120;
            this.nudDuration.Value = 12;
            this.nudDuration.Font = new Font("Segoe UI", 10F);

            // Buttons
            this.btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnAdd.ForeColor = Color.White;
            this.btnAdd.Location = new Point(600, 350);
            this.btnAdd.Size = new Size(80, 35);
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);

            this.btnUpdate.BackColor = Color.FromArgb(0, 123, 255);
            this.btnUpdate.Enabled = false;
            this.btnUpdate.FlatStyle = FlatStyle.Flat;
            this.btnUpdate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnUpdate.ForeColor = Color.White;
            this.btnUpdate.Location = new Point(690, 350);
            this.btnUpdate.Size = new Size(80, 35);
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new EventHandler(this.btnUpdate_Click);

            this.btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            this.btnDelete.Enabled = false;
            this.btnDelete.FlatStyle = FlatStyle.Flat;
            this.btnDelete.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.Location = new Point(780, 350);
            this.btnDelete.Size = new Size(80, 35);
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            this.btnClear.BackColor = Color.FromArgb(108, 117, 125);
            this.btnClear.FlatStyle = FlatStyle.Flat;
            this.btnClear.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnClear.ForeColor = Color.White;
            this.btnClear.Location = new Point(600, 400);
            this.btnClear.Size = new Size(80, 35);
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // Add controls to form
            this.Controls.Add(this.dgvCourses);
            this.Controls.Add(lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(lblCode);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(lblDescription);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(lblDuration);
            this.Controls.Add(this.nudDuration);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnClear);

            ((System.ComponentModel.ISupportInitialize)(this.dgvCourses)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDuration)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}