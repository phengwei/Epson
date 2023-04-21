using Epson.Core.Domain.AuditTrail;
using Epson.Core.Domain.Category;
using Epson.Core.Domain.Email;
using Epson.Core.Domain.Product;
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

        public DbSet<AuditTrail> AuditTrails { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<EmailAccount> EmailAccounts { get; set; }
        public DbSet<EmailQueue> EmailQueues{ get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"server=127.0.0.1;port=3306;database=epsonlocaldb;user=root;password=Abcde123.;");
        }

    }
}
