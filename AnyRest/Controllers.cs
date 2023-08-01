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
            var userEndpoint = (UserEndpoint)this.RouteData.Values["endpointSpecification"];
            var verbAction = userEndpoint.GetAction(Request.Method);
            if (verbAction == null)
                return NotFound($"No action defined for {Request.Method}");
            else
            {
                try
                {
                    var httpEnvironment = new HttpEnvironment(verbAction.ValidateQueryParms(Request), Request.Method, Request.Path, verbAction.ContentType, Request.Body);
                    try
                    {
                        foreach (var routeValue in Request.RouteValues)
                        {
                            if (routeValue.Value != null && routeValue.Value.GetType() == typeof(string))
                            {
                                if (routeValue.Key != "controller" && routeValue.Key != "action")
                                    httpEnvironment.RouteValues.Add(new KeyValuePair<string, string>(routeValue.Key, (string)routeValue.Value));
                            }
                        }
                        return verbAction.ReturnFromCommand(httpEnvironment, Response);
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
