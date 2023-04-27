using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Epson.Services.DTO.Base
{
    public enum AppResponse : int
    {
        [Description("Invalid username or password.")]
        InvalidCredential = 401
    }
}
