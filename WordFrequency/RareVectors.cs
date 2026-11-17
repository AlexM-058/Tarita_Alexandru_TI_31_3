namespace WordFrequency
{
    public static class RareVectors
    {
        public static void SaveRareVectors(string folder, string outputFolder, string[] globalVector)
        {
            Dictionary<string, int> wordIndices = GlobalVocabulary.CreateGlobalWordIndices(globalVector);
            string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.Ordinal);

            Directory.CreateDirectory(outputFolder);

            foreach (string file in files)
            {
                var rareVector = new SortedDictionary<int, int>();
                foreach (var entry in LocalVectors.CreateLocalVector(file))
                {
                    if (wordIndices.TryGetValue(entry.Key, out int index))
                        rareVector.Add(index, entry.Value);
                }

                string outputFile = Path.Combine(outputFolder, Path.GetRelativePath(folder, file));
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
                string values = string.Join(" ", rareVector.Select(entry => $"{entry.Key}:{entry.Value}"));
                File.WriteAllText(outputFile, values + VectorMetadata.CreateSuffix(file) + Environment.NewLine);
            }

        }
    }
}
