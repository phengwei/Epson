using Epson.Data;
using Epson.Core.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epson.Services.Interface.Products;
using LinqToDB;

namespace Epson.Services.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _ProductRepository;

        public ProductService
            (IRepository<Product> productRepository)
        {
            _ProductRepository = productRepository;
        }

        public List<Product> GetProduct()
        {
            var products = _ProductRepository.GetAll();

            return products.ToList();
        }
    }
}
