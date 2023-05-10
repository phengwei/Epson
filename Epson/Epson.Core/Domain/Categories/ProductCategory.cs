using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Categories
{
    public class ProductCategory
    {
        public int Id { get; set; }
        public int categoryId { get; set; }
        public int productId { get; set; }
    }
}
