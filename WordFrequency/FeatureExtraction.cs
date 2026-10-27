namespace WordFrequency
{
    public static class FeatureExtraction
    {
        public static void SaveLocalVectors(string folder, string outputFile)
        {
            string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.Ordinal);

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputFile))!);
            using var writer = new StreamWriter(outputFile, append: false);

            foreach (string file in files)
            {
                var localVector = CreateLocalVector(file);
                string document = Path.ChangeExtension(Path.GetRelativePath(folder, file), null);
                string values = string.Join(" ", localVector.Select(entry => $"{entry.Key}:{entry.Value}"));
                writer.WriteLine($"{document} # {values}");
            }

            Console.WriteLine($"{files.Length} vectori locali salvati in {outputFile}.");
        }


        public static string[] CreateGlobalVector(params string[] folders)
        {
            var uniqueWords = new SortedSet<string>(StringComparer.Ordinal);

            foreach (string folder in folders)
            {
                foreach (string file in Directory.EnumerateFiles(folder, "*.txt", SearchOption.AllDirectories))
                {
                    // Only the first line contains article text; the rest is metadata.
                    using var reader = new StreamReader(file);
                    string text = reader.ReadLine() ?? string.Empty;

                    foreach (string word in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
                    {
                        uniqueWords.Add(word);
                    }
                }
            }

            return uniqueWords.ToArray();
        }

        public static Dictionary<string, int> CreateGlobalWordIndices(string[] globalVector)
        {
            var wordIndices = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int index = 0; index < globalVector.Length; index++)
            {
                wordIndices.Add(globalVector[index], index);
            }

            return wordIndices;
        }

        public static SortedDictionary<string, int> CreateLocalVector(string file)
        {
            var frequencies = new SortedDictionary<string, int>(StringComparer.Ordinal);

            // Metadata follows the first line and must not be counted as words.
            using var reader = new StreamReader(file);
            string text = reader.ReadLine() ?? string.Empty;

            foreach (string word in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                frequencies.TryGetValue(word, out int count);
                frequencies[word] = count + 1;
            }

            return frequencies;
        }

        public static void SaveGlobalVector(string folder, string outputFile)
        {
            string[] globalVector = CreateGlobalVector(folder);

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputFile))!);
            using var writer = new StreamWriter(outputFile, append: false);
            for (int index = 0; index < globalVector.Length; index++)
            {
                writer.WriteLine($"{index}:{globalVector[index]}");
            }

            Console.WriteLine($"Vector global cu {globalVector.Length} cuvinte unice salvat in {outputFile}.");
        }
    }
}
