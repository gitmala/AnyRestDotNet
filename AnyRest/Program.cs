using System;
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
                Endpoints endpoints;
                try
                {
                    endpoints = FileConfig.LoadFromFile(filename);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Config error: {ex.Message}");
                    return 1;
                }
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
