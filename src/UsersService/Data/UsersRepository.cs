using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace UsersService.Data;

public class UsersRepository(UsersDbContext context) : IUsersRepository
{
    private readonly UsersDbContext _context = context;

    public async Task<User> CreateUser(User user)
    {
        var result = await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<bool> DeleteUser(int userId)
    {
        await _context.Users.Where(e => e.Id == userId).ExecuteDeleteAsync();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<User>> GetAllUsers()
    {
        var userEntities = await _context.Users.AsNoTracking().ToListAsync();

        return userEntities.ToList();
    }

    public async Task<User> GetUserById(int userId)
    {
        var userEntity = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
        return (userEntity == null) ? null : userEntity;
    }

    public async Task<User> GetUserByEmail(string email)
    {
        var userEntity = await _context.Users.Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
        return (userEntity == null) ? null : userEntity;
    }

    public async Task<int> UpdateUser(User user)
    {
        await _context.Users.Where(e => e.Id == user.Id)
            .ExecuteUpdateAsync(x => x
            .SetProperty(p => p.FirstName, p => user.FirstName)
            .SetProperty(p => p.LastName, p => user.LastName)
            .SetProperty(p => p.MiddleName, p => user.MiddleName)
            .SetProperty(p => p.Email, p => user.Email.ToLower())
            .SetProperty(p => p.PasswordSalt, p => user.PasswordSalt)
            .SetProperty(p => p.PasswordHash, p => user.PasswordHash)
            );

        await _context.SaveChangesAsync();

        return user.Id;
    }

    public async Task<int> UpdateUserPartial(User user)
    {
        var userEntity = await _context.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync();

        if(userEntity == null) 
            return 0;

        if (user.FirstName != null)
            userEntity.FirstName = user.FirstName;

        if (user.LastName != null)
            userEntity.LastName = user.LastName;

        if (user.MiddleName != null)
            userEntity.MiddleName = user.MiddleName;

        if (user.Email != null)
            userEntity.Email = user.Email;

        _context.Users.Update(userEntity);
        await _context.SaveChangesAsync();
        return user.Id;
    }

    public async Task<bool> ResetDb()
    {
        await _context.Database.EnsureDeletedAsync();
        var result = await _context.Database.EnsureCreatedAsync();
        return result;
    }
}

