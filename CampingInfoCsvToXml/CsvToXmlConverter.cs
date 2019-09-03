using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using FileHelpers;

namespace CampingInfoCsvToXml {
    public class CsvToXmlConverter {
        internal const string XmlTabCode = "&#x9;";
        private readonly Options _options;

        private string ImagePathPrefix => _options.ImagesRootFolder.ToString().TrimEnd('/');

        private string FolderColumn => _options.FolderColumn;

        private readonly XDocument _xDocument;

        public CsvToXmlConverter(Options options) {
            _options = options;
            _xDocument = XDocument.Parse(File.ReadAllText(options.XmlTemplateFile.FullName));
        }

        private XDocument GetFreshDocument() {
            using (var stream = new MemoryStream()) {
                _xDocument.Save(stream);
                stream.Position = 0;
                return XDocument.Load(stream);
            }
        }

        public IEnumerable<XDocument> Process() {
            var stopwatchTotal = Stopwatch.StartNew();
            var stopwatch = Stopwatch.StartNew();

            var csvFilePath = _options.CsvDataFile.FullName;
            EnsureUtf8Bom(csvFilePath);

            var dataTable = CommonEngine.CsvToDataTable(csvFilePath, ';');
            stopwatch.Stop();
            Console.WriteLine($"### importing csv data took: {stopwatch.ElapsedMilliseconds}ms");
            var columns = dataTable.Columns;

            foreach (DataRow tableRow in dataTable.Rows) {
                stopwatch = Stopwatch.StartNew();
                if (columns.Contains("Name")) {
                    Console.WriteLine($"processing row: \"{tableRow["Name"]}\"");
                }
                var xDocument = GetFreshDocument();

                var folder = columns.Contains(FolderColumn)
                    ? tableRow[FolderColumn].ToString()
                    : null;

                foreach (var column in columns) {
                    var columnName = column.ToString();
                    Console.Write($"processing column: {columnName}");
                    var node = xDocument.XPathSelectElement(".//" + columnName);
                    if (node == null) {
                        if (columnName.EndsWith("Href")) {
                            Console.WriteLine(" - href");
                        }
                        else if (columnName.EndsWith("Value")) {
                            Console.WriteLine(" - value");
                        }
                        else if (columnName == FolderColumn) {
                            Console.WriteLine(" - Bilderordner");
                        }
                        else if (columnName == "Premium") {
                            Console.WriteLine(" - Premium");
                        }
                        else {
                            Console.WriteLine(" - NOT FOUND!");
                        }
                    }
                    else {
                        Console.WriteLine();

                        // remove node content and attributes from example file
                        node.RemoveAll();

                        var text = tableRow[columnName].ToString();
                        string href = null;
                        string value = null;
                        var valueColumn = columnName + "Value";

                        if (dataTable.Columns.Contains(valueColumn)) {
                            var raw = tableRow[valueColumn].ToString();
                            if (raw.IsImage()) {
                                href = raw;
                            }
                            else {
                                value = raw;
                            }
                        }

                        // special treatment for rating columns
                        /*
                            <RatingAvgCatering>
                                <RatingAvgCateringGraphic href="file://Bilder/Balken_50.ai" />
                                5,0
                            </RatingAvgCatering>
                         */
                        if (columnName.StartsWith("RatingAvg") && columnName != "RatingAvgOverall" && text.IsImage()) {
                            // text is something like folder/balken_39.ai
                            // extract rating value from it
                            var idxOfUnderscore = text.LastIndexOf('_');
                            var ratingValue = text.Substring(idxOfUnderscore + 1, 2).Insert(1, ",");
                            href = GetFullPath(text, folder);
                            var graphicNode = columnName + "Graphic";

                            node.SetValue(XmlTabCode + ratingValue);
                            node.AddFirst(new XElement(XName.Get(graphicNode),
                                new XAttribute(XName.Get("href"), href)));
                        }
                        // Besonderheit: 1 o. 2 Kindknoten im verschachtelten Knoten
                        /*
                            <SwimmingPoolOutdoor>
                                Pool / Hallenbad&#x9;
                                <SwimmingPoolOutdoor href="No.ai" />
                                / <SwimmingPoolOutdoor href="Yes.ai" />
                            </SwimmingPoolOutdoor>
                         */
                        else if (columnName == "SwimmingPoolOutdoor" || columnName == "Ski" ||
                                 columnName == "Restaurant") {
                            // Pool (&|/) Hallenbad
                            text += XmlTabCode;
                            node.SetValue(text);
                            var values =
                                tableRow[columnName + "Value"].ToString().Split('/').Select(s => s.Trim()).ToArray();
                            for (var i = 0; i < values.Length; i++) {
                                if (i > 0) {
                                    node.Add(" / ");
                                }
                                value = values[i];
                                if (value.IsImage()) {
                                    value = GetFullPath(value, folder);
                                    node.Add(new XElement(XName.Get(columnName),
                                        new XAttribute(XName.Get("href"), value)));
                                }
                                else {
                                    node.Add(value);
                                }
                            }
                        }
                        // verschachtelter Knoten
                        /*
                            <Imbiss>
                                Imbiss am Platz
                                <Imbiss href="file://Bilder/Yes.ai" />
                            </Imbiss>
                         */
                        else if (!string.IsNullOrEmpty(href)) {
                            // append TAB after text for Yes/No img
                            text += href.EndsWith("Yes.ai") || href.EndsWith("No.ai") ? XmlTabCode : "";
                            href = GetFullPath(href, folder);
                            node.SetValue(text);
                            node.Add(new XElement(XName.Get(columnName), new XAttribute(XName.Get("href"), href)));
                        }
                        else {
                            if (text.IsImage()) {
                                // prepend file path
                                text = GetFullPath(text, folder);
                                node.SetAttributeValue(XName.Get("href"), text);
                            }
                            else {
                                // append value after TAB
                                if (!string.IsNullOrEmpty(value)) {
                                    text += XmlTabCode + value;
                                }

                                // the last default
                                node.SetValue(text);
                            }
                        }
                    }
                }

                stopwatch.Stop();
                Console.WriteLine($"### processing row took: {stopwatch.ElapsedMilliseconds}ms");

                yield return xDocument;
            }

            stopwatchTotal.Stop();
            Console.WriteLine($"### processing all rows took: {stopwatchTotal.ElapsedMilliseconds}ms");
        }

