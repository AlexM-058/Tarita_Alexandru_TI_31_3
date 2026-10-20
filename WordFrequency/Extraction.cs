using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WordFrequency
{
    public class Extraction
    {
        public static IEnumerable<string> ExtractClasses(XElement? metadata)
        {
            if (metadata == null) yield break;

            foreach (var group in metadata.Elements("codes"))
            {
                string? className = (string?)group.Attribute("class");
                if (string.IsNullOrWhiteSpace(className)) continue;

                yield return className;
                foreach (var element in group.Elements("code"))
                {
                    string? code = (string?)element.Attribute("code");
                    if (!string.IsNullOrWhiteSpace(code)) yield return code;
                }
            }
        }

        public static void Extract(string XMLFilePath, string OutputFilePath)
        {
          XDocument doc = XDocument.Load(XMLFilePath);
          var sb = new StringBuilder();
          var content = new List<string>();
          content.Add(doc.Root?.Element("title")?.Value ?? string.Empty);
          XElement? text = doc.Root?.Element("text");
          if (text != null)
          {
            // Keep paragraph boundaries even when XML has no whitespace between tags.
            content.AddRange(text.Nodes().Select(node => node is XElement element
                ? element.Value
                : (node as XText)?.Value ?? string.Empty));
          }
          sb.AppendLine(Regex.Replace(string.Join(" ", content), @"\s+", " ").Trim());
          foreach (string line in ExtractClasses(doc.Root?.Element("metadata")))
              sb.AppendLine(line);
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
