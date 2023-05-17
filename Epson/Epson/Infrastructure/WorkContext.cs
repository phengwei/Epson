using Epson.Core.Domain.Users;
using Epson.Model.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Epson.Infrastructure
{
    public class WorkContext : IWorkContext
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private Customer _cachedCustomer;
        public WorkContext(UserManager<ApplicationUser> userManager, 
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        private Customer GetAuthenticatedUser()
        {
            var authenticatedUser = new Customer();

            var loggedInUser = _httpContextAccessor.HttpContext.User;
            authenticatedUser.Id = loggedInUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            authenticatedUser.Name = loggedInUser.Identity.Name;

            authenticatedUser.Roles = loggedInUser.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return authenticatedUser;
        }

        public virtual Customer CurrentUser
        {
            get
            {
                if (_cachedCustomer != null)
                    return _cachedCustomer;

                Customer customer = null;

                if (customer == null)
                {
                    _cachedCustomer = GetAuthenticatedUser();
                }

                return _cachedCustomer;
            }
            set
            {
                _cachedCustomer = value;
            }
        }
    }
}
