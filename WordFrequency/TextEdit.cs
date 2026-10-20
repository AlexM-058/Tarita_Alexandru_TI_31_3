using System.Text;
using System.Text.RegularExpressions;


namespace  WordFrequency
{
    public class TextEdit
    {
        private static readonly HashSet<string> StopWords =
            new HashSet<string>(File.ReadAllLines("stopwords.txt"));

        private static readonly PorterStemmer Stemmer = new PorterStemmer();

        public static string ToLowerCase(string text)
        {
          return text.ToLowerInvariant();
        }
        public static string RemovePunctuation(string text)
        {
            return Regex.Replace(text, "[^a-z ]", " ");
        }

        public static string RemoveStopWords(string text)
        {
            string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var kept = new List<string>();
            foreach (string word in words)
            {
                if (!StopWords.Contains(word))
                {
                    kept.Add(word);
                }
            }

            return string.Join(" ", kept);
        }

        public static string StemWords(string text)
        {
            string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var stemmed = new List<string>();
            foreach (string word in words)
            {
                stemmed.Add(Stemmer.StemWord(word));
            }

            return string.Join(" ", stemmed);
        }

        public static void ProcessFolder(string folder)
        {
            string[] txtFiles = Directory.GetFiles(folder, "*.txt");

            foreach (string file in txtFiles)
            {
                string metadata;
                string text;
                using (var reader = new StreamReader(file))
                {
                    text = reader.ReadLine() ?? string.Empty;
                    metadata = reader.ReadToEnd();
                }

                text = ToLowerCase(text);
                text = RemovePunctuation(text);
                text = RemoveStopWords(text);
                text = StemWords(text);

                File.WriteAllText(file, text + Environment.NewLine + metadata);
            }

            Console.WriteLine($"{folder}: {txtFiles.Length} fisiere procesate.");
        }
    }
}
