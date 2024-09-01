using Epson.Core.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Epson.Extensions
{
    public class UserManagerExtension : UserManager<ApplicationUser>
    {
        public UserManagerExtension(
            IUserStore<ApplicationUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<ApplicationUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override async Task<ApplicationUser> FindByNameAsync(string userName)
        {
            var user = await base.FindByNameAsync(userName);
            if (user != null && !user.IsActive)
            {
                return null;
            }
            return user;
        }

        public override async Task<ApplicationUser> FindByIdAsync(string userId)
        {
            var user = await base.FindByIdAsync(userId);
            if (user != null && !user.IsActive)
            {
                return null;
            }
            return user;
        }

        public override async Task<ApplicationUser> FindByEmailAsync(string email)
        {
            var user = await base.FindByEmailAsync(email);
            if (user != null && !user.IsActive)
            {
                return null;
            }
            return user;
        }

        public override async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            if (user != null && !user.IsActive)
            {
                return false;
            }
            return await base.CheckPasswordAsync(user, password);
        }

        public override async Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            if (!user.IsActive)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Cannot create a user that is not active." });
            }
            return await base.CreateAsync(user);
        }

        public override async Task<IdentityResult> AddPasswordAsync(ApplicationUser user, string password)
        {
            if (user != null && !user.IsActive)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Cannot add a password to a user that is not active." });
            }
            return await base.AddPasswordAsync(user, password);
        }

        public override async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            if (user != null && !user.IsActive)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Cannot change the password of a user that is not active." });
            }
            return await base.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public override async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
        {
            if (user != null && !user.IsActive)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Cannot reset the password of a user that is not active." });
            }
            return await base.ResetPasswordAsync(user, token, newPassword);
        }
        public async Task<ApplicationUser> FindByIdIncludingInactiveAsync(string userId)
        {
            var user = await base.FindByIdAsync(userId);
            return user;  
        }
    }
}
