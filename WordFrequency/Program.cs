using WordFrequency;
using System.Globalization;

// Etapele pot fi rulate si separat, fara reprocesarea XML-urilor.
if (args.Length > 0 && args[0] == "--gain")
{
    if (args.Length != 3) throw new ArgumentException("--gain intrare.txt iesire.txt");
    FeatureSelection.CalculateGain(args[1], args[2]);
    Console.WriteLine($"Gain salvat in {args[2]}.");
    return;
}
string? selectionInput = null;
string? selectionOutput = null;
if (args.Length > 0 && args[0] == "--select")
{
    if (args.Length < 3) throw new ArgumentException("--select scored.txt folder --top-k 100");
    selectionInput = args[1];
    selectionOutput = args[2];
    args = args.Skip(3).ToArray();
}

string targetTopic = "C22";
double minimumGain = 0.5;
int? topK = null;
if (args.Length % 2 != 0)
    throw new ArgumentException("Optiuni: --topic C22 --min-gain 0.5 sau --topic C22 --top-k 100");
for (int index = 0; index < args.Length; index += 2)
{
    string value = args[index + 1];
    switch (args[index])
    {
        case "--topic":
            if (value.Length == 0 || value.Any(character => !char.IsAsciiLetterOrDigit(character)))
                throw new ArgumentException("Topic must contain only ASCII letters and digits.");
            targetTopic = value;
            break;
        case "--min-gain":
            minimumGain = double.Parse(value, CultureInfo.InvariantCulture);
            if (!double.IsFinite(minimumGain) || minimumGain < 0)
                throw new ArgumentException("Minimum gain must be finite and nonnegative.");
            break;
        case "--top-k":
            topK = int.Parse(value, CultureInfo.InvariantCulture);
            if (topK <= 0) throw new ArgumentException("Top K must be positive.");
            break;
        default: throw new ArgumentException($"Unknown option: {args[index]}");
    }
}

if (selectionInput != null)
{
    FeatureSelection.SelectFeatures(selectionInput, selectionOutput!, minimumGain, topK);
    return;
}

Extraction.ProcessFolder("../Reuters/Reuters_34/Training", "output/Reuters_34/Training");
TextEdit.ProcessFolder("output/Reuters_34/Training");
Extraction.ProcessFolder("../Reuters/Reuters_34/Testing", "output/Reuters_34/Testing");
TextEdit.ProcessFolder("output/Reuters_34/Testing");
Extraction.ProcessFolder("../Reuters/Reuters_7083", "output/Reuters_7083");
TextEdit.ProcessFolder("output/Reuters_7083");

LocalVectors.SaveLocalVectors("output/Reuters_34/Training", "output/vectors/local/Reuters_34/Training");
LocalVectors.SaveLocalVectors("output/Reuters_34/Testing", "output/vectors/local/Reuters_34/Testing");
LocalVectors.SaveLocalVectors("output/Reuters_7083", "output/vectors/local/Reuters_7083");

string[] trainingVocabulary = GlobalVocabulary.CreateGlobalVector("output/Reuters_34/Training");
string[] reutersVocabulary = GlobalVocabulary.CreateGlobalVector("output/Reuters_7083");

GlobalVocabulary.SaveGlobalVector(trainingVocabulary, "output/vectors/global/Reuters_34_Training_global.txt");
GlobalVocabulary.SaveGlobalVector(trainingVocabulary, "output/vectors/global/Reuters_34_Testing_global.txt");
GlobalVocabulary.SaveGlobalVector(reutersVocabulary, "output/vectors/global/Reuters_7083_global.txt");

RareVectors.SaveRareVectors("output/Reuters_34/Training", "output/vectors/rari/Reuters_34/Training", trainingVocabulary);
RareVectors.SaveRareVectors("output/Reuters_34/Testing", "output/vectors/rari/Reuters_34/Testing", trainingVocabulary);
RareVectors.SaveRareVectors("output/Reuters_7083", "output/vectors/rari/Reuters_7083", reutersVocabulary);

FeatureSelection.Run(trainingVocabulary, targetTopic, minimumGain, topK);

Console.WriteLine("Procesare finalizata.");
// nu uitam sa folosim si sa abordam contractiile din limba engleza exista un fisier cu toate contractiile si forma lor extinsa, trebuie sa facem un replace inainte de a face tokenizarea.
