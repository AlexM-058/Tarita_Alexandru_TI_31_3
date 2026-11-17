namespace WordFrequency
{
    public static class VectorMetadata
    {
        public static Dictionary<string, int> CountTopics(params string[] folders)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (string folder in folders)
            {
                foreach (string file in Directory.GetFiles(folder, "*.txt"))
                {
                    foreach (string topic in ReadTopics(file))
                        counts[topic] = counts.GetValueOrDefault(topic) + 1;
                }
            }
            return counts;
        }

        public static string CreateSuffix(string articleFile)
        {
            return $" # {string.Join(" ", ReadTopics(articleFile))}";
        }

        public static string[] ReadTopics(string articleFile)
        {
            var topics = new List<string>();
            bool inTopics = false;
            using var reader = new StreamReader(articleFile);
            reader.ReadLine(); // The first line is article content, not metadata.

            while (reader.ReadLine() is string line)
            {
                line = line.Trim();
                if (line.Length == 0) continue;

                // Extracted Reuters class labels are qualified names, e.g. bip:topics:1.0.
                if (line.Contains(':'))
                {
                    inTopics = line == "bip:topics:1.0";
                }
                else if (inTopics)
                {
                    topics.Add(line);
                }
            }

            return topics.Distinct(StringComparer.Ordinal).ToArray();
        }
    }
}
