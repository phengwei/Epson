using Epson.Data.Context;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Autofac;
using Epson.Infrastructure;
using Autofac.Extensions.DependencyInjection;
using Epson.Services.Services.Products;
using Epson.Data;
using Epson.Core.Domain.Products;
using EpsonPortal.Controllers.API;
using Epson.Services.Interface.Products;
using Autofac.Core;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Epson.Services.DTO;

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

builder.Services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));

#region Services
builder.Services.AddScoped<IProductService, ProductService>();
#endregion

#region DbContext
builder.Services.AddScoped<EpsonSQLConnectionFactory>(provider =>
    new EpsonSQLConnectionFactory(builder.Configuration));
builder.Services.AddSingleton<IDbConnectionFactory, EpsonSQLConnectionFactory>();
builder.Services.AddDbContext<EpsonDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("EpsonDbConnection")));
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

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.UseSpaStaticFiles();

//app.MapWhen(x => !x.Request.Path.Value.StartsWith("/api"), builder =>
//{
//    builder.UseSpa(spa =>
//    {
//        spa.Options.SourcePath = "client-app";
//        if (app.Environment.IsDevelopment())
//        {
//            // Launch development server for Nuxt
//            spa.UseNuxtDevelopmentServer();
//        }
//    });
//});


app.Run();
