using Epson.Core.Domain.AuditTrail;
using Epson.Data;
using Epson.Data.Context;
using Epson.Services.DTO.Products;
using Epson.Services.Interface.AuditTrails;
using System.Web.Mvc;

namespace Epson.Services.Services.AuditTrails
{
    public class AuditTrailService : IAuditTrailService
    {
        private readonly IRepository<AuditTrail> _auditTrailRepository;
        private readonly EpsonDbContext _context;

        public AuditTrailService(IRepository<AuditTrail> auditTrailRepository,
            EpsonDbContext context)
        {
            _context = context;
            _auditTrailRepository = auditTrailRepository;
        }

        public void CreateAuditTrail(int entityId, string entity, DateTime actionTime, string actor, string actionDetails, string action)
        {
            var auditEntry = new AuditTrail
            {
                EntityId = entityId,
                Entity = entity,
                ActionTime = actionTime,
                Actor = actor,
                ActionDetails = actionDetails,
                Action = action,
                CreatedOnUTC = DateTime.UtcNow
            };

            _auditTrailRepository.Add(auditEntry);
        }

        public List<AuditTrail> GetProductAuditTrails()
        {
            return _auditTrailRepository.Table.Where(x => x.Entity == "Product").ToList();
        }

        public List<int> GetFulfilledRequestsByUser(string userId, string[] actions)
        {
            var fulfilledRequestIds = _auditTrailRepository.Table
                .Where(log => log.Actor == userId && actions.Contains(log.Action))
                .Select(log => log.EntityId)
                .Distinct()
                .ToList();

            return fulfilledRequestIds;
        }

        public List<AuditTrail> GetRequestAuditTrails(out int totalItems, Func<AuditTrail, bool> filter = null, string search = null, int? page = null, int? itemsPerPage = null)
        {
            var query = _context.AuditTrail.AsQueryable();


            if (filter != null)
            {
                query = query.Where(filter).AsQueryable();
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.ToList().Where(x =>
                    (x.EntityId.ToString() == search)).AsQueryable();
            }

            totalItems = query.Count();

            return query.ToList();
        }
    }
}
