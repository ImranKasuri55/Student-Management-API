using Student_Management_API.Models;

namespace Student_Management_API.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
