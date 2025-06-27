using System.Data.SQLite;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Interfaces;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Repositories
{
    public class SubjectRepository : IRepository<Subject>
    {
        private readonly DatabaseContext _context;

        public SubjectRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<Subject>> GetAllAsync()
        {
            var subjects = new List<Subject>();

            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"SELECT s.*, c.Name as CourseName 
                             FROM Subjects s 
                             INNER JOIN Courses c ON s.CourseId = c.Id 
                             WHERE s.IsActive = 1 
                             ORDER BY c.Name, s.Name";

                using var command = new SQLiteCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    subjects.Add(new Subject
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Code = reader.GetString("Code"),
                        CourseId = reader.GetInt32("CourseId"),
                        Credits = reader.GetInt32("Credits"),
                        Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedDate = reader.GetDateTime("CreatedDate"),
                        CourseName = reader.GetString("CourseName")
                    });
                }

                return subjects;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get subjects: {ex.Message}", ex);
            }
        }

        public async Task<Subject?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"SELECT s.*, c.Name as CourseName 
                             FROM Subjects s 
                             INNER JOIN Courses c ON s.CourseId = c.Id 
                             WHERE s.Id = @id AND s.IsActive = 1";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Subject
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Code = reader.GetString("Code"),
                        CourseId = reader.GetInt32("CourseId"),
                        Credits = reader.GetInt32("Credits"),
                        Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedDate = reader.GetDateTime("CreatedDate"),
                        CourseName = reader.GetString("CourseName")
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get subject: {ex.Message}", ex);
            }
        }

        public async Task<List<Subject>> GetByCourseIdAsync(int courseId)
        {
            var subjects = new List<Subject>();

            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"SELECT s.*, c.Name as CourseName 
                             FROM Subjects s 
                             INNER JOIN Courses c ON s.CourseId = c.Id 
                             WHERE s.CourseId = @courseId AND s.IsActive = 1 
                             ORDER BY s.Name";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@courseId", courseId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    subjects.Add(new Subject
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Code = reader.GetString("Code"),
                        CourseId = reader.GetInt32("CourseId"),
                        Credits = reader.GetInt32("Credits"),
                        Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedDate = reader.GetDateTime("CreatedDate"),
                        CourseName = reader.GetString("CourseName")
                    });
                }

                return subjects;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get subjects by course: {ex.Message}", ex);
            }
        }

        public async Task<int> AddAsync(Subject entity)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"INSERT INTO Subjects (Name, Code, CourseId, Credits, Description) 
                             VALUES (@name, @code, @courseId, @credits, @description);
                             SELECT last_insert_rowid();";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@name", entity.Name);
                command.Parameters.AddWithValue("@code", entity.Code);
                command.Parameters.AddWithValue("@courseId", entity.CourseId);
                command.Parameters.AddWithValue("@credits", entity.Credits);
                command.Parameters.AddWithValue("@description", entity.Description);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add subject: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateAsync(Subject entity)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"UPDATE Subjects 
                             SET Name = @name, Code = @code, CourseId = @courseId, Credits = @credits, Description = @description 
                             WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@name", entity.Name);
                command.Parameters.AddWithValue("@code", entity.Code);
                command.Parameters.AddWithValue("@courseId", entity.CourseId);
                command.Parameters.AddWithValue("@credits", entity.Credits);
                command.Parameters.AddWithValue("@description", entity.Description);
                command.Parameters.AddWithValue("@id", entity.Id);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update subject: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = "UPDATE Subjects SET IsActive = 0 WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete subject: {ex.Message}", ex);
            }
        }
    }
}