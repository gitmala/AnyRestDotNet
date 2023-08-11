using Newtonsoft.Json.Schema;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AnyRest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "-io")
            {
                StreamUtils.StdInToStdOut();
            }
            else if (args.Length == 2 && args[0] == "-s")
            {
                int time;
                if (Int32.TryParse(args[1], out time))
                    Thread.Sleep(time);
            }
            else
            {
                var filename = "config.json";
                Endpoints endpoints = FileConfig.LoadFromFile(filename);
                Console.WriteLine($"Endpoints from file {filename}");
                foreach (var endpoint in endpoints)
                {
                    Console.WriteLine("    " + endpoint.AsString());
                }
                Console.WriteLine("");
                Console.WriteLine("Starting service");
                Service.StartService(endpoints);
            }
        }
    }
}
