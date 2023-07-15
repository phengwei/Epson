using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Requests
{
    public class ProjectInformationReason
    {
        public int Id { get; set; }
        public int ProjectInformationId { get; set; }
        public string? SelectedReason { get; set; }
        public string? AdditionalInfo { get; set; }
    }
}
