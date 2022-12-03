using FeatherAndBeads.API.Models;

namespace FeatherAndBeads.API.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
