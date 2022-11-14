using System;
using System.Threading;

namespace AnyRest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "-io")
                StreamUtils.StdInToStdOut();
            else if (args.Length == 2 && args[0] == "-s")
            {
                int time;
                if(Int32.TryParse(args[1], out time))
                    Thread.Sleep(time);
            }
            else
                Service.StartService();
        }
    }
}
