using System;

namespace CampingInfoCsvToXml {
    internal class Program {
        private static void Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine("Need 2 args: schema file and csv file");
                return;
            }
        }
    }
}