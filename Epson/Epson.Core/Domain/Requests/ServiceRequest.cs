using Epson.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Requests
{
    public class ServiceRequest : BaseEntityExtension
    {
        public string serviceRequestNo { get; set; }
        public string owner { get; set; }
        public string ownerGroup { get; set; }
        public string status { get; set; }
        public DateTime statusDate { get; set; }
        public string fixStatus { get; set; }
        public DateTime fixStatusDate { get; set; }
        public string paymentStatus { get; set; }
        public DateTime paymentStatusDate { get; set; }
        public string requester { get; set; }
        public string requesterName { get; set; }
        public string requesterPhone { get; set; }
        public string requesterEmail { get; set; }
        public string reportedBy { get; set; }
        public string reportedByName { get; set; }
        public string reportedByPhone { get; set; }
        public string reportedByEmail { get; set; }
        public string classificationPath { get; set; }
        public string services { get; set; }
        public string category { get; set; }
        public string section { get; set; }
        public string timeTracking { get; set; }
        public string summary { get; set; }
        public string details { get; set; }
        public DateTime reportedDate { get; set; }
        public DateTime requesterAffectedDate { get; set; }
        public DateTime targetFinishDate { get; set; }
        public string standardTAT { get; set; }
        public DateTime actualStartDate { get; set; }
        public DateTime actualFinishDate { get; set; }
        public string workNotes { get; set; }
        public string verificationNotes { get; set; }
        public string approvedBy { get; set; }
        public string approvedByName { get; set; }
        public string checkedBy { get; set; }
        public string checkedByName { get; set; }
        public int serviceRequestStatus { get; set; }
        public TimeSpan? timeToResolution { get; set; }
        public bool? isBreached { get; set; }
    }

}
