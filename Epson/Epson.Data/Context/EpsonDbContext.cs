using Epson.Core.Domain.AuditTrail;
using Epson.Core.Domain.Category;
using Epson.Core.Domain.Email;
using Epson.Core.Domain.Products;
using Epson.Core.Domain.Request;
using Epson.Core.Domain.User;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<User> User { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<UserRoles> UserRole { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"server=127.0.0.1;port=3306;database=epsonlocaldb;user=root;password=Abcde123.;");
        }

    }
}
