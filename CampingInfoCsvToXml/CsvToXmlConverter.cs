using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using FileHelpers;

namespace CampingInfoCsvToXml {
    public class CsvToXmlConverter {
        private readonly XDocument _xDocument;

        public CsvToXmlConverter(string xmlTemplate) {
            _xDocument = XDocument.Parse(xmlTemplate);
        }

        public CsvToXmlConverter(FileInfo xmlTemplateFile) {
            using (var stream = File.OpenRead(xmlTemplateFile.ToString())) {
                _xDocument = XDocument.Load(stream);
            }
        }

        private XDocument GetFreshDocument() {
            using (var stream = new MemoryStream()) {
                _xDocument.Save(stream);
                stream.Position = 0;
                return XDocument.Load(stream);
            }
        }

        public IEnumerable<XDocument> Process(string csvFilePath) {
            var table = CommonEngine.CsvToDataTable(csvFilePath, ';');
            var columns = table.Columns;

            foreach (DataRow tableRow in table.Rows) {
                if (columns.Contains("Name")) {
                    Console.Write("processing campsite: \"{0}\"", tableRow["Name"]);
                }
                var xDocument = GetFreshDocument();

                foreach (var column in columns) {
                    var columnName = column.ToString();
                    Console.Write("processing column: {0}", columnName);
                    var node = xDocument.XPathSelectElement(".//" + columnName);
                    if (node == null) {
                        if (columnName.EndsWith("Href")) {
                            Console.WriteLine(" - href");
                        }
                        else {
                            Console.WriteLine(" - NOT FOUND!");
                        }
                    }
                    else {
                        var value = tableRow[columnName].ToString();
                        string href = null;
                        if (table.Columns.Contains(columnName + "Href")) {
                            href = tableRow[columnName + "Href"].ToString();
                        }
                        if (!string.IsNullOrEmpty(href)) {
                            node.SetValue(value);
                            node.Add(new XElement(XName.Get(columnName), new XAttribute(XName.Get("href"), href)));
                        }
                        else {
                            if (value.EndsWith(".ai")) {
                                node.SetAttributeValue(XName.Get("href"), value);
                            }
                            else {
                                node.Value = value;
                            }
                        }
                        Console.WriteLine();
                    }
                }

                yield return xDocument;
            }
        }
    }
}