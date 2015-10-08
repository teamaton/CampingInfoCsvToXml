using System;
using System.IO;
using System.Text;

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
                File.WriteAllText(counter + ".xml", cpXml.ToString(), Encoding.UTF8);
                counter++;
            }
        }
    }
}