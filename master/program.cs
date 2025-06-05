using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Master
{
    // class that holds the result for a specific word in a specific file.
    public class WordCount
    {
        public string FileName { get; set; }
        public string Word { get; set; }
        public int Count { get; set; }

        //  print the result in a readable format.
        public override string ToString()
        {
            return $"{FileName}:{Word}:{Count}";
        }
    }

    class Program
    {
        // Shared the list to store results from both agents.
        static List<WordCount> allResults = new List<WordCount>();

        // Locker object for thread safety when adding results to the list.
        static object locker = new object();

        static void Main(string[] args)
        {
            // checking if the pipe names are passed from command line.
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Master <PipeName1> <PipeName2>");
                return;
            }

            // Reading pipe names from arguments.
            string pipeName1 = args[0]; // e.g., "agent1"
            string pipeName2 = args[1]; // e.g., "agent2"

            // Creating two threads to read data from each agent (pipe).
            Thread agent1Thread = new Thread(() => ReceiveFromPipe(pipeName1));
            Thread agent2Thread = new Thread(() => ReceiveFromPipe(pipeName2));

            // Starting both threads.
            agent1Thread.Start();
            agent2Thread.Start();

            // Making sure both threads complete before we move on or not.
            agent1Thread.Join();
            agent2Thread.Join();

            // After both agents send their data and print the combined results.
            Console.WriteLine("\n--- Merged Word Count Results ---");
            foreach (var result in allResults)
            {
                Console.WriteLine(result.ToString());
            }
        }

        // This method handles reading from a named pipe.
        static void ReceiveFromPipe(string pipeName)
        {
            // Creating the server side of the pipe and waiting for agent to connect.
            using var server = new NamedPipeServerStream(pipeName, PipeDirection.In);
            Console.WriteLine($"Waiting for connection on pipe: {pipeName}...");
            server.WaitForConnection();

            // Reading all data sent through the pipe.
            using MemoryStream ms = new MemoryStream();
            server.CopyTo(ms);

            // Converting the byte data into a string (JSON format).
            string json = Encoding.UTF8.GetString(ms.ToArray());

            // decoding the JSON string back to a list of WordCount objects.
            var list = JsonSerializer.Deserialize<List<WordCount>>(json);

            // Adding the deserialized list to the shared result list.
            if (list != null)
            {
                lock (locker) // Making sure multiple threads don’t access the list at the same time.
                {
                    allResults.AddRange(list);
                }
            }

            Console.WriteLine($"Data received from {pipeName}.");
        }
    }
}
