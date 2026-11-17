using WordFrequency;

// Pragul de selectie; 0.5 este valoarea folosita in exemplul laboratorului.
double minimumGain = 0.01;
string[] sets = ["Reuters_34/Training", "Reuters_34/Testing", "Reuters_7083"];

foreach (string set in sets)
{
    string folder = $"output/{set}";
    Extraction.ProcessFolder($"../Reuters/{set}", folder);
    TextEdit.ProcessFolder(folder);
    string[] vocabulary = GlobalVocabulary.CreateGlobalVector(folder);
    LocalVectors.SaveLocalVectors(folder, $"output/vectors/local/{set}");
    RareVectors.SaveRareVectors(folder, $"output/vectors/rari/{set}", vocabulary);
    string name = set.Replace('/', '_');
    GlobalVocabulary.SaveGlobalVector(vocabulary, $"output/vectors/global/{name}_global.txt");
}

FeatureSelection.Run(minimumGain);

double reuters34Entropy = InformationGain.CalculateEntropy(
    VectorMetadata.CountTopics("output/Reuters_34/Training", "output/Reuters_34/Testing").Values);
double archiveEntropy = InformationGain.CalculateEntropy(
    VectorMetadata.CountTopics("output/Reuters_7083").Values);

Console.WriteLine(FormattableString.Invariant($"Entropie Reuters_34 (distributia codurilor): {reuters34Entropy:F6} biti"));
Console.WriteLine(FormattableString.Invariant($"Entropie Reuters_7083 (distributia codurilor): {archiveEntropy:F6} biti"));