        private string GetFullPath(string value, string folder) {
            var paths = new[] { ImagePathPrefix, folder, value.TrimStart('/') };
            return string.Join("/", paths.Where(p => !string.IsNullOrEmpty(p)));
        }

        private static void EnsureUtf8Bom(string csvFilePath) {
            using (var tempStream = File.Open(csvFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                var buffer = new byte[3];
                var readCount = tempStream.Read(buffer, 0, 3);
                tempStream.Close();
                // UTF-8 BOM = 0xEF, 0xBB, 0xBF
                if (readCount < 3 || buffer[0] != 0xEF || buffer[1] != 0xBB || buffer[2] != 0xBF) {
                    Console.WriteLine("~~~~~~~~~");
                    Console.WriteLine("Added BOM");
                    Console.WriteLine("~~~~~~~~~");
                    var contents = File.ReadAllText(csvFilePath);
                    File.WriteAllBytes(csvFilePath, new byte[] { 0xEF, 0xBB, 0xBF });
                    File.AppendAllText(csvFilePath, contents);
                }
            }
        }
    }

    public static class StringExtensions {
        private static readonly string[] KnownImageFileExtensions =
            { ".ai", ".eps", ".jpeg", ".jpg", ".pdf", ".png", ".psd", ".tif", ".tiff" };

        public static bool IsImage(this string value) {
            return KnownImageFileExtensions.Any(ext => value.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }
    }
}