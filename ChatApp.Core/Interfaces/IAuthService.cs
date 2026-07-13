using System.Threading.Tasks;
using ChatApp.Core.Entities;

namespace ChatApp.Core.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(User user);
        string GenerateOtp();
    }
}
