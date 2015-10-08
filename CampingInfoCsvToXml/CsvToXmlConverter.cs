﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
            var stopwatchTotal = Stopwatch.StartNew();
            var stopwatch = Stopwatch.StartNew();
            var table = CommonEngine.CsvToDataTable(csvFilePath, ';');
            stopwatch.Stop();
            Console.WriteLine("### importing csv data took: {0}ms", stopwatch.ElapsedMilliseconds);
            var columns = table.Columns;

            foreach (DataRow tableRow in table.Rows) {
                stopwatch = Stopwatch.StartNew();
                if (columns.Contains("Name")) {
                    Console.WriteLine("processing campsite: \"{0}\"", tableRow["Name"]);
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
                        else if (columnName.EndsWith("Value")) {
                            Console.WriteLine(" - value");
                        }
                        else {
                            Console.WriteLine(" - NOT FOUND!");
                        }
                    }
                    else {
                        Console.WriteLine();
                        var text = tableRow[columnName].ToString();
                        string href = null;
                        string value = null;

                        {
                            var valueColumn = columnName + "Value";

                            if (table.Columns.Contains(valueColumn)) {
                                var raw = tableRow[valueColumn].ToString();
                                if (raw.IsImage()) {
                                    href = raw;
                                }
                                else {
                                    value = raw;
                                }
                            }
                        }

                        // special treatment for rating columns
                        /*
                            <RatingAvgCatering>
                                <RatingAvgCateringGraphic href="file://Bilder/Balken_50.ai" />
                                5,0
                            </RatingAvgCatering>
                         */
                        if (columnName.StartsWith("RatingAvg") && columnName != "RatingAvgOverall") {
                            // text is something like balken_39.ai
                            // extract rating value from it
                            var ratingValue = text.Substring(7, 2).Insert(1, ",");
                            href = "file://Bilder/" + text;
                            var graphicNode = columnName + "Graphic";

                            node.SetValue(ratingValue);
                            node.AddFirst(new XElement(XName.Get(graphicNode), new XAttribute(XName.Get("href"), href)));
                        }
                        // verschachtelter Knoten
                        /*
                            <Imbiss>
                                Imbiss am Platz
                                <Imbiss href="file://Bilder/Yes.ai" />
                            </Imbiss>
                         */
                        else if (!string.IsNullOrEmpty(href)) {
                            href = "file://Bilder/" + href;
                            node.SetValue(text);
                            node.Add(new XElement(XName.Get(columnName), new XAttribute(XName.Get("href"), href)));
                        }
                        else {
                            if (text.IsImage()) {
                                // prepend file path
                                text = "file://Bilder/" + text;
                                node.SetAttributeValue(XName.Get("href"), text);
                            }
                            else {
                                // append value after TAB
                                if (!string.IsNullOrEmpty(value)) {
                                    text += "\t" + value;
                                }
                                node.SetValue(text);
                            }
                        }
                    }
                }

                stopwatch.Stop();
                Console.WriteLine("### processing campsite took: {0}ms", stopwatch.ElapsedMilliseconds);

                yield return xDocument;
            }

            stopwatchTotal.Stop();
            Console.WriteLine("### processing all campsites took: {0}ms", stopwatchTotal.ElapsedMilliseconds);
        }
    }

    public static class StringExtensions {
        public static bool IsImage(this string value) {
            return value != null && value.EndsWith(".ai");
        }
    }
}