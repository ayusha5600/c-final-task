using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace ScannerA
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
        static List<WordCount> wordCounts = new List<WordCount>();

        static void Main(string[] args)
        {
            try
            {
                System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)2;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to set CPU affinity: " + ex.Message);
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ScannerA <DirectoryPath> <PipeName>");
                return;
            }

            string directoryPath = args[0];
            string pipeName = args[1];

            Thread readerThread = new Thread(() => ReadAndIndexFiles(directoryPath));
            Thread senderThread = new Thread(() => SendData(pipeName));

            readerThread.Start();
            senderThread.Start();

            readerThread.Join();
            senderThread.Join();
        }

        static void ReadAndIndexFiles(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Directory does not exist.");
                return;
            }

            string[] txtFiles = Directory.GetFiles(folderPath, "*.txt");

            foreach (string file in txtFiles)
            {
                string content = File.ReadAllText(file);
                string[] words = content.Split(new[] { ' ', '\n', '\r', '.', ',', ';', ':', '!', '?', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                var grouped = words.GroupBy(w => w.ToLower());

                foreach (var group in grouped)
                {
                    wordCounts.Add(new WordCount
                    {
                        FileName = Path.GetFileName(file),
                        Word = group.Key,
                        Count = group.Count()
                    });
                }
            }
        }

        static void SendData(string pipeName)
        {
            try
            {
                
                while (wordCounts.Count == 0)
                {
                    Thread.Sleep(100);
                }

                using (var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
                {
                    Console.WriteLine($"Connecting to pipe: {pipeName}...");
                    pipeClient.Connect();

                    string json = JsonSerializer.Serialize(wordCounts);
                    byte[] buffer = Encoding.UTF8.GetBytes(json);
                    pipeClient.Write(buffer, 0, buffer.Length);
                    Console.WriteLine("Data sent successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data through pipe {pipeName}: {ex.Message}");
            }
        }
    }
}
