using Epson.Core.Domain.Users;

namespace Epson.Infrastructure
{
    public class ApplicationUserComparer : IEqualityComparer<ApplicationUser>
    {
        public bool Equals(ApplicationUser x, ApplicationUser y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(ApplicationUser obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
