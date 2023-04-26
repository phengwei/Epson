using Epson.Core.Domain.Users;
using Epson.Model.Users;
using System.Threading.Tasks;

namespace Epson.Models.Users
{
    public interface IJwtService
    {
        Task<string> GenerateToken(ApplicationUser user);
    }
}