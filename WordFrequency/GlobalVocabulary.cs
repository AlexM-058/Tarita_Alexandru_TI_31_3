namespace WordFrequency
{
    public static class GlobalVocabulary
    {
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

        public static void SaveGlobalVector(string[] globalVector, string outputFile)
        {

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputFile))!);
            using var writer = new StreamWriter(outputFile, append: false);
            for (int index = 0; index < globalVector.Length; index++)
            {
                writer.WriteLine($"{index}:{globalVector[index]}");
            }

        }
    }
}
