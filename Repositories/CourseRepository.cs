using System.Data.SQLite;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Interfaces;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Repositories
{
    public class CourseRepository : IRepository<Course>
    {
        private readonly DatabaseContext _context;

        public CourseRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAllAsync()
        {
            var courses = new List<Course>();

            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT * FROM Courses WHERE IsActive = 1 ORDER BY Name";

                using var command = new SQLiteCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    courses.Add(new Course
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Code = reader.GetString("Code"),
                        Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                        Duration = reader.GetInt32("Duration"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedDate = reader.GetDateTime("CreatedDate")
                    });
                }

                return courses;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get courses: {ex.Message}", ex);
            }
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT * FROM Courses WHERE Id = @id AND IsActive = 1";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Course
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Code = reader.GetString("Code"),
                        Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                        Duration = reader.GetInt32("Duration"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedDate = reader.GetDateTime("CreatedDate")
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get course: {ex.Message}", ex);
            }
        }

        public async Task<int> AddAsync(Course entity)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"INSERT INTO Courses (Name, Code, Description, Duration) 
                             VALUES (@name, @code, @description, @duration);
                             SELECT last_insert_rowid();";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@name", entity.Name);
                command.Parameters.AddWithValue("@code", entity.Code);
                command.Parameters.AddWithValue("@description", entity.Description);
                command.Parameters.AddWithValue("@duration", entity.Duration);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add course: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateAsync(Course entity)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"UPDATE Courses 
                             SET Name = @name, Code = @code, Description = @description, Duration = @duration 
                             WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@name", entity.Name);
                command.Parameters.AddWithValue("@code", entity.Code);
                command.Parameters.AddWithValue("@description", entity.Description);
                command.Parameters.AddWithValue("@duration", entity.Duration);
                command.Parameters.AddWithValue("@id", entity.Id);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update course: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = "UPDATE Courses SET IsActive = 0 WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete course: {ex.Message}", ex);
            }
        }
    }
}