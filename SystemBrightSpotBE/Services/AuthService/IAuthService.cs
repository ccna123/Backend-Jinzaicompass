using SystemBrightSpotBE.Models;

namespace SystemBrightSpotBE.Services.AuthService
{
    public interface IAuthService
    {
        bool Login(string email, string password);
        string GetToken(User account);
        long GetAccountId(string type);
        string GetAccountFullName();
        Task<bool> ResetPassword(long user_id);
        Task<bool> ChangePassword(long user_id, string new_password);
        string GenerateSecurePassword();
    }
}
