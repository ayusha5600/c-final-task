// Import required namespaces from the .NET framework
using System;
using System.Collections.Generic;   // For using generic collections like List
using System.IO;                    // For file and directory handling
using System.IO.Pipes;              // For inter-process communication using named pipes
using System.Linq;                  // For LINQ operations like grouping and counting
using System.Text;                  // For encoding strings to bytes
using System.Text.Json;            // For JSON serialization/deserialization
using System.Threading;            // For working with threads (multi-threading)

// Define a namespace for this program
namespace ScannerA
{
    // Class to represent a word found in a file along with its count
    public class WordCount
    {
        public string FileName { get; set; } // The name of the file where the word was found
        public string Word { get; set; }     // The actual word
        public int Count { get; set; }       // How many times the word appears

        // Override ToString method to format output when object is printed
        public override string ToString()
        {
            return $"{FileName}:{Word}:{Count}";
        }
    }

    // Main program class
    class Program
    {
        // List to store all WordCount objects collected from the text files
        static List<WordCount> wordCounts = new List<WordCount>();

        // Entry point of the program
        static void Main(string[] args)
        {
            // If not enough arguments are provided, show usage info and exit
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ScannerA <DirectoryPath> <PipeName>");
                return;
            }

            string directoryPath = args[0]; // Folder where .txt files are located
            string pipeName = args[1];      // The name of the pipe to connect to

            // Create two threads: one for reading files, one for sending data
            Thread readerThread = new Thread(() => ReadAndIndexFiles(directoryPath));
            Thread senderThread = new Thread(() => SendData(pipeName));

            // Start both threads
            readerThread.Start();
            senderThread.Start();

            // Wait for both threads to finish before exiting
            readerThread.Join();
            senderThread.Join();
        }

        // Method to read all .txt files in the directory and count word occurrences
        static void ReadAndIndexFiles(string folderPath)
        {
            // If the directory doesn't exist, exit early
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Directory does not exist.");
                return;
            }

            // Get all text file paths in the folder
            string[] txtFiles = Directory.GetFiles(folderPath, "*.txt");

            // Loop through each file
            foreach (string file in txtFiles)
            {
                // Read the entire file content
                string content = File.ReadAllText(file);

                // Split content into words using common delimiters
                string[] words = content.Split(new[] { ' ', '\n', '\r', '.', ',', ';', ':', '!', '?', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

                // Group words by lowercase form for case-insensitive counting
                var grouped = words.GroupBy(w => w.ToLower());

                // For each word group, create and add a WordCount object
                foreach (var group in grouped)
                {
                    wordCounts.Add(new WordCount
                    {
                        FileName = Path.GetFileName(file), // Only the file name, not full path
                        Word = group.Key,                  // The word
                        Count = group.Count()              // Number of times it appears
                    });
                }
            }
        }

        // Method to send the word count data through a named pipe
        static void SendData(string pipeName)
        {
            // Wait until data is available
            while (wordCounts.Count == 0)
            {
                Thread.Sleep(100); // Wait 100 milliseconds
            }

            // Create and connect to the named pipe server
            using (var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                Console.WriteLine($"Connecting to pipe: {pipeName}...");
                pipeClient.Connect(); // Connect to the pipe

                // Convert wordCounts list to JSON format
                string json = JsonSerializer.Serialize(wordCounts);

                // Convert JSON string to byte array
                byte[] buffer = Encoding.UTF8.GetBytes(json);

                // Write the data to the pipe
                pipeClient.Write(buffer, 0, buffer.Length);
                Console.WriteLine("Data sent successfully.");
            }
        }
    }
}
