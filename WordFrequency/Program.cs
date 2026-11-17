using WordFrequency;

// Pastram cuvintele cu gain >= acest prag, separat pentru fiecare set.
double minimumGain = 0.01;

Extraction.ProcessFolder("../Reuters/Reuters_34/Training", "output/Reuters_34/Training");
TextEdit.ProcessFolder("output/Reuters_34/Training");
Extraction.ProcessFolder("../Reuters/Reuters_34/Testing", "output/Reuters_34/Testing");
TextEdit.ProcessFolder("output/Reuters_34/Testing");
Extraction.ProcessFolder("../Reuters/Reuters_7083", "output/Reuters_7083");
TextEdit.ProcessFolder("output/Reuters_7083");

string[] sets = ["Reuters_34/Training", "Reuters_34/Testing", "Reuters_7083"];
foreach (string set in sets)
{
    string folder = $"output/{set}";
    string[] vocabulary = GlobalVocabulary.CreateGlobalVector(folder);
    LocalVectors.SaveLocalVectors(folder, $"output/vectors/local/{set}");
    RareVectors.SaveRareVectors(folder, $"output/vectors/rari/{set}", vocabulary);
    string name = set.Replace('/', '_');
    GlobalVocabulary.SaveGlobalVector(vocabulary, $"output/vectors/global/{name}_global.txt");
}

FeatureSelection.Run(minimumGain);
Console.WriteLine("Procesare finalizata. Rezultatele sunt in output/selection.");
