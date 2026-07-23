using Epson.Core.Domain.AuditTrail;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.Interface.AuditTrails
{
    public interface IAuditTrailService
    {
        public void CreateAuditTrail(int entityId, string entity, DateTime actionTime, string actor, string actionDetails, string action, IDbConnection connection = null, IDbTransaction transaction = null);
        public List<AuditTrail> GetProductAuditTrails();
        public List<AuditTrail> GetRequestAuditTrails(out int totalItems, Func<AuditTrail, bool> filter = null, string search = null, int? page = null, int? itemsPerPage = null);
        List<int> GetFulfilledRequestsByUser(string userId, string[] actions);

        //public List<AuditTrail> GetRejectionAuditTrails();
    }
}
