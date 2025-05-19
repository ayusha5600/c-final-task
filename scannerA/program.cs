using System; using System.IO; using System.Collections.Generic;
using System.Text.Json; using System.IO.Pipes; using System.Threading;
using System.Linq; using System.Text;

namespace ScannerA
{
    class WcData
    {
        public string f; public string w; public int c;
        public override string ToString() { return f + ":" + w + ":" + c; }
    }

    internal class P
    {
        static List<WcData> xX = new List<WcData>();

        static void Main(string[] a)
        {
            if (a.Length < 2)
            {
                Console.WriteLine("Arg missing: usage: app <dir> <pipe>");
                return;
            }

            var p1 = a[0]; var p2 = a[1];

            var tR = new Thread(() => R(p1));
            var tS = new Thread(() => S(p2));
            tR.Start(); tS.Start();
            tR.Join(); tS.Join();
        }

        static void R(string p)
        {
            if (!Directory.Exists(p)) { Console.WriteLine("No dir found."); return; }

            var ff = Directory.GetFiles(p, "*.txt");

            foreach (var z in ff)
            {
                var t = File.ReadAllText(z);
                var s = t.Split(new[] { ' ', '\n','\r','.',',',';',':','!','?','-','_' },
                    StringSplitOptions.RemoveEmptyEntries);

                var g = s.GroupBy(q => q.ToLower());

                foreach (var gr in g)
                {
                    xX.Add(new WcData { f = Path.GetFileName(z), w = gr.Key, c = gr.Count() });
                }
            }
        }

        static void S(string n)
        {
            while (xX.Count == 0) Thread.Sleep(88);

            using (var pc = new NamedPipeClientStream(".", n, PipeDirection.Out))
            {
                Console.WriteLine("Connecting pipe..");
                pc.Connect();

                var js = JsonSerializer.Serialize(xX);
                var b = Encoding.UTF8.GetBytes(js);
                pc.Write(b, 0, b.Length);
                Console.WriteLine("Sent ✔");
            }
        }
    }
}
