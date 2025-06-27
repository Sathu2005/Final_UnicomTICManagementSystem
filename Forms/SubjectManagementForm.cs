using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Repositories;

namespace SchoolManagementSystem.Forms
{
    public partial class SubjectManagementForm : BaseForm
    {
        private readonly SubjectRepository _subjectRepository;
        private readonly CourseRepository _courseRepository;
        private DataGridView dgvSubjects;
        private TextBox txtName;
        private TextBox txtCode;
        private TextBox txtDescription;
        private ComboBox cboCourse;
        private NumericUpDown nudCredits;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private Subject? _selectedSubject;

        public SubjectManagementForm(User currentUser) : base(currentUser)
        {
            var context = new DatabaseContext();
            _subjectRepository = new SubjectRepository(context);
            _courseRepository = new CourseRepository(context);
            InitializeComponent();
            LoadCourses();
            LoadSubjects();
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

        private async void LoadSubjects()
        {
            try
            {
                var subjects = await _subjectRepository.GetAllAsync();
                dgvSubjects.DataSource = subjects;
                
                // Hide unnecessary columns
                if (dgvSubjects.Columns["Id"] != null)
                    dgvSubjects.Columns["Id"].Visible = false;
                if (dgvSubjects.Columns["CourseId"] != null)
                    dgvSubjects.Columns["CourseId"].Visible = false;
                if (dgvSubjects.Columns["IsActive"] != null)
                    dgvSubjects.Columns["IsActive"].Visible = false;
                if (dgvSubjects.Columns["CreatedDate"] != null)
                    dgvSubjects.Columns["CreatedDate"].Visible = false;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load subjects: {ex.Message}");
            }
        }

        private void dgvSubjects_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSubjects.CurrentRow?.DataBoundItem is Subject subject)
            {
                _selectedSubject = subject;
                txtName.Text = subject.Name;
                txtCode.Text = subject.Code;
                txtDescription.Text = subject.Description;
                cboCourse.SelectedValue = subject.CourseId;
                nudCredits.Value = subject.Credits;
                
                btnUpdate.Enabled = true;
                btnDelete.Enabled = HasPermission(UserRole.Admin);
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                var subject = new Subject
                {
                    Name = txtName.Text.Trim(),
                    Code = txtCode.Text.Trim(),
                    Description = txtDescription.Text.Trim(),
                    CourseId = (int)cboCourse.SelectedValue,
                    Credits = (int)nudCredits.Value
                };

                var result = await _subjectRepository.AddAsync(subject);
                if (result > 0)
                {
                    ShowSuccess("Subject added successfully!");
                    ClearForm();
                    LoadSubjects();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to add subject: {ex.Message}");
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedSubject == null || !ValidateInput()) return;

                _selectedSubject.Name = txtName.Text.Trim();
                _selectedSubject.Code = txtCode.Text.Trim();
                _selectedSubject.Description = txtDescription.Text.Trim();
                _selectedSubject.CourseId = (int)cboCourse.SelectedValue;
                _selectedSubject.Credits = (int)nudCredits.Value;

                var result = await _subjectRepository.UpdateAsync(_selectedSubject);
                if (result)
                {
                    ShowSuccess("Subject updated successfully!");
                    ClearForm();
                    LoadSubjects();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update subject: {ex.Message}");
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedSubject == null) return;

                if (ConfirmAction($"Are you sure you want to delete the subject '{_selectedSubject.Name}'?"))
                {
                    var result = await _subjectRepository.DeleteAsync(_selectedSubject.Id);
                    if (result)
                    {
                        ShowSuccess("Subject deleted successfully!");
                        ClearForm();
                        LoadSubjects();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to delete subject: {ex.Message}");
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
            cboCourse.SelectedIndex = -1;
            nudCredits.Value = 3;
            _selectedSubject = null;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            dgvSubjects.ClearSelection();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowWarning("Please enter subject name.");
                txtName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                ShowWarning("Please enter subject code.");
                txtCode.Focus();
                return false;
            }

            if (cboCourse.SelectedValue == null)
            {
                ShowWarning("Please select a course.");
                cboCourse.Focus();
                return false;
            }

            if (nudCredits.Value <= 0)
            {
                ShowWarning("Please enter valid credits.");
                nudCredits.Focus();
                return false;
            }

            return true;
        }

        private void InitializeComponent()
        {
            this.dgvSubjects = new DataGridView();
            this.txtName = new TextBox();
            this.txtCode = new TextBox();
            this.txtDescription = new TextBox();
            this.cboCourse = new ComboBox();
            this.nudCredits = new NumericUpDown();
            this.btnAdd = new Button();
            this.btnUpdate = new Button();
            this.btnDelete = new Button();
            this.btnClear = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSubjects)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCredits)).BeginInit();
            this.SuspendLayout();

