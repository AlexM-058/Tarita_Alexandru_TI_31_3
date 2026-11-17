using System.Globalization;

namespace WordFrequency
{
    public static class FeatureSelection
    {
        // Indicele 1 din @data corespunde primului @attribute din fisier.
        public static (FeatureScore[] Features, LabeledDocument[] Documents) ReadDataset(string inputFile)
        {
            var features = new List<FeatureScore>();
            var documents = new List<LabeledDocument>();
            var words = new HashSet<string>(StringComparer.Ordinal);
            bool readingData = false;
            int lineNumber = 0;
            foreach (string line in File.ReadLines(inputFile))
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.Trim() == "@data" && !readingData)
                {
                    readingData = true;
                    continue;
                }

                if (!readingData)
                {
                    string[] parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 3 || parts[0] != "@attribute"
                        || !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double gain)
                        || !double.IsFinite(gain) || gain < 0 || !words.Add(parts[1]))
                        throw new InvalidDataException($"{inputFile}:{lineNumber}: atribut invalid sau duplicat.");
                    features.Add(new FeatureScore(parts[1], gain));
                    continue;
                }

                string[] sections = line.Split('#', StringSplitOptions.TrimEntries);
                if ((sections.Length != 2 && sections.Length != 3) || sections[1].Length == 0
                    || (sections.Length == 3 && sections[2] != "train" && sections[2] != "test"))
                    throw new InvalidDataException($"{inputFile}:{lineNumber}: asteptat vector # clase [# train/test].");

                var frequencies = new Dictionary<string, int>(StringComparer.Ordinal);
                foreach (string entry in sections[0].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] pair = entry.Split(':');
                    if (pair.Length != 2
                        || !int.TryParse(pair[0], out int index) || index < 1 || index > features.Count
                        || !int.TryParse(pair[1], out int count) || count < 0
                        || !frequencies.TryAdd(features[index - 1].Word, count))
                        throw new InvalidDataException($"{inputFile}:{lineNumber}: indice sau frecventa invalida.");
                }
                string[] classes = sections[1].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                    .Distinct(StringComparer.Ordinal).ToArray();
                // Un fisier fara marcaje reprezinta un singur set analizat integral.
                documents.Add(new LabeledDocument(classes, sections.Length == 2 || sections[2] == "train", frequencies));
            }
            if (!readingData)
                throw new InvalidDataException($"{inputFile}: lipseste sectiunea @data.");
            return (features.ToArray(), documents.ToArray());
        }

        public static void CalculateGain(string inputFile, string outputFile)
        {
            var dataset = ReadDataset(inputFile);
            FeatureScore[] scores = InformationGain.Calculate(dataset.Documents,
                dataset.Features.Select(feature => feature.Word).ToArray());
            // Copiem liniile originale: frecventele, clasele si marcajele nu se rescriu.
            string[] dataLines = File.ReadAllLines(inputFile)
                .SkipWhile(line => line.Trim() != "@data").ToArray();
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputFile))!);
            using var writer = new StreamWriter(outputFile);
            foreach (FeatureScore score in scores)
                writer.WriteLine($"@attribute {score.Word} {score.Gain.ToString("G17", CultureInfo.InvariantCulture)}");
            foreach (string line in dataLines) writer.WriteLine(line);
        }

        public static LabeledDocument[] LoadDocuments(string textFolder, bool isTraining)
        {
            var documents = new List<LabeledDocument>();
            foreach (string file in Directory.GetFiles(textFolder, "*.txt").Order(StringComparer.Ordinal))
            {
                string[] topics = VectorMetadata.ReadTopics(file);
                if (topics.Length == 0)
                    throw new InvalidDataException($"Missing topic labels: {file}");
                var frequencies = LocalVectors.CreateLocalVector(file);
                documents.Add(new LabeledDocument(topics, isTraining, frequencies));
            }
            return documents.ToArray();
        }

        public static void SaveDataset(string outputFile, IReadOnlyList<FeatureScore> features,
            IEnumerable<LabeledDocument> documents, bool includeSplit)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputFile))!);
            using var writer = new StreamWriter(outputFile);
            foreach (FeatureScore feature in features)
                writer.WriteLine($"@attribute {feature.Word} {feature.Gain.ToString("G17", CultureInfo.InvariantCulture)}");
            writer.WriteLine("@data");
            foreach (LabeledDocument document in documents)
            {
                var values = new List<string>();
                for (int index = 0; index < features.Count; index++)
                {
                    string word = features[index].Word;
                    if (document.Frequencies.TryGetValue(word, out int count) && count > 0)
                        values.Add($"{index + 1}:{count}");
                }
                string split = includeSplit ? $" # {(document.IsTraining ? "train" : "test")}" : "";
                writer.WriteLine($"{string.Join(" ", values)} # {string.Join(" ", document.Classes)}{split}");
            }
        }

        public static void Run(double minimumGain)
        {
            string outputFolder = "output/selection/laborator3";
            LabeledDocument[] documents = LoadDocuments("output/Reuters_34/Training", true)
                .Concat(LoadDocuments("output/Reuters_34/Testing", false)).ToArray();
            string[] vocabulary = GlobalVocabulary.CreateGlobalVector(
                "output/Reuters_34/Training", "output/Reuters_34/Testing");
            string originalFile = Path.Combine(outputFolder, "original.txt");
            SaveDataset(originalFile, vocabulary.Select(word => new FeatureScore(word, 0)).ToArray(),
                documents, true);
            ProcessDataset(originalFile, outputFolder, minimumGain);
        }

        public static void ProcessDataset(string inputFile, string outputFolder, double minimumGain)
        {
            string scoredFile = Path.Combine(outputFolder, "scored.txt");
            CalculateGain(inputFile, scoredFile);
            var dataset = ReadDataset(scoredFile);
            FeatureScore[] selected = InformationGain.SelectByMinimumGain(dataset.Features, minimumGain);
            SaveDataset(Path.Combine(outputFolder, "training.txt"), selected,
                dataset.Documents.Where(document => document.IsTraining), false);
            SaveDataset(Path.Combine(outputFolder, "testing.txt"), selected,
                dataset.Documents.Where(document => !document.IsTraining), false);
        }

    }
}
