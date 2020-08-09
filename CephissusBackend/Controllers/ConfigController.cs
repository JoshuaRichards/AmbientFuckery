using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace CephissusBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigController : ControllerBase
    {
        [Route("values")]
        [HttpGet]
        public ActionResult<object> GetValues()
        {
            var rng = new Random();

            return Enumerable.Range(1, 20).Select(_ => rng.Next()).ToList();
        }
    }
}
