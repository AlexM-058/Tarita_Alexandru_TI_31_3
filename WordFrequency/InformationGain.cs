namespace WordFrequency
{
    public record LabeledDocument(string[] Classes, bool IsTraining,
        IReadOnlyDictionary<string, int> Frequencies);

    public record FeatureScore(string Word, double Gain);

    public static class InformationGain
    {
        public static double CalculateEntropy(IEnumerable<int> classCounts)
        {
            int[] counts = classCounts.ToArray();
            if (counts.Any(count => count < 0))
                throw new ArgumentException("Class counts cannot be negative.");
            double total = counts.Sum(count => (double)count);
            if (total == 0) return 0;

            double entropy = 0;
            foreach (int count in counts.Where(count => count > 0))
            {
                double probability = count / total;
                entropy -= probability * Math.Log2(probability);
            }
            return entropy;
        }

        public static FeatureScore[] Calculate(IReadOnlyList<LabeledDocument> documents,
            IReadOnlyList<string> vocabulary)
        {
            LabeledDocument[] training = documents.Where(document => document.IsTraining).ToArray();
            if (training.Length == 0)
                throw new ArgumentException("At least one training document is required.");

            if (training.Any(document => document.Classes.Length == 0))
                throw new ArgumentException("Fiecare document Training trebuie sa aiba cel putin o clasa.");
            string[] classes = training.SelectMany(document => document.Classes)
                .Distinct(StringComparer.Ordinal).ToArray();
            var classIndices = classes.Select((name, index) => (name, index))
                .ToDictionary(entry => entry.name, entry => entry.index, StringComparer.Ordinal);
            var classTotals = new int[classes.Length];
            var presentCounts = vocabulary.ToDictionary(word => word, _ => new int[classes.Length], StringComparer.Ordinal);

            // Numaram o singura data documentele si clasele pentru fiecare cuvant.
            foreach (LabeledDocument document in training)
            {
                int[] documentClasses = document.Classes.Distinct(StringComparer.Ordinal)
                    .Select(name => classIndices[name]).ToArray();
                foreach (int index in documentClasses) classTotals[index]++;
                foreach (var entry in document.Frequencies)
                {
                    if (entry.Value <= 0 || !presentCounts.TryGetValue(entry.Key, out int[]? counts)) continue;
                    foreach (int index in documentClasses) counts[index]++;
                }
            }

            // Esantionul este perechea document-topic. Cu o clasa pe document,
            // numarul perechilor este chiar numarul documentelor, ca in laborator.
            double total = classTotals.Sum(count => (double)count);
            double initialEntropy = CalculateEntropy(classTotals);
            var scores = new List<FeatureScore>();
            foreach (string word in vocabulary)
            {
                double present = presentCounts[word].Sum(count => (double)count);
                double absent = total - present;
                int[] absentCounts = classTotals.Select((total, index) => total - presentCounts[word][index]).ToArray();
                double presentEntropy = CalculateEntropy(presentCounts[word]);
                double absentEntropy = CalculateEntropy(absentCounts);
                double gain = initialEntropy
                    - present / total * presentEntropy
                    - absent / total * absentEntropy;
                scores.Add(new FeatureScore(word, Math.Max(0, gain)));
            }
            return scores.ToArray();
        }

        public static FeatureScore[] SelectByMinimumGain(IEnumerable<FeatureScore> scores, double minimumGain)
        {
            if (!double.IsFinite(minimumGain) || minimumGain < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumGain));
            return scores.Where(score => score.Gain >= minimumGain)
                .OrderBy(score => score.Word, StringComparer.Ordinal).ToArray();
        }

    }
}
