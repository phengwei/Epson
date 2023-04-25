using System.Collections.Generic;

namespace Epson.Model.Common
{
    public class BaseResponseModel
    {
        public BaseResponseModel()
        {
            ErrorList = new List<string>();
        }

        public string Message { get; set; }

        public List<string> ErrorList { get; set; }
    }
}
