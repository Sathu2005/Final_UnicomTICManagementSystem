using System.Data.SQLite;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Data
{
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext()
        {
            _connectionString = "Data Source=unicomtic.db;Version=3;";
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                await CreateTablesAsync(connection);
                await SeedDataAsync(connection);
            }
            catch (Exception ex)
            {
                throw new Exception($"Database initialization failed: {ex.Message}", ex);
            }
        }

        private async Task CreateTablesAsync(SQLiteConnection connection)
        {
            var commands = new[]
            {
                @"CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE NOT NULL,
                    Password TEXT NOT NULL,
                    FullName TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Role INTEGER NOT NULL,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    IsActive BOOLEAN DEFAULT 1
                )",

                @"CREATE TABLE IF NOT EXISTS Courses (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Code TEXT UNIQUE NOT NULL,
                    Description TEXT,
                    Duration INTEGER NOT NULL,
                    IsActive BOOLEAN DEFAULT 1,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
                )",

                @"CREATE TABLE IF NOT EXISTS Subjects (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Code TEXT UNIQUE NOT NULL,
                    CourseId INTEGER NOT NULL,
                    Credits INTEGER NOT NULL,
                    Description TEXT,
                    IsActive BOOLEAN DEFAULT 1,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (CourseId) REFERENCES Courses(Id)
                )",

                @"CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentNumber TEXT UNIQUE NOT NULL,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Phone TEXT,
                    DateOfBirth DATE NOT NULL,
                    CourseId INTEGER NOT NULL,
                    EnrollmentDate DATE DEFAULT CURRENT_DATE,
                    IsActive BOOLEAN DEFAULT 1,
                    FOREIGN KEY (CourseId) REFERENCES Courses(Id)
                )",

                @"CREATE TABLE IF NOT EXISTS Rooms (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Code TEXT UNIQUE NOT NULL,
                    Type INTEGER NOT NULL,
                    Capacity INTEGER NOT NULL,
                    Location TEXT,
                    Equipment TEXT,
                    IsActive BOOLEAN DEFAULT 1
                )",

                @"CREATE TABLE IF NOT EXISTS Exams (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    SubjectId INTEGER NOT NULL,
                    ExamDate DATE NOT NULL,
                    StartTime TIME NOT NULL,
                    EndTime TIME NOT NULL,
                    RoomId INTEGER NOT NULL,
                    MaxMarks INTEGER NOT NULL,
                    Description TEXT,
                    IsActive BOOLEAN DEFAULT 1,
                    FOREIGN KEY (SubjectId) REFERENCES Subjects(Id),
                    FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
                )",

                @"CREATE TABLE IF NOT EXISTS Marks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL,
                    ExamId INTEGER NOT NULL,
                    MarksObtained DECIMAL(5,2) NOT NULL,
                    Grade TEXT,
                    Remarks TEXT,
                    RecordedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    RecordedBy INTEGER NOT NULL,
                    FOREIGN KEY (StudentId) REFERENCES Students(Id),
                    FOREIGN KEY (ExamId) REFERENCES Exams(Id),
                    FOREIGN KEY (RecordedBy) REFERENCES Users(Id)
                )",

                @"CREATE TABLE IF NOT EXISTS Timetables (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SubjectId INTEGER NOT NULL,
                    RoomId INTEGER NOT NULL,
                    DayOfWeek INTEGER NOT NULL,
                    StartTime TIME NOT NULL,
                    EndTime TIME NOT NULL,
                    LecturerId INTEGER NOT NULL,
                    IsActive BOOLEAN DEFAULT 1,
                    FOREIGN KEY (SubjectId) REFERENCES Subjects(Id),
                    FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
                    FOREIGN KEY (LecturerId) REFERENCES Users(Id)
                )"
            };

            foreach (var commandText in commands)
            {
                using var command = new SQLiteCommand(commandText, connection);
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task SeedDataAsync(SQLiteConnection connection)
        {
            // Check if data already exists
            using var checkCommand = new SQLiteCommand("SELECT COUNT(*) FROM Users", connection);
            var userCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (userCount > 0) return; // Data already seeded

            var seedCommands = new[]
            {
                // Seed Users
                "INSERT INTO Users (Username, Password, FullName, Email, Role) VALUES ('admin', 'admin123', 'System Administrator', 'admin@unicomtic.edu', 1)",
                "INSERT INTO Users (Username, Password, FullName, Email, Role) VALUES ('lecturer1', 'lect123', 'Dr. John Smith', 'john.smith@unicomtic.edu', 2)",
                "INSERT INTO Users (Username, Password, FullName, Email, Role) VALUES ('staff1', 'staff123', 'Mary Johnson', 'mary.johnson@unicomtic.edu', 3)",
                "INSERT INTO Users (Username, Password, FullName, Email, Role) VALUES ('student1', 'stud123', 'Alice Brown', 'alice.brown@student.unicomtic.edu', 4)",

                // Seed Courses
                "INSERT INTO Courses (Name, Code, Description, Duration) VALUES ('Computer Science', 'CS', 'Bachelor of Computer Science', 36)",
                "INSERT INTO Courses (Name, Code, Description, Duration) VALUES ('Information Technology', 'IT', 'Bachelor of Information Technology', 36)",
                "INSERT INTO Courses (Name, Code, Description, Duration) VALUES ('Software Engineering', 'SE', 'Bachelor of Software Engineering', 48)",

                // Seed Rooms
                "INSERT INTO Rooms (Name, Code, Type, Capacity, Location, Equipment) VALUES ('Main Lecture Hall', 'LH001', 1, 100, 'Ground Floor', 'Projector, Sound System')",
                "INSERT INTO Rooms (Name, Code, Type, Capacity, Location, Equipment) VALUES ('Computer Lab 1', 'CL001', 3, 30, 'First Floor', '30 Computers, Projector')",
                "INSERT INTO Rooms (Name, Code, Type, Capacity, Location, Equipment) VALUES ('Physics Lab', 'PL001', 2, 25, 'Second Floor', 'Lab Equipment, Safety Gear')",

                // Seed Subjects
                "INSERT INTO Subjects (Name, Code, CourseId, Credits, Description) VALUES ('Programming Fundamentals', 'CS101', 1, 3, 'Introduction to Programming')",
                "INSERT INTO Subjects (Name, Code, CourseId, Credits, Description) VALUES ('Database Systems', 'CS201', 1, 4, 'Database Design and Management')",
                "INSERT INTO Subjects (Name, Code, CourseId, Credits, Description) VALUES ('Web Development', 'IT101', 2, 3, 'HTML, CSS, JavaScript')",

                // Seed Students
                "INSERT INTO Students (StudentNumber, FirstName, LastName, Email, Phone, DateOfBirth, CourseId) VALUES ('2024001', 'Alice', 'Brown', 'alice.brown@student.unicomtic.edu', '123-456-7890', '2000-05-15', 1)",
                "INSERT INTO Students (StudentNumber, FirstName, LastName, Email, Phone, DateOfBirth, CourseId) VALUES ('2024002', 'Bob', 'Wilson', 'bob.wilson@student.unicomtic.edu', '123-456-7891', '1999-08-22', 2)",
                "INSERT INTO Students (StudentNumber, FirstName, LastName, Email, Phone, DateOfBirth, CourseId) VALUES ('2024003', 'Carol', 'Davis', 'carol.davis@student.unicomtic.edu', '123-456-7892', '2001-03-10', 1)"
            };

            foreach (var commandText in seedCommands)
            {
                using var command = new SQLiteCommand(commandText, connection);
                await command.ExecuteNonQueryAsync();
            }
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }
    }
}