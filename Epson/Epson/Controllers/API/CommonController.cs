using Microsoft.AspNetCore.Mvc;

namespace EpsonPortal.Controllers.API
{
    [Route("api/[controller]")]
    public class CommonController : Controller
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
