using LoginApi.Models;

namespace LoginApi.Services
{
    public interface ILoginService
    {
        Task<UserLogin?> GetLoginByIdAsync(int id);
        Task<IEnumerable<UserLogin>> GetAllLoginsAsync();
        Task<UserLogin> CreateLoginAsync(UserLogin login);
        Task<UserLogin?> UpdateLoginAsync(int id, UserLogin login);
        Task<bool> DeleteLoginAsync(int id);
    }
}
