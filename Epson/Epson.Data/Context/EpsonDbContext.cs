using Epson.Core.Domain.AuditTrail;
using Epson.Core.Domain.Category;
using Epson.Core.Domain.Email;
using Epson.Core.Domain.Products;
using Epson.Core.Domain.Request;
using Epson.Core.Domain.Users;
using LinqToDB.DataProvider.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Epson.Data.Context
{
    public class EpsonDbContext : DbContext
    {
        public EpsonDbContext(DbContextOptions<EpsonDbContext> options) : base(options)
        {
        }

        public DbSet<AuditTrail> Audit_Trail { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<EmailAccount> Email_Account { get; set; }
        public DbSet<EmailQueue> Email_Queue { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Request> Request { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>(entity => { entity.ToTable("AspNetUsers"); });
            builder.Entity<Role>(entity => { entity.ToTable("AspNetRoles"); });
            builder.Entity<IdentityUserRole<string>>(entity => { 
                entity.ToTable("AspNetUserRoles"); 
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            });
            builder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("AspNetUserClaims"); });
            builder.Entity<IdentityUserLogin<string>>(entity => { 
                entity.ToTable("AspNetUserLogins");
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
            });
            builder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("AspNetRoleClaims"); });
            builder.Entity<IdentityUserToken<string>>(entity => { 
                entity.ToTable("AspNetUserTokens"); 
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("No Connection String", new MySqlServerVersion(new Version(8, 0, 22)));
            }
        }
    }
}
