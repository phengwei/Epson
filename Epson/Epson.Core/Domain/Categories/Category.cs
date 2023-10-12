using Epson.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Categories
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BackupFulfiller1 { get; set; }
        public string BackupFulfiller2 { get; set; }
    }
}
