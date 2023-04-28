using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epson.Core.Domain.Base
{
    public abstract partial class BaseEntity
    {
        public int Id { get; set; }

        //todo caching of keys if necessary
    }
}
