using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using FileHelpers;
using NUnit.Framework;

namespace CampingInfoCsvToXml {
    [TestFixture]
    public class CsvToXmlTest {
        [Test]
        public void convert_url_column_value_to_href_attribute_on_node_in_xml() {
            var csv = "Spalte" + Environment.NewLine + "Wert.ai";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
<cell><Spalte /></cell>
</Root>";
            var xmlResult = Apply(csv, xml);
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining("<Spalte href=\"Wert.ai\" />"));
        }

        [Test]
        public void convert_simple_column_value_to_text_node_in_xml() {
            var csv = "Spalte" + Environment.NewLine + "Wert";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
<cell><Spalte /></cell>
</Root>";
            var xmlResult = Apply(csv, xml);
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining("<Spalte>Wert</Spalte>"));
        }

        [Test]
        public void convert_columns_with_value_and_href_to_correct_xml_nodes() {
            var csv = "Spalte;NochEine" + Environment.NewLine + "Wert;bild.ai";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
<cell><Spalte /><NochEine /></cell>
</Root>";
            var xmlResult = Apply(csv, xml);
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining("<Spalte>Wert</Spalte>"));
            Assert.That(xmlResult, Is.StringContaining("<NochEine href=\"bild.ai\" />"));
        }

        private string Apply(string csv, string xml) {
            File.WriteAllText("tmp.csv", csv);
            var table = CommonEngine.CsvToDataTable("tmp.csv", ';');
            var columns = table.Columns;

            var xDocument = XDocument.Parse(xml);
            Console.WriteLine("Elements:");
            Console.WriteLine(string.Join(Environment.NewLine, xDocument.Elements().Select(elm => elm.Name)));
            Console.WriteLine("---------");

            foreach (var column in columns) {
                var columnName = column.ToString();
                var node = xDocument.XPathSelectElement(".//" + columnName);
                var value = table.Rows[0][columnName].ToString();

                if (value.EndsWith(".ai")) {
                    node.SetAttributeValue(XName.Get("href"), value);
                }
                else {
                    node.Value = value;
                }
            }

            return xDocument.ToString();
        }
    }
}