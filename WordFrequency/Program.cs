using WordFrequency;

Extraction.ProcessFolder("../Reuters/Reuters_34/Training", "output/Reuters_34/Training");
TextEdit.ProcessFolder("output/Reuters_34/Training");
Extraction.ProcessFolder("../Reuters/Reuters_34/Testing", "output/Reuters_34/Testing");
TextEdit.ProcessFolder("output/Reuters_34/Testing");
Extraction.ProcessFolder("../Reuters/Reuters_7083", "output/Reuters_7083");
TextEdit.ProcessFolder("output/Reuters_7083");

FeatureExtraction.SaveLocalVectors("output/Reuters_34/Training", "output/vectors/Reuters_34_Training.txt");
FeatureExtraction.SaveLocalVectors("output/Reuters_34/Testing", "output/vectors/Reuters_34_Testing.txt");
FeatureExtraction.SaveLocalVectors("output/Reuters_7083", "output/vectors/Reuters_7083.txt");

FeatureExtraction.SaveGlobalVector("output/Reuters_34/Training", "output/vectors/Reuters_34_Training_global.txt");
FeatureExtraction.SaveGlobalVector("output/Reuters_34/Testing", "output/vectors/Reuters_34_Testing_global.txt");
FeatureExtraction.SaveGlobalVector("output/Reuters_7083", "output/vectors/Reuters_7083_global.txt");

Console.WriteLine("Procesare finalizata.");
// nu uitam sa folosim si sa abordam contractiile din limba engleza exista un fisier cu toate contractiile si forma lor extinsa, trebuie sa facem un replace inainte de a face tokenizarea.
