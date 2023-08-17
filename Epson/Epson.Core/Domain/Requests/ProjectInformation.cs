using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Requests
{
    public class ProjectInformation
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectId { get; set; }
        public string Industry { get; set; }
        public string Type { get; set; }
        public DateTime ClosingDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string CompanyAddress { get; set; }
        public string ContactPersonName { get; set; }
        public string TelephoneNo { get; set; }
        public string Email { get; set; }
        public string Requirements { get; set; }
        public string CustomerApplications { get; set; }
        public decimal Budget { get; set; }
        public string StaggeredComments { get; set; }
        public string StaggeredMonth { get; set; }
        public string OtherInformation { get; set; }

    }
}
