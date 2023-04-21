using Epson.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Products
{
    public class Product : BaseEntityExtension
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
