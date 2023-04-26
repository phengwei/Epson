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
using Epson.Models.Users;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v3", new OpenApiInfo { Title = "Epson APIs", Version = "v3" });
});

//var containerBuilder = new ContainerBuilder();
//containerBuilder.RegisterModule(new DependencyRegister());

//var container = containerBuilder.Build();
//builder.Services.AddAutofac(c => c.Populate((IEnumerable<ServiceDescriptor>)container));

builder.Services.AddControllers();

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
builder.Services.AddSingleton<JwtSettings>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
builder.Services.AddScoped<JwtService>();
#endregion

#region DbContext
builder.Services.AddScoped<EpsonSQLConnectionFactory>(provider =>
    new EpsonSQLConnectionFactory(builder.Configuration));
builder.Services.AddSingleton<IDbConnectionFactory, EpsonSQLConnectionFactory>();
builder.Services.AddDbContext<EpsonDbContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("EpsonDbConnection"),
        new MySqlServerVersion(new Version(8, 0, 22))
));
#endregion

#region Authentication
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
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
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
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
    c.SwaggerEndpoint("/swagger/v3/swagger.json", "My API v1");
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
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
