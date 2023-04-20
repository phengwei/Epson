using Epson.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.User
{
    public class Roles : BaseEntityExtension
    {
        public string Name { get; set; }
        public string NormalizedName { get; set; }

    }
}
