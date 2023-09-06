using System;
using System.IO;
using System.Reflection;

namespace AnyRest
{
    class StreamUtils
    {
        public static string GetEmbeddedResource(string ns, string res)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}.{1}", ns, res))))
            {
                return reader.ReadToEnd();
            }
        }

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
