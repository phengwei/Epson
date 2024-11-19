using Epson.Core.Domain.AuditTrail;
using Epson.Core.Domain.Categories;
using Epson.Core.Domain.Email;
using Epson.Core.Domain.Products;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.SLA;
using Epson.Core.Domain.Users;
using LinqToDB.DataProvider.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection.Emit;

namespace Epson.Data.Context
{
    public class EpsonDbContext : DbContext
    {
        public EpsonDbContext(DbContextOptions<EpsonDbContext> options) : base(options)
        {
        }

        public DbSet<AuditTrail> AuditTrail { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<EmailAccount> EmailAccount { get; set; }
        public DbSet<EmailQueue> EmailQueue { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Request> Request { get; set; }
        public DbSet<RequestProduct> RequestProduct { get; set; }
        public DbSet<ProjectInformation> ProjectInformation { get; set; }
        public DbSet<RequestSubmissionDetail> RequestSubmissionDetail { get; set; }
        public DbSet<ProjectInformationReason> ProjectInformationReason { get; set; }
        public DbSet<CompetitorInformation> CompetitorInformation { get; set; }
        public DbSet<SLAHoliday> SLAHoliday { get; set; }
        public DbSet<TeamHierarchy> TeamHierarchy { get; set; }
        public DbSet<SLAStaffLeave> SLAStaffLeave { get; set; }
        public DbSet<Team> Team { get; set; }
        public DbSet<Draft> Draft{ get; set; }

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

            builder.Entity<Request>(entity =>
            {
                entity.ToTable("Request");
                entity.HasMany(r => r.CompetitorInformations)
                    .WithOne(ci => ci.Request)
                    .HasForeignKey(ci => ci.RequestId);

                entity.HasMany(r => r.RequestProducts)
                    .WithOne(rp => rp.Request)
                    .HasForeignKey(rp => rp.RequestId);

                entity.HasOne(r => r.RequestSubmissionDetail)
                    .WithOne(rs => rs.Request)
                    .HasForeignKey<RequestSubmissionDetail>(rs => rs.RequestId);

                entity.HasOne(r => r.ProjectInformation)
                    .WithOne(pi => pi.Request)
                    .HasForeignKey<ProjectInformation>(pi => pi.RequestId);
            });

            builder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");
                entity.HasMany(r => r.ProductCategories)
                    .WithOne(ci => ci.Product)
                    .HasForeignKey(ci => ci.ProductId);
            });

            builder.Entity<RequestProduct>(entity =>
            {
                entity.ToTable("RequestProduct");
            });

            builder.Entity<CompetitorInformation>(entity =>
            {
                entity.ToTable("CompetitorInformation");
            });

            builder.Entity<RequestSubmissionDetail>(entity =>
            {
                entity.ToTable("RequestSubmissionDetail");
            });

            builder.Entity<ProjectInformation>(entity =>
            {
                entity.ToTable("ProjectInformation");
                entity.HasMany(pi => pi.ProjectInformationReasons)
                    .WithOne(pir => pir.ProjectInformation)
                    .HasForeignKey(pir => pir.ProjectInformationId);
            });

            builder.Entity<ProjectInformationReason>(entity =>
            {
                entity.ToTable("ProjectInformationReason");
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
