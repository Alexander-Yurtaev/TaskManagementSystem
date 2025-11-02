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
        var user = await _db.Users.FindAsync(id);
        return user;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task<UserEntity?> GetByUsernameAsync(string userName)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower());
        return user;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task<bool> UserExistsAsync(string userName)
    {
        ArgumentException.ThrowIfNullOrEmpty(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        return await _db.Users.AnyAsync(u => u.UserName.ToLower() == userName.ToLower());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    public async Task AddUserAsync(UserEntity user)
    {
        ArgumentNullException.ThrowIfNull(user);

        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<UserEntity>> GetUsersAsync()
    {
        var users = await _db.Users.ToArrayAsync();
        return users;
    }
}
