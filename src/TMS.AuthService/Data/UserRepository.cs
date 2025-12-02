using Microsoft.EntityFrameworkCore;
using TMS.AuthService.Entities;

namespace TMS.AuthService.Data;

/// <summary>
///
/// </summary>
/// <param name="db"></param>
public class UserRepository(AuthDataContext db) : IUserRepository
{
    private readonly AuthDataContext _db = db;

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<UserEntity?> GetByIdAsync(int id)
    {
        var user = await _db.Users.Where(u => u.IsDeleted == false).FindAsync(id);
        return user;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task<UserEntity?> GetByUsernameAsync(string userName)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower() && u.IsDeleted == false);
        return user;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task<bool> UserExistsByNameAsync(string userName)
    {
        ArgumentException.ThrowIfNullOrEmpty(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        return await _db.Users
            .AnyAsync(u => u.UserName.ToLower() == userName.ToLower() && u.IsDeleted == false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> UserExistsByIdAsync(int id)
    {
        return await _db.Users.AnyAsync(u => u.Id == id && u.IsDeleted == false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="user"></param>
    public async Task AddUserAsync(UserEntity user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<UserEntity>> GetUsersAsync()
    {
        var users = await _db.Users.Where(u => u.IsDeleted == false).ToArrayAsync();
        return users;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public async Task DeleteUserAsync(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted == false);
        if (user == null)
        {
            throw new NullReferenceException();
        }

        user.IsDeleted = true;
        await _db.SaveChangesAsync();
    }
}