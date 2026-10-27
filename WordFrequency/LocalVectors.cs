namespace WordFrequency
{
    public static class LocalVectors
    {
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

        public static void SaveLocalVectors(string folder, string outputFolder)
        {
            string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.Ordinal);

            Directory.CreateDirectory(outputFolder);

            foreach (string file in files)
            {
                var localVector = CreateLocalVector(file);
                string outputFile = Path.Combine(outputFolder, Path.GetRelativePath(folder, file));
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
                string values = string.Join(" ", localVector.Select(entry => $"{entry.Key}:{entry.Value}"));
                File.WriteAllText(outputFile, values + Environment.NewLine);
            }

            Console.WriteLine($"{files.Length} vectori locali salvati in {outputFolder}.");
        }
    }
}
