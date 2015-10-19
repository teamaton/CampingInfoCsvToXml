using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CampingInfoCsvToXml {
    internal class Program {
        private static void Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine("Need 2 args: schema file and csv file");
                return;
            }

            var xmlTemplateFile = new FileInfo(args[0]);
            Console.WriteLine("Using: {0}", xmlTemplateFile);
            Console.WriteLine("Data : {0}", new FileInfo(args[1]));
            var converter = new CsvToXmlConverter(xmlTemplateFile);
            var result = converter.Process(args[1]);

            var counter = 1;
            foreach (var cpXml in result) {
                var contents = cpXml.ToString();
                var newLineWithZeroOrMoreSpaces = "\r\n *";
                contents = Regex.Replace(contents, newLineWithZeroOrMoreSpaces, "")
                    .Replace("</Street><StreetNo>", "</Street> <StreetNo>")
                    .Replace("</ZipCode><Town>", "</ZipCode> <Town>")
                    .Replace("</GeoLatitude><GeoLongitude>", "</GeoLatitude>&#x20;&#x20;<GeoLongitude>")
                    .Replace("><Fkk", ">&#x20;&#x20;<Fkk")
                    .Replace(" lt. Bewertung von ", "\u2029lt. Bewertung von\u2029")
                    .Replace("&amp;#x9;", "&#x9;");
                contents = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + contents;
                File.WriteAllText(counter + ".xml", contents);
                counter++;
            }
        }
    }
}