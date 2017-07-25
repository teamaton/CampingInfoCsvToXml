using System;
using System.IO;
using CommandLine;

namespace CampingInfoCsvToXml {
    public class Options {
        [Option('t', "template", Required = true,
            HelpText = "The XML file to be used as the template for the data.")]
        public FileInfo XmlTemplateFile { get; set; }

        [Option('d', "data", Required = true,
            HelpText = "The CSV file containing the data to be processed with the template.")]
        public FileInfo CsvDataFile { get; set; }

        [Option('i', "imagesroot", Required = true,
            HelpText = "The folder that serves as the root for all included images, formatted as a file:/// url.")]
        public Uri ImagesRootFolder { get; set; }

        [Option('c', "foldercolumn", Default = "Pfad",
            HelpText =
                "The name of the column in the CSV file that contains a relative path for Campsite specific pictures.")]
        public string CampsiteFolderColumn { get; set; }
    }
}