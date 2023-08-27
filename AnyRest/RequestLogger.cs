using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Net;

namespace AnyRest
{
    public interface IRequestLogger
    {
        public void LogRequest(string method, string url, IPAddress clientIp, Guid requestId, string endpointId, HttpStatusCode httpStatusCode, string errorText);
        public void LogRequest(HttpContext httpContext, Guid requestId, string endpointId, HttpStatusCode httpStatusCode, string errorText);
    }

    class ConsoleLogger : IRequestLogger
    {
        public void LogRequest(string method, string url, IPAddress clientIp, Guid requestId, string endpointId, HttpStatusCode httpStatusCode, string errorText)
        {
            System.Console.WriteLine($"{method}, {url}, {clientIp}, {requestId}, {endpointId}, {httpStatusCode}, {errorText}");
        }

        public void LogRequest(HttpContext httpContext, Guid requestId, string endpointId, HttpStatusCode httpStatusCode, string errorText)
        {
            LogRequest(httpContext.Request.Method, httpContext.Request.GetDisplayUrl(), httpContext.Connection.RemoteIpAddress, requestId, endpointId, httpStatusCode, errorText);
        }
    }
}
