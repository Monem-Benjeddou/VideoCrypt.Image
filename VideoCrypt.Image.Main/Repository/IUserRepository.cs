using Microsoft.AspNetCore.Identity;

namespace VideoCrypt.Image.Main.Repository;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

public interface IUserRepository
{
    Task<string> AuthenticateAsync(string email, string password);
    Task<string> RefreshTokenAsync(string refreshToken);
}
