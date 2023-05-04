using Epson.Core.Domain.AuditTrail;
using Epson.Data;
using Epson.Services.Interface.AuditTrails;

namespace Epson.Services.Services.AuditTrails
{
    public class AuditTrailService : IAuditTrailService
    {
        private readonly IRepository<AuditTrail> _auditTrailRepository;

        public AuditTrailService(IRepository<AuditTrail> auditTrailRepository)
        {
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
    }
}
