using Epson.Core.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.Interface.Products
{
    public interface IProductService
    {
        public List<Product> GetProduct();
    }
}
