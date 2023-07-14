using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Requests
{
    public class RequestSubmissionDetail
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string? DistributorName { get; set; }
        public string? ResellerName { get; set; }
        public string? ContactPersonName { get; set; }
        public string? TelephoneNo { get; set; }
        public string? FaxNo { get; set; }
        public string? Email { get; set; }
    }
}
