using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.DTO.Requests
{
    public class CheckerDTO
    {
        public int id { get; set; }
        public string? paymentStatus { get; set; }
        public DateTime? paymentStatusDate { get; set; }
        public string? verificationNotes { get; set; }
        public DateTime? actualFinishDate { get; set; }
    }

}
