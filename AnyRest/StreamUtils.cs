using System;
using System.IO;

namespace AnyRest
{
    class StreamUtils
    {
        const int bufferSize = 4096;
        public static void CopyStream(Stream inStream, Stream outStream)
        {   
            var buffer = new byte[bufferSize];
            var read = inStream.Read(buffer, 0, bufferSize);
            while (read != 0)
            {
                outStream.Write(buffer, 0, read);
                read = inStream.Read(buffer, 0, bufferSize);
            }
        }

        public static void StdInToStdOut()
        {
            using (var stdIn = Console.OpenStandardInput())
            {
                using (var stdOut = Console.OpenStandardOutput())
                {
                    stdIn.CopyTo(stdOut);
                    stdOut.Flush();
                }
            }
        }
    }
}
