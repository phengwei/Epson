using Epson.Core.Domain.Base;
using Epson.Core.Domain.Categories;
using Epson.Core.Domain.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Products
{
    public class Product : BaseEntityExtension
    {
        public string SKU { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public decimal? DealerPrice { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
