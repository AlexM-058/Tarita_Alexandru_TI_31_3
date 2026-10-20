using System.Text;
using System.Xml.Linq;

namespace WordFrequency
{
    public class Extraction
    {
        public static void Extract(string XMLFilePath, string OutputFilePath)
        {
          XDocument doc = XDocument.Load(XMLFilePath);
          var sb = new StringBuilder();
          var classes = doc.Root?.Element("metadata")?
              .Elements("codes")
              .Select(element => (string?)element.Attribute("class"))
              .OfType<string>()
              .Where(value => !string.IsNullOrWhiteSpace(value))
              ?? Enumerable.Empty<string>();
          sb.AppendLine(string.Join(" ", classes));

          XElement? title = doc.Root?.Element("title");
          if (title != null)
          {
            sb.AppendLine(title.Value);
          }
          foreach (XElement element in doc.Descendants("p"))
          {
            sb.AppendLine(element.Value);
          }
          File.WriteAllText(OutputFilePath, sb.ToString());

        }

        public static void ProcessFolder(string folderXml, string folderTxt)
        {
            Directory.CreateDirectory(folderTxt);

            string[] xmlFiles = Directory.GetFiles(folderXml, "*.xml");
            Console.WriteLine($"{folderXml}: {xmlFiles.Length} fisiere XML.");

            int ok = 0;
            int erori = 0;

            foreach (string caleXml in xmlFiles)
            {
                string nume = Path.GetFileNameWithoutExtension(caleXml);
                string caleTxt = Path.Combine(folderTxt, nume + ".txt");

                try
                {
                    Extract(caleXml, caleTxt);
                    ok++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Eroare la {nume}: {ex.Message}");
                    erori++;
                }
            }

            Console.WriteLine($"  -> {ok} ok, {erori} erori, salvate in {folderTxt}");
        }
    }
}
