using System.Data.SQLite;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Interfaces;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly DatabaseContext _context;

        public UserService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"SELECT Id, Username, Password, FullName, Email, Role, CreatedDate, IsActive 
                             FROM Users 
                             WHERE Username = @username AND Password = @password AND IsActive = 1";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new User
                    {
                        Id = reader.GetInt32("Id"),
                        Username = reader.GetString("Username"),
                        Password = reader.GetString("Password"),
                        FullName = reader.GetString("FullName"),
                        Email = reader.GetString("Email"),
                        Role = (UserRole)reader.GetInt32("Role"),
                        CreatedDate = reader.GetDateTime("CreatedDate"),
                        IsActive = reader.GetBoolean("IsActive")
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Authentication failed: {ex.Message}", ex);
            }
        }

        public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
        {
            var users = new List<User>();

            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"SELECT Id, Username, FullName, Email, Role, CreatedDate, IsActive 
                             FROM Users 
                             WHERE Role = @role AND IsActive = 1";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@role", (int)role);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        Id = reader.GetInt32("Id"),
                        Username = reader.GetString("Username"),
                        FullName = reader.GetString("FullName"),
                        Email = reader.GetString("Email"),
                        Role = (UserRole)reader.GetInt32("Role"),
                        CreatedDate = reader.GetDateTime("CreatedDate"),
                        IsActive = reader.GetBoolean("IsActive")
                    });
                }

                return users;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get users by role: {ex.Message}", ex);
            }
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"INSERT INTO Users (Username, Password, FullName, Email, Role) 
                             VALUES (@username, @password, @fullName, @email, @role)";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@password", user.Password);
                command.Parameters.AddWithValue("@fullName", user.FullName);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@role", (int)user.Role);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create user: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = @"UPDATE Users 
                             SET Username = @username, FullName = @fullName, Email = @email, Role = @role 
                             WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@fullName", user.FullName);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@role", (int)user.Role);
                command.Parameters.AddWithValue("@id", user.Id);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update user: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = "UPDATE Users SET IsActive = 0 WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete user: {ex.Message}", ex);
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                var query = "UPDATE Users SET Password = @password WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@password", newPassword);
                command.Parameters.AddWithValue("@id", userId);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change password: {ex.Message}", ex);
            }
        }
    }
}