using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
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
        static void StreamBodyToStdInput(Stream bodyStream, Stream stdIn)
        {
            try
            {
                bodyStream.CopyToAsync(stdIn).Wait();
            }
            catch (Exception ex)
            {
                var type = ex.GetType();
                var msg = ex.Message;
            }

            try
            {
                stdIn.Close();
            }
            catch (Exception ex)
            {
                var type = ex.GetType();
                var msg = ex.Message;
            }
        }

        static Process StartProcess(string commandLine, HttpEnvironment httpEnvironment)
        {
            Process p = new Process();

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;

            p.StartInfo.FileName = "cmd";
            p.StartInfo.Arguments = $"/c {commandLine}";

            p.StartInfo.Environment.Add($"AnyRESTHttpMethod", httpEnvironment.RequestMethod);
            p.StartInfo.Environment.Add($"AnyRESTPath", httpEnvironment.RequestPath);
            p.StartInfo.Environment.Add($"AnyRESTContentType", httpEnvironment.ContentType);

            foreach (var queryParm in httpEnvironment.QueryParms)
                p.StartInfo.Environment.Add($"AnyRESTQueryParm_{queryParm.Key}", queryParm.Value);

            foreach (var routeValue in httpEnvironment.RouteValues)
            {
                p.StartInfo.Environment.Add($"AnyRESTRouteParm_{routeValue.Key}", routeValue.Value);
            }

            if (p.Start())
            {
                Task.Run(() => StreamBodyToStdInput(httpEnvironment.RequestBody, p.StandardInput.BaseStream));
                return p;
            }
            else
                return null; //Throw instead
        }

        public static void WaitForProcessExit(Process p, int timeOut)
        {
            if (!p.WaitForExit(timeOut))
                p.Kill(true);
            p.Dispose();
        }

        public static CommandResult ExecuteCommand(string commandLine, HttpEnvironment httpEnvironment, int timeOut = -1)
        {
            using (Process p = StartProcess(commandLine, httpEnvironment))
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

        public static Stream ExecuteDataCommand(string commandLine, HttpEnvironment httpEnvironment, int timeOut = -1)
        {
            Process p = StartProcess(commandLine, httpEnvironment);
            Task.Run(() => WaitForProcessExit(p, timeOut));
            return p.StandardOutput.BaseStream;
        }
    }
}
