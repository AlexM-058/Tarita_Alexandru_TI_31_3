using WordFrequency;

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
string[] testingVocabulary = GlobalVocabulary.CreateGlobalVector("output/Reuters_34/Testing");
string[] reutersVocabulary = GlobalVocabulary.CreateGlobalVector("output/Reuters_7083");

GlobalVocabulary.SaveGlobalVector(trainingVocabulary, "output/vectors/global/Reuters_34_Training_global.txt");
GlobalVocabulary.SaveGlobalVector(testingVocabulary, "output/vectors/global/Reuters_34_Testing_global.txt");
GlobalVocabulary.SaveGlobalVector(reutersVocabulary, "output/vectors/global/Reuters_7083_global.txt");

RareVectors.SaveRareVectors("output/Reuters_34/Training", "output/vectors/rari/Reuters_34/Training", trainingVocabulary);
RareVectors.SaveRareVectors("output/Reuters_34/Testing", "output/vectors/rari/Reuters_34/Testing", testingVocabulary);
RareVectors.SaveRareVectors("output/Reuters_7083", "output/vectors/rari/Reuters_7083", reutersVocabulary);

Console.WriteLine("Procesare finalizata.");
// nu uitam sa folosim si sa abordam contractiile din limba engleza exista un fisier cu toate contractiile si forma lor extinsa, trebuie sa facem un replace inainte de a face tokenizarea.
