using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.DTO.Requests
{
    public class MakerDTO
    {
        public int id { get; set; }
        public string? fixStatus { get; set; }
        public DateTime? fixStatusDate { get; set; }
        public string? workNotes { get; set; }
        public DateTime? targetFinishDate { get; set; }
    }

}
