using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CampingInfoCsvToXml {
    internal class Program {
        /// <summary>
        /// Paragraph separator.
        /// </summary>
        private const char PS = '\u2029';

        private const string NewLineWithZeroOrMoreSpaces = "\r\n *";

        private static void Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine("Need 2 args: schema file and csv file");
                return;
            }

            var xmlTemplateFile = new FileInfo(args[0]);
            var csvDataFile = new FileInfo(args[1]);
            var destination = $"xml_{SafeName(xmlTemplateFile)}_{SafeName(csvDataFile)}";
            Directory.CreateDirectory(destination);
            Console.WriteLine($"Using: {xmlTemplateFile}");
            Console.WriteLine($"Data : {csvDataFile}");
            Console.WriteLine($"Dest.: {destination}");

            var converter = new CsvToXmlConverter(xmlTemplateFile);
            var result = converter.Process(csvDataFile.FullName);

            var counter = 1;
            foreach (var cpXml in result) {
                var contents = cpXml.ToString();
                contents = Regex.Replace(contents, NewLineWithZeroOrMoreSpaces, "")
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