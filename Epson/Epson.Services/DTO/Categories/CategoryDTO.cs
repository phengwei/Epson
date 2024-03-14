using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.DTO.Categories
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BackupFulfiller1 { get; set; }
        public string BackupFulfiller2 { get; set; }
        public string EscalationFulfiller { get; set; }
    }
}
