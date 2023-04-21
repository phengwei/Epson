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

builder.Services.AddScoped<EpsonSQLConnectionFactory>(provider =>
    new EpsonSQLConnectionFactory(builder.Configuration));

builder.Services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddSingleton<IDbConnectionFactory, EpsonSQLConnectionFactory>();

builder.Services.AddDbContext<EpsonDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("EpsonDbConnection")));

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

app.Run();