            // 
            // SubjectManagementForm
            // 
            this.ClientSize = new Size(950, 600);
            this.Text = "Subject Management";

            // 
            // dgvSubjects
            // 
            this.dgvSubjects.AllowUserToAddRows = false;
            this.dgvSubjects.AllowUserToDeleteRows = false;
            this.dgvSubjects.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSubjects.BackgroundColor = Color.White;
            this.dgvSubjects.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSubjects.Location = new Point(20, 20);
            this.dgvSubjects.MultiSelect = false;
            this.dgvSubjects.ReadOnly = true;
            this.dgvSubjects.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvSubjects.Size = new Size(600, 400);
            this.dgvSubjects.TabIndex = 0;
            this.dgvSubjects.SelectionChanged += new EventHandler(this.dgvSubjects_SelectionChanged);

            // Form controls
            var lblName = new Label() { Text = "Name:", Location = new Point(650, 50), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtName.Location = new Point(650, 75);
            this.txtName.Size = new Size(250, 23);
            this.txtName.Font = new Font("Segoe UI", 10F);

            var lblCode = new Label() { Text = "Code:", Location = new Point(650, 110), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtCode.Location = new Point(650, 135);
            this.txtCode.Size = new Size(250, 23);
            this.txtCode.Font = new Font("Segoe UI", 10F);

            var lblCourse = new Label() { Text = "Course:", Location = new Point(650, 170), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.cboCourse.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboCourse.Location = new Point(650, 195);
            this.cboCourse.Size = new Size(250, 23);
            this.cboCourse.Font = new Font("Segoe UI", 10F);

            var lblCredits = new Label() { Text = "Credits:", Location = new Point(650, 230), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.nudCredits.Location = new Point(650, 255);
            this.nudCredits.Size = new Size(100, 23);
            this.nudCredits.Minimum = 1;
            this.nudCredits.Maximum = 10;
            this.nudCredits.Value = 3;
            this.nudCredits.Font = new Font("Segoe UI", 10F);

            var lblDescription = new Label() { Text = "Description:", Location = new Point(650, 290), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtDescription.Location = new Point(650, 315);
            this.txtDescription.Size = new Size(250, 60);
            this.txtDescription.Multiline = true;
            this.txtDescription.Font = new Font("Segoe UI", 10F);

            // Buttons
            this.btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnAdd.ForeColor = Color.White;
            this.btnAdd.Location = new Point(650, 400);
            this.btnAdd.Size = new Size(80, 35);
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);

            this.btnUpdate.BackColor = Color.FromArgb(0, 123, 255);
            this.btnUpdate.Enabled = false;
            this.btnUpdate.FlatStyle = FlatStyle.Flat;
            this.btnUpdate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnUpdate.ForeColor = Color.White;
            this.btnUpdate.Location = new Point(740, 400);
            this.btnUpdate.Size = new Size(80, 35);
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new EventHandler(this.btnUpdate_Click);

            this.btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            this.btnDelete.Enabled = false;
            this.btnDelete.FlatStyle = FlatStyle.Flat;
            this.btnDelete.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.Location = new Point(830, 400);
            this.btnDelete.Size = new Size(80, 35);
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            this.btnClear.BackColor = Color.FromArgb(108, 117, 125);
            this.btnClear.FlatStyle = FlatStyle.Flat;
            this.btnClear.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnClear.ForeColor = Color.White;
            this.btnClear.Location = new Point(650, 450);
            this.btnClear.Size = new Size(80, 35);
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // Add controls to form
            this.Controls.Add(this.dgvSubjects);
            this.Controls.Add(lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(lblCode);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(lblCourse);
            this.Controls.Add(this.cboCourse);
            this.Controls.Add(lblCredits);
            this.Controls.Add(this.nudCredits);
            this.Controls.Add(lblDescription);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnClear);

            ((System.ComponentModel.ISupportInitialize)(this.dgvSubjects)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCredits)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}