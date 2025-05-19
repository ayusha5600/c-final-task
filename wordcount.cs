namespace Shared
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
}

