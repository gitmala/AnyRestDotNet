using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AnyRest
{
    public class CommandResult
    {
        public int exitCode { get; set; }
        public string stdOutput { get; set; }
        public string stdError { get; set; }
    }
    class CommandExecuter
    {
        static void streamBodyToStdInput(HttpRequest request, Process p)
        {
            try
            {
                request.Body.CopyToAsync(p.StandardInput.BaseStream).Wait();
            }
            catch (Exception ex)
            {
                var type = ex.GetType();
                var msg = ex.Message;
            }

            try
            {
                p.StandardInput.Close();
            }
            catch (Exception ex)
            {
                var type = ex.GetType();
                var msg = ex.Message;
            }
        }

        static Process StartProcess(string commandLine, HttpRequest request)
        {
            Process p = new Process();

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;

            p.StartInfo.FileName = "cmd";
            p.StartInfo.Arguments = $"/c {commandLine}";

            p.StartInfo.Environment.Add($"AnyRESTHttpMethod", request.Method);
            p.StartInfo.Environment.Add($"AnyRESTPath", request.Path);

            foreach (var key in request.Query.Keys)
                p.StartInfo.Environment.Add($"AnyRESTQueryParm_{key}", request.Query[key]);

            foreach (var routeValue in request.RouteValues)
            {
                if (routeValue.Value != null && routeValue.Value.GetType() == typeof(string))
                    p.StartInfo.Environment.Add($"AnyRESTRouteParm_{routeValue.Key}", (string)routeValue.Value);
            }

            p.Start();

            Task.Run(() => streamBodyToStdInput(request, p));

            return p;
        }

        public static void WaitForProcessExit(Process p, int timeOut)
        {
            if (!p.WaitForExit(timeOut))
                p.Kill(true);
            p.Dispose();
        }

        public static CommandResult ExecuteCommand(string commandLine, HttpRequest request, int timeOut = -1)
        {
            using (Process p = StartProcess(commandLine, request))
            {
                var stdOutputTask = p.StandardOutput.ReadToEndAsync();
                var stdErrorTask = p.StandardError.ReadToEndAsync();

                //Use WaitForProcessExit so process is killed after timeout?
                //In that case do not use using() since WaitForProcessExit disposes p
                p.WaitForExit(timeOut);

                var result = new CommandResult()
                {
                    stdOutput = stdOutputTask.Result,
                    stdError = stdErrorTask.Result,
                    exitCode = p.ExitCode
                };
                return result;
            }
        }

        public static Stream ExecuteDataCommand(string commandLine, HttpRequest request, int timeOut = -1)
        {
            Process p = StartProcess(commandLine, request);
            Task.Run(() => WaitForProcessExit(p, timeOut));
            return p.StandardOutput.BaseStream;
        }
    }
}
