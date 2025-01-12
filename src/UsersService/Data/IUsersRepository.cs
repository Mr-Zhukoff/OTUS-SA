using CoreLogic.Models;

namespace UsersService.Data;

public interface IUsersRepository
{
    Task<User> CreateUser(User user);
    Task<bool> DeleteUser(int userId);
    Task<User> GetUserById(int userId);
    Task<User> GetUserByEmail(string email);
    Task<List<User>> GetAllUsers();
    Task<int> UpdateUser(User user);
    Task<int> UpdateUserPartial(User user);
    Task<bool> ResetDb();
    string GetConnectionInfo();
}