﻿using Epson.Core.Domain.AuditTrail;
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
        public DbSet<SLAStaffLeave> SLAStaffLeave { get; set; }
        public DbSet<Team> Team { get; set; }

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
