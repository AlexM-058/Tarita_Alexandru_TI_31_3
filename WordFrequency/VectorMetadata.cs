namespace WordFrequency
{
    public static class VectorMetadata
    {
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
