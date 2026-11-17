using System.Globalization;
using System.Xml.Linq;

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
                if (sections.Length != 3 || sections[1].Length == 0
                    || (sections[2] != "train" && sections[2] != "test"))
                    throw new InvalidDataException($"{inputFile}:{lineNumber}: asteptat vector # clasa # train/test.");

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
                documents.Add(new LabeledDocument(sections[1], sections[2] == "train", frequencies));
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

        public static LabeledDocument[] LoadDocuments(string xmlFolder, string textFolder,
            bool isTraining, string targetTopic)
        {
            var documents = new List<LabeledDocument>();
            foreach (string file in Directory.GetFiles(xmlFolder, "*.xml").Order(StringComparer.Ordinal))
            {
                XElement? metadata = XDocument.Load(file).Root?.Element("metadata");
                string[] topics = metadata?.Elements("codes")
                    .Where(group => (string?)group.Attribute("class") == "bip:topics:1.0")
                    .SelectMany(group => group.Elements("code"))
                    .Select(code => (string?)code.Attribute("code"))
                    .Where(code => !string.IsNullOrWhiteSpace(code)).Select(code => code!).ToArray()
                    ?? [];
                if (topics.Length == 0)
                    throw new InvalidDataException($"Missing topic labels: {file}");
                string id = Path.GetFileNameWithoutExtension(file);
                string className = topics.Contains(targetTopic) ? targetTopic : $"not_{targetTopic}";
                var frequencies = LocalVectors.CreateLocalVector(Path.Combine(textFolder, id + ".txt"));
                documents.Add(new LabeledDocument(className, isTraining, frequencies));
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
                writer.WriteLine($"{string.Join(" ", values)} # {document.ClassName}{split}");
            }
        }

        public static void Run(string[] trainingVocabulary, string targetTopic, double minimumGain, int? topK)
        {
            LabeledDocument[] documents = LoadDocuments("../Reuters/Reuters_34/Training",
                "output/Reuters_34/Training", true, targetTopic)
                .Concat(LoadDocuments("../Reuters/Reuters_34/Testing",
                    "output/Reuters_34/Testing", false, targetTopic)).ToArray();
            int classCount = documents.Where(document => document.IsTraining)
                .Select(document => document.ClassName).Distinct(StringComparer.Ordinal).Count();
            if (classCount < 2)
                throw new InvalidOperationException($"Topic {targetTopic} must have positive and negative training examples.");

            string outputFolder = $"output/selection/{targetTopic}";
            string originalFile = Path.Combine(outputFolder, "original.txt");
            string scoredFile = Path.Combine(outputFolder, "scored.txt");
            SaveDataset(originalFile, trainingVocabulary.Select(word => new FeatureScore(word, 0)).ToArray(),
                documents, true);
            CalculateGain(originalFile, scoredFile);
            SelectFeatures(scoredFile, outputFolder, minimumGain, topK);
        }

        public static void SelectFeatures(string inputFile, string outputFolder, double minimumGain, int? topK)
        {
            var dataset = ReadDataset(inputFile);
            FeatureScore[] scores = dataset.Features;
            LabeledDocument[] documents = dataset.Documents;
            FeatureScore[] selected = topK.HasValue
                ? InformationGain.SelectTopK(scores, topK.Value)
                : InformationGain.SelectByMinimumGain(scores, minimumGain);
            SaveDataset(Path.Combine(outputFolder, "training.txt"), selected,
                documents.Where(document => document.IsTraining), false);
            SaveDataset(Path.Combine(outputFolder, "testing.txt"), selected,
                documents.Where(document => !document.IsTraining), false);
            File.WriteAllLines(Path.Combine(outputFolder, "vocabulary.txt"),
                selected.Select((score, index) => $"{index + 1}:{score.Word}"));
            Console.WriteLine($"IG: {selected.Length}/{scores.Length} trasaturi selectate. Rezultate: {outputFolder}");
            foreach (FeatureScore score in scores.OrderByDescending(score => score.Gain).Take(10))
                Console.WriteLine($"  {score.Word}: {score.Gain.ToString("F6", CultureInfo.InvariantCulture)}");
            if (selected.Length == 0)
                Console.WriteLine("Pragul nu a pastrat cuvinte. Reduceti --min-gain sau folositi --top-k.");
        }
    }
}
