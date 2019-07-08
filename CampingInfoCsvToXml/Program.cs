using System;
using System.IO;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;

namespace CampingInfoCsvToXml {
    internal class Program {
        /// <summary>
        /// Paragraph separator.
        /// </summary>
        private const char PS = '\u2029';

        private const string NewLineWithZeroOrMoreSpaces = "\r\n *";
        private const string SpaceBeetweenTags = ">[\r\n ]+<";

        private static void Main(string[] args) {
            var options = new Options();

            var parserResult = Parser.Default.ParseArguments(() => options, args);

            if (parserResult.Tag == ParserResultType.NotParsed) {
                HelpText.RenderUsageText(parserResult);
                return;
            }

            var destination = $"xml_{SafeName(options.XmlTemplateFile)}_{SafeName(options.CsvDataFile)}";
            Directory.CreateDirectory(destination);
            Console.WriteLine($"Template: {options.XmlTemplateFile}");
            Console.WriteLine($"Csv Data: {options.CsvDataFile}");
            Console.WriteLine($"ImageDir: {options.ImagesRootFolder}");
            Console.WriteLine($"  Output: {destination}");

            var converter = new CsvToXmlConverter(options);
            var result = converter.Process();

            var counter = 1;
            foreach (var cpXml in result) {
                var contents = cpXml.ToString();
                contents = //Regex.Replace(contents, NewLineWithZeroOrMoreSpaces, "")
                contents = Regex.Replace(contents, SpaceBeetweenTags, "><")
                    .Replace("</Street><StreetNo>", "</Street> <StreetNo>")
                    .Replace("</ZipCode><Town>", "</ZipCode> <Town>")
                    .Replace("</GeoLatitude><GeoLongitude>", "</GeoLatitude>&#x20;&#x20;<GeoLongitude>")
                    .Replace("><Fkk", ">&#x20;&#x20;<Fkk")
                    .Replace(" lt. Bewertung von ", PS + "lt. Bewertung von" + PS)
                    .Replace("&amp;#x9;", "&#x9;");
                contents = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n" + contents;
                File.WriteAllText(Path.Combine(destination, counter + ".xml"), contents);
                counter++;
            }
        }

        private static string SafeName(FileSystemInfo fileInfo) {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
            return Regex.Replace(fileNameWithoutExtension, "[\\W_]+", "-");
        }
    }
}