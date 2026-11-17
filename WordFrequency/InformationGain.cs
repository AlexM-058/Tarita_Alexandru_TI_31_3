namespace WordFrequency
{
    public record LabeledDocument(string ClassName, bool IsTraining,
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

            double initialEntropy = CalculateEntropy(training.GroupBy(document => document.ClassName)
                .Select(group => group.Count()));
            var scores = new List<FeatureScore>();
            foreach (string word in vocabulary)
            {
                var present = new List<LabeledDocument>();
                var absent = new List<LabeledDocument>();
                foreach (LabeledDocument document in training)
                {
                    // O aparitie sau zece aparitii inseamna acelasi lucru: prezent.
                    if (document.Frequencies.TryGetValue(word, out int count) && count > 0)
                        present.Add(document);
                    else
                        absent.Add(document);
                }

                double presentEntropy = CalculateEntropy(present.GroupBy(document => document.ClassName)
                    .Select(group => group.Count()));
                double absentEntropy = CalculateEntropy(absent.GroupBy(document => document.ClassName)
                    .Select(group => group.Count()));
                double gain = initialEntropy
                    - (double)present.Count / training.Length * presentEntropy
                    - (double)absent.Count / training.Length * absentEntropy;
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

        public static FeatureScore[] SelectTopK(IEnumerable<FeatureScore> scores, int count)
        {
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            return scores.OrderByDescending(score => score.Gain)
                .ThenBy(score => score.Word, StringComparer.Ordinal).Take(count)
                .OrderBy(score => score.Word, StringComparer.Ordinal).ToArray();
        }
    }
}
