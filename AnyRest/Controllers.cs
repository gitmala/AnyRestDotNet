using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AnyRest
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AliveController : Controller
    {
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GET()
        {
            return Ok($"Im aliiive!!");
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class ConfigSchemaController : Controller
    {
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GET()
        {
            var gen = new Newtonsoft.Json.Schema.Generation.JSchemaGenerator();
            var schema = gen.Generate(typeof(FileConfig));
            return Ok(schema);
        }
    }
}
