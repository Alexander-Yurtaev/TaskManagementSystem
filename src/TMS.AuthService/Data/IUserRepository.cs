using TMS.AuthService.Entities;

namespace TMS.AuthService.Data;

/// <summary>
/// 
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<UserEntity?> GetByIdAsync(int id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    Task<UserEntity?> GetByUsernameAsync(string userName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    Task<bool> UserExistsByNameAsync(string userName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> UserExistsByIdAsync(int id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task AddUserAsync(UserEntity user);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<UserEntity>> GetUsersAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task DeleteUserAsync(int id);
}