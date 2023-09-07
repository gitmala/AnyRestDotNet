using System;
using System.IO;
using System.Threading;

namespace AnyRest
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "-io")
            {
                StreamUtils.StdInToStdOut();
                return 0;
            }
            else if (args.Length == 2 && args[0] == "-s")
            {
                int time;
                if (Int32.TryParse(args[1], out time))
                {
                    Thread.Sleep(time);
                    return 0;
                }
                return 1;
            }
            else
            {
                var filename = "config.json";
                if (args.Length > 0)
                    filename = args[0];
                if (!File.Exists(filename))
                {
                    Console.Write($"Config file \"{filename}\" not found");
                    return 1;
                }

                EndpointList endpoints;
#if !DEBUG
                try
                {
#endif
                    endpoints = FileConfig.LoadFromFile(filename);
#if !DEBUG
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Config error: {ex.Message}");
                    return 1;
                }
#endif          
                Console.WriteLine($"Endpoints from file {filename}");
                foreach (var endpoint in endpoints)
                {
                    Console.WriteLine("    " + endpoint.AsString());
                }
                Console.WriteLine("");
                Console.WriteLine("Starting service");
                Service.StartService(endpoints);

                return 0;
            }
        }
    }
}
