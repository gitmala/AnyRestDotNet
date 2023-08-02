using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AnyRest
{
    public class DynamicEndpoint : Controller
    {
        public IActionResult MethodHandler()
        {
            var endpoint = (Endpoint)this.RouteData.Values["endpointSpecification"];
            Request.RouteValues.Remove("endpointSpecification");

            var action = endpoint.GetAction(Request.Method);
            if (action == null)
                return NotFound($"No action defined for {Request.Method}");
            else
            {
                Request.RouteValues.Remove("action");
                Request.RouteValues.Remove("controller");
                try
                {
                    var actionEnvironment = action.MakeActionEnvironment(Request);
                    try
                    {
                        return action.Run(actionEnvironment, Response);
                    }
                    catch (Exception ex)
                    {
                        return Problem();
                    }
                }
                catch (ApplicationException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }

    [ApiController]
    [Route("api/v1/[controller]")]
    public class AliveController : Controller
    {
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GET(int foo)
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
