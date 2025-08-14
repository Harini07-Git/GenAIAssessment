using LoginApi.Models;

namespace LoginApi.Services
{
    public class LoginService : ILoginService
    {
        private readonly List<UserLogin> _logins;

        public LoginService()
        {
            _logins = new List<UserLogin>();
        }

        public async Task<UserLogin?> GetLoginByIdAsync(int id)
        {
            return await Task.FromResult(_logins.FirstOrDefault(x => x.Id == id));
        }

        public async Task<IEnumerable<UserLogin>> GetAllLoginsAsync()
        {
            return await Task.FromResult(_logins);
        }

        public async Task<UserLogin> CreateLoginAsync(UserLogin login)
        {
            login.Id = _logins.Count > 0 ? _logins.Max(x => x.Id) + 1 : 1;
            login.CreatedAt = DateTime.UtcNow;
            _logins.Add(login);
            return await Task.FromResult(login);
        }

        public async Task<UserLogin?> UpdateLoginAsync(int id, UserLogin login)
        {
            var existingLogin = _logins.FirstOrDefault(x => x.Id == id);
            if (existingLogin == null)
                return null;

            existingLogin.Username = login.Username;
            existingLogin.Email = login.Email;
            existingLogin.Password = login.Password;
            existingLogin.LastLoginAt = DateTime.UtcNow;

            return await Task.FromResult(existingLogin);
        }

        public async Task<bool> DeleteLoginAsync(int id)
        {
            var login = _logins.FirstOrDefault(x => x.Id == id);
            if (login == null)
                return await Task.FromResult(false);

            _logins.Remove(login);
            return await Task.FromResult(true);
        }
    }
}
