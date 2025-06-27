using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Interfaces
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<List<User>> GetUsersByRoleAsync(UserRole role);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ChangePasswordAsync(int userId, string newPassword);
    }
}