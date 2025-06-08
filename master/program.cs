using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Diagnostics;

namespace Master
{
    public class WordCount
    {
        public string FileName { get; set; }
        public string Word { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return $"{FileName}:{Word}:{Count}";
        }
    }

    class Program
    {
        static List<WordCount> allResults = new List<WordCount>();
        static object locker = new object();

        static void Main(string[] args)
        {
            // Set CPU affinity to core 0
            try
            {
                Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)1; // core 0 = bit 0 set
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to set CPU affinity: " + ex.Message);
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Master <PipeName1> <PipeName2>");
                return;
            }

            string pipeName1 = args[0]; // "agent1"
            string pipeName2 = args[1]; // "agent2"

            Thread agent1Thread = new Thread(() => ReceiveFromPipe(pipeName1));
            Thread agent2Thread = new Thread(() => ReceiveFromPipe(pipeName2));

            agent1Thread.Start();
            agent2Thread.Start();

            agent1Thread.Join();
            agent2Thread.Join();

            Console.WriteLine("\n--- Merged Word Count Results ---");
            foreach (var result in allResults)
            {
                Console.WriteLine(result.ToString());
            }
        }

        static void ReceiveFromPipe(string pipeName)
        {
            try
            {
                using var server = new NamedPipeServerStream(pipeName, PipeDirection.In);
                Console.WriteLine($"Waiting for connection on pipe: {pipeName}...");
                server.WaitForConnection();

                using MemoryStream ms = new MemoryStream();
                server.CopyTo(ms);

                string json = Encoding.UTF8.GetString(ms.ToArray());
                var list = JsonSerializer.Deserialize<List<WordCount>>(json);

                if (list != null)
                {
                    lock (locker)
                    {
                        allResults.AddRange(list);
                    }
                }

                Console.WriteLine($"Data received from {pipeName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in pipe {pipeName}: {ex.Message}");
            }
        }
    }
}
