using Epson.Data.Context;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Epson.Services.Services.Products;
using Epson.Data;
using Epson.Services.Interface.Products;
using AutoMapper;
using Epson.Services.DTO;
using NuxtIntegration.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Epson.Model.Users;
using Epson.Core.Domain.Users;
using Epson.Factories;
using Serilog;
using Epson.Infrastructure;
using Epson.Services.Services.Email;
using Epson.Services.Interface.Email;
using Epson.Job;
using Epson.Services.Interface.Requests;
using Epson.Services.Services.Requests;
using Epson.Services.Interface.SLA;
using Epson.Services.Services.SLA;
using Epson.Services.Interface.AuditTrails;
using Epson.Services.Services.AuditTrails;
using Epson.Services.Interface.Categories;
using Epson.Services.Services.Categories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Epson.Core.Domain.SLA;
using Epson.Services.Interface.Report;
using Epson.Services.Interface.Users;
using Epson.Services.Services.Users;
using System.Reflection;
using Epson.Services.Services.Report;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

#region Serilog
using var log = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});

builder.Services.AddSingleton<Serilog.ILogger>(log);
builder.Host.UseSerilog();
#endregion

#region Authentication
builder.Services.AddIdentity<ApplicationUser, Role>()
    .AddRoleManager<RoleManager<Role>>()
    .AddEntityFrameworkStores<EpsonDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<ApplicationUser>, UserManager<ApplicationUser>>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Sales", policy => policy.RequireRole("Sales"));
    options.AddPolicy("Product", policy => policy.RequireRole("Product"));
});

builder.Services.AddOptions<SLASetting>().Bind(builder.Configuration.GetSection("SLA"));
builder.Services.AddOptions<JwtSettings>().Bind(builder.Configuration.GetSection("Jwt"));
#endregion

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "Epson APIs", Version = "v2" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

#region DbContext
builder.Services.AddScoped<EpsonSQLConnectionFactory>(provider =>
    new EpsonSQLConnectionFactory(builder.Configuration));
builder.Services.AddSingleton<IDbConnectionFactory, EpsonSQLConnectionFactory>();
builder.Services.AddDbContext<EpsonDbContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("EpsonDbConnection"),
        new MySqlServerVersion(new Version(8, 0, 22))
));
#endregion

builder.Services.AddControllers();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

#region AutoMapper
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AutoMappingProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
#endregion

#region Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IAuditTrailService, AuditTrailService>();
builder.Services.AddScoped<ISLAService, SLAService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IWorkContext, WorkContext>();
builder.Services.AddSingleton<JwtSettings>();
builder.Services.AddSingleton<SLASetting>();
builder.Services.AddSingleton<IOptionsMonitor<SLASetting>, OptionsMonitor<SLASetting>>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
builder.Services.AddScoped<JwtService>();
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddHostedService<DeadlineReminderService>();
#endregion

#region Factories
builder.Services.AddScoped<IProductModelFactory, ProductModelFactory>();
builder.Services.AddScoped<ICategoryModelFactory, CategoryModelFactory>();
builder.Services.AddScoped<IRequestModelFactory, RequestModelFactory>();
builder.Services.AddScoped<ISLAModelFactory, SLAModelFactory>();
#endregion

builder.Services.AddSpaStaticFiles(options => options.RootPath = "client-app/dist");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "My API v2");
});

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Add(
        "Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' https:; " + 
        "style-src 'self' 'unsafe-inline' https:; " + 
        "img-src 'self' https: data:; " + 
        "font-src 'self' https: data:; " + 
        "frame-src 'self'; " + 
        "connect-src 'self' https:; " + 
        "object-src 'none'; " + 
        "form-action 'self'; " +
        "frame-ancestors 'self'; " 
    );
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("X-Powered-By");
        return Task.CompletedTask;
    });
    await next.Invoke();
});


// app.MapRazorPages();
app.MapControllers();

app.UseSpaStaticFiles();

app.MapWhen(x => !x.Request.Path.Value.StartsWith("/api"), builder =>
{
    builder.UseSpa(spa =>
    {
        spa.Options.SourcePath = "client-app";
        if (app.Environment.IsDevelopment())
        {
            // Launch development server for Nuxt
            spa.UseNuxtDevelopmentServer();
        }
    });
});


app.Run();
