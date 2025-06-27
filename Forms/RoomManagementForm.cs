using System.Data.SQLite;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Forms
{
    public partial class RoomManagementForm : BaseForm
    {
        private readonly DatabaseContext _context;
        private DataGridView dgvRooms;
        private TextBox txtName;
        private TextBox txtCode;
        private ComboBox cboType;
        private NumericUpDown nudCapacity;
        private TextBox txtLocation;
        private TextBox txtEquipment;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private Room? _selectedRoom;

        public RoomManagementForm(User currentUser) : base(currentUser)
        {
            _context = new DatabaseContext();
            InitializeComponent();
            LoadRoomTypes();
            LoadRooms();
        }

        private void LoadRoomTypes()
        {
            var roomTypes = Enum.GetValues(typeof(RoomType))
                .Cast<RoomType>()
                .Select(rt => new { Value = (int)rt, Name = rt.ToString() })
                .ToList();

            cboType.DataSource = roomTypes;
            cboType.DisplayMember = "Name";
            cboType.ValueMember = "Value";
            cboType.SelectedIndex = -1;
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

                dgvRooms.DataSource = rooms;
                
                // Hide unnecessary columns
                if (dgvRooms.Columns["Id"] != null)
                    dgvRooms.Columns["Id"].Visible = false;
                if (dgvRooms.Columns["IsActive"] != null)
                    dgvRooms.Columns["IsActive"].Visible = false;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load rooms: {ex.Message}");
            }
        }

        private void dgvRooms_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRooms.CurrentRow?.DataBoundItem is Room room)
            {
                _selectedRoom = room;
                txtName.Text = room.Name;
                txtCode.Text = room.Code;
                cboType.SelectedValue = (int)room.Type;
                nudCapacity.Value = room.Capacity;
                txtLocation.Text = room.Location;
                txtEquipment.Text = room.Equipment;
                
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

                var query = @"INSERT INTO Rooms (Name, Code, Type, Capacity, Location, Equipment) 
                             VALUES (@name, @code, @type, @capacity, @location, @equipment)";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@name", txtName.Text.Trim());
                command.Parameters.AddWithValue("@code", txtCode.Text.Trim());
                command.Parameters.AddWithValue("@type", (int)cboType.SelectedValue);
                command.Parameters.AddWithValue("@capacity", (int)nudCapacity.Value);
                command.Parameters.AddWithValue("@location", txtLocation.Text.Trim());
                command.Parameters.AddWithValue("@equipment", txtEquipment.Text.Trim());

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    ShowSuccess("Room added successfully!");
                    ClearForm();
                    LoadRooms();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to add room: {ex.Message}");
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedRoom == null || !ValidateInput()) return;

                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"UPDATE Rooms 
                             SET Name = @name, Code = @code, Type = @type, Capacity = @capacity, 
                                 Location = @location, Equipment = @equipment 
                             WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@name", txtName.Text.Trim());
                command.Parameters.AddWithValue("@code", txtCode.Text.Trim());
                command.Parameters.AddWithValue("@type", (int)cboType.SelectedValue);
                command.Parameters.AddWithValue("@capacity", (int)nudCapacity.Value);
                command.Parameters.AddWithValue("@location", txtLocation.Text.Trim());
                command.Parameters.AddWithValue("@equipment", txtEquipment.Text.Trim());
                command.Parameters.AddWithValue("@id", _selectedRoom.Id);

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    ShowSuccess("Room updated successfully!");
                    ClearForm();
                    LoadRooms();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update room: {ex.Message}");
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedRoom == null) return;

                if (ConfirmAction($"Are you sure you want to delete the room '{_selectedRoom.Name}'?"))
                {
                    using var connection = _context.GetConnection();
                    await connection.OpenAsync();

                    var query = "UPDATE Rooms SET IsActive = 0 WHERE Id = @id";

                    using var command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@id", _selectedRoom.Id);

                    var result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        ShowSuccess("Room deleted successfully!");
                        ClearForm();
                        LoadRooms();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to delete room: {ex.Message}");
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
            cboType.SelectedIndex = -1;
            nudCapacity.Value = 20;
            txtLocation.Clear();
            txtEquipment.Clear();
            _selectedRoom = null;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            dgvRooms.ClearSelection();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowWarning("Please enter room name.");
                txtName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                ShowWarning("Please enter room code.");
                txtCode.Focus();
                return false;
            }

            if (cboType.SelectedValue == null)
            {
                ShowWarning("Please select room type.");
                cboType.Focus();
                return false;
            }

            if (nudCapacity.Value <= 0)
            {
                ShowWarning("Please enter valid capacity.");
                nudCapacity.Focus();
                return false;
            }

            return true;
        }

        private void InitializeComponent()
        {
            this.dgvRooms = new DataGridView();
            this.txtName = new TextBox();
            this.txtCode = new TextBox();
            this.cboType = new ComboBox();
            this.nudCapacity = new NumericUpDown();
            this.txtLocation = new TextBox();
            this.txtEquipment = new TextBox();
            this.btnAdd = new Button();
            this.btnUpdate = new Button();
            this.btnDelete = new Button();
            this.btnClear = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRooms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCapacity)).BeginInit();
            this.SuspendLayout();

            // 
            // RoomManagementForm
            // 
            this.ClientSize = new Size(950, 600);
            this.Text = "Room Management";

            // 
            // dgvRooms
            // 
            this.dgvRooms.AllowUserToAddRows = false;
            this.dgvRooms.AllowUserToDeleteRows = false;
            this.dgvRooms.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvRooms.BackgroundColor = Color.White;
            this.dgvRooms.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRooms.Location = new Point(20, 20);
            this.dgvRooms.MultiSelect = false;
            this.dgvRooms.ReadOnly = true;
            this.dgvRooms.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvRooms.Size = new Size(600, 400);
            this.dgvRooms.TabIndex = 0;
            this.dgvRooms.SelectionChanged += new EventHandler(this.dgvRooms_SelectionChanged);

            // Form controls
            var lblName = new Label() { Text = "Name:", Location = new Point(650, 50), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtName.Location = new Point(650, 75);
            this.txtName.Size = new Size(250, 23);
            this.txtName.Font = new Font("Segoe UI", 10F);

            var lblCode = new Label() { Text = "Code:", Location = new Point(650, 110), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtCode.Location = new Point(650, 135);
            this.txtCode.Size = new Size(250, 23);
            this.txtCode.Font = new Font("Segoe UI", 10F);

            var lblType = new Label() { Text = "Type:", Location = new Point(650, 170), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.cboType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboType.Location = new Point(650, 195);
            this.cboType.Size = new Size(250, 23);
            this.cboType.Font = new Font("Segoe UI", 10F);

            var lblCapacity = new Label() { Text = "Capacity:", Location = new Point(650, 230), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.nudCapacity.Location = new Point(650, 255);
            this.nudCapacity.Size = new Size(100, 23);
            this.nudCapacity.Minimum = 1;
            this.nudCapacity.Maximum = 1000;
            this.nudCapacity.Value = 20;
            this.nudCapacity.Font = new Font("Segoe UI", 10F);

            var lblLocation = new Label() { Text = "Location:", Location = new Point(650, 290), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtLocation.Location = new Point(650, 315);
            this.txtLocation.Size = new Size(250, 23);
            this.txtLocation.Font = new Font("Segoe UI", 10F);

            var lblEquipment = new Label() { Text = "Equipment:", Location = new Point(650, 350), Size = new Size(80, 23), Font = new Font("Segoe UI", 10F) };
            this.txtEquipment.Location = new Point(650, 375);
            this.txtEquipment.Size = new Size(250, 60);
            this.txtEquipment.Multiline = true;
            this.txtEquipment.Font = new Font("Segoe UI", 10F);

            // Buttons
            this.btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnAdd.ForeColor = Color.White;
            this.btnAdd.Location = new Point(650, 460);
            this.btnAdd.Size = new Size(80, 35);
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);

            this.btnUpdate.BackColor = Color.FromArgb(0, 123, 255);
            this.btnUpdate.Enabled = false;
            this.btnUpdate.FlatStyle = FlatStyle.Flat;
            this.btnUpdate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnUpdate.ForeColor = Color.White;
            this.btnUpdate.Location = new Point(740, 460);
            this.btnUpdate.Size = new Size(80, 35);
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new EventHandler(this.btnUpdate_Click);

            this.btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            this.btnDelete.Enabled = false;
            this.btnDelete.FlatStyle = FlatStyle.Flat;
            this.btnDelete.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.Location = new Point(830, 460);
            this.btnDelete.Size = new Size(80, 35);
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            this.btnClear.BackColor = Color.FromArgb(108, 117, 125);
            this.btnClear.FlatStyle = FlatStyle.Flat;
            this.btnClear.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnClear.ForeColor = Color.White;
            this.btnClear.Location = new Point(650, 510);
            this.btnClear.Size = new Size(80, 35);
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // Add controls to form
            this.Controls.Add(this.dgvRooms);
            this.Controls.Add(lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(lblCode);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(lblType);
            this.Controls.Add(this.cboType);
            this.Controls.Add(lblCapacity);
            this.Controls.Add(this.nudCapacity);
            this.Controls.Add(lblLocation);
            this.Controls.Add(this.txtLocation);
            this.Controls.Add(lblEquipment);
            this.Controls.Add(this.txtEquipment);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnClear);

            ((System.ComponentModel.ISupportInitialize)(this.dgvRooms)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCapacity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}