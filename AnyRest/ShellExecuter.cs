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
    class ShellExecuter
    {
        static void StreamBodyToStdInput(Stream bodyStream, Stream stdIn)
        {
            try
            {
                bodyStream.CopyToAsync(stdIn).Wait();
            }
            catch (Exception)
            {}

            try
            {
                stdIn.Close();
            }
            catch (Exception)
            {}
        }

        static Process StartProcess(string shell, string argumentsPrefix, string arguments, ActionEnvironment actionEnvironment)
        {
            var process = new Process();

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;

            process.StartInfo.FileName = shell;
            process.StartInfo.Arguments = $"{argumentsPrefix}{arguments}";

            process.StartInfo.Environment.Add($"AnyRESTHttpMethod", actionEnvironment.RequestMethod);
            process.StartInfo.Environment.Add($"AnyRESTPath", actionEnvironment.RequestPath);
            process.StartInfo.Environment.Add($"AnyRESTRequestId", actionEnvironment.RequestId);
            if (!String.IsNullOrEmpty(actionEnvironment.ContentType))
                process.StartInfo.Environment.Add($"AnyRESTContentType", actionEnvironment.ContentType);

            foreach (var queryParm in actionEnvironment.QueryParms)
                process.StartInfo.Environment.Add($"AnyRESTQueryParm_{queryParm.Key}", queryParm.Value);

            foreach (var routeValue in actionEnvironment.RouteValues)
            {
                process.StartInfo.Environment.Add($"AnyRESTRouteParm_{routeValue.Key}", routeValue.Value);
            }

            try
            {
                if (process.Start())
                {
                    Task.Run(() => StreamBodyToStdInput(actionEnvironment.RequestBody, process.StandardInput.BaseStream));
                    return process;
                }
                else
                    throw new ApplicationException("starting process returned null");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to start process", ex);
            }
        }

        public static void WaitForProcessExit(Process process, int timeOut)
        {
            if (!process.WaitForExit(timeOut))
                process.Kill(true);
            process.Dispose();
        }

        public static CommandResult GetCommandResult(string shell, string argumentsPrefix, string arguments, ActionEnvironment actionEnvironment, int timeOut = -1)
        {
            using (var process = StartProcess(shell, argumentsPrefix, arguments, actionEnvironment))
            {
                var stdOutputTask = process.StandardOutput.ReadToEndAsync();
                var stdErrorTask = process.StandardError.ReadToEndAsync();

                //Use WaitForProcessExit so process is killed after timeout?
                //In that case do not use using() since WaitForProcessExit disposes p
                process.WaitForExit(timeOut);

                var result = new CommandResult()
                {
                    stdOutput = stdOutputTask.Result,
                    stdError = stdErrorTask.Result,
                    exitCode = process.ExitCode
                };
                return result;
            }
        }

        public static Stream GetStreamResult(string shell, string argumentsPrefix, string arguments, ActionEnvironment actionEnvironment, int timeOut = -1)
        {
            var process = StartProcess(shell, argumentsPrefix, arguments, actionEnvironment);
            var returnSteam = process.StandardOutput.BaseStream;
            Task.Run(() => WaitForProcessExit(process, timeOut));
            return returnSteam;
        }
    }
}
