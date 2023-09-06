using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AnyRest
{
    [ApiController]
    [Route(ControllerRoute)]
    public class BultIn : Controller
    {
        public const string ControllerRoute = "builtin";

        [Route("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult status()
        {
            return Ok($"Im aliiive!!");
        }

        [Route("configschema")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult configschema()
        {
            var configSchema = StreamUtils.GetEmbeddedResource("AnyRest", "ConfigSchema.json");
            return Ok(configSchema);
        }
    }
}
