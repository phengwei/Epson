using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Epson.Core.Domain.Base
{
    public abstract partial class BaseEntityExtension : BaseEntity
    {
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }


        public void CreateAudit(string actorId)
        {
            CreatedById = actorId;
            CreatedOnUTC = DateTime.UtcNow;
            UpdatedById = actorId;
            UpdatedOnUTC = DateTime.UtcNow;
        }

        public void UpdateAudit(string actorId)
        {
            UpdatedById = actorId;
            UpdatedOnUTC = DateTime.UtcNow;
        }

        public void UpdateAudit(BaseEntityExtension oldModel, string actorId)
        {
            CreatedById = oldModel.CreatedById;
            CreatedOnUTC = oldModel.CreatedOnUTC;
            UpdatedById = actorId;
            UpdatedOnUTC = DateTime.UtcNow;
        }
    }

}
