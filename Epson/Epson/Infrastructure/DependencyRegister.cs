using Autofac;
using Autofac.Core;
using Epson.Data;
using Epson.Data.Context;
using Epson.Services.Interface.Products;
using Epson.Services.Services.Products;
using Microsoft.EntityFrameworkCore;

namespace Epson.Infrastructure
{
    public class DependencyRegister : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterGeneric(typeof(EntityRepository<>))
                   .As(typeof(IRepository<>))
                   .InstancePerLifetimeScope();

            builder.RegisterType<ProductService>()
                   .As<IProductService>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<EpsonSQLConnectionFactory>()
                   .As<IDbConnectionFactory>()
                   .SingleInstance();

        }

    }
}
