﻿using Epson.Core.Domain.Products;
using Epson.Services.DTO.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.Interface.Products
{
    public interface IProductService
    {
        public ProductDTO GetProductById(int id);
        public List<ProductDTO> GetProduct();
        public void InsertProduct(Product product);
    }
}
