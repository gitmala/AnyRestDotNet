using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace AnyRest
{
    public class DynamicEndpoint : Controller
    {
        public IActionResult MethodHandler()
        {
            var endpoint = (Endpoint)this.RouteData.Values["endpointSpecification"];

            Request.RouteValues.Remove("endpointSpecification");
            Request.RouteValues.Remove("action");
            Request.RouteValues.Remove("controller");

            try
            {
                var action = endpoint.GetAction(Request.Method);
                try
                {
                    var actionEnvironment = action.MakeActionEnvironment(Request);
                    try
                    {
                        return action.Run(actionEnvironment, Response);
                    }
                    catch (Exception ex)
                    {
                        return Problem(ex.Message);
                    }
                }
                catch (ApplicationException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"No action of type {Request.Method} for endpoint {endpoint.Id}");
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
