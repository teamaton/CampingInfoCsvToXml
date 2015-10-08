using System;
using System.IO;
using System.Linq;
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
            File.WriteAllText("tmp.csv", csv);
            var fileName = new FileInfo("tmp.csv").ToString();
            var xmlResult = new CsvToXmlConverter(xml).Process(fileName).First().ToString();
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
            File.WriteAllText("tmp.csv", csv);
            var fileName = new FileInfo("tmp.csv").ToString();
            var xmlResult = new CsvToXmlConverter(xml).Process(fileName).First().ToString();
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
            File.WriteAllText("tmp.csv", csv);
            var fileName = new FileInfo("tmp.csv").ToString();
            var xmlResult = new CsvToXmlConverter(xml).Process(fileName).First().ToString();
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining("<Spalte>Wert</Spalte>"));
            Assert.That(xmlResult, Is.StringContaining("<NochEine href=\"bild.ai\" />"));
        }

        [Test]
        public void convert_column_to_text_node_from_xml_file() {
            var csv = "Spalte" + Environment.NewLine + "Wert";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
<cell><Spalte /></cell>
</Root>";
            File.WriteAllText("tmp.csv", csv);
            File.WriteAllText("tmp.xml", xml);
            var fileName = new FileInfo("tmp.csv").ToString();
            var xmlResult = new CsvToXmlConverter(new FileInfo("tmp.xml")).Process(fileName).First().ToString();
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining("<Spalte>Wert</Spalte>"));
        }

        [Test]
        public void convert_column_and_neighbor_to_nested_node_with_value_and_href() {
            var csv = "Lebensmittelversorgung;LebensmittelversorgungHref" + Environment.NewLine +
                      "Lebensmittel am Platz;file://Bilder/Yes.ai";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
  <cell>
    <Lebensmittelversorgung>
      Lebensmittel am Platz
      <Lebensmittelversorgung href=""file://Bilder/Yes.ai"" />
    </Lebensmittelversorgung>
  </cell>
</Root>";
            File.WriteAllText("tmp.csv", csv);
            File.WriteAllText("tmp.xml", xml);
            var fileName = new FileInfo("tmp.csv").ToString();
            var xmlResult = new CsvToXmlConverter(new FileInfo("tmp.xml")).Process(fileName).First().ToString();
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining(
                @"<Lebensmittelversorgung>Lebensmittel am Platz<Lebensmittelversorgung href=""file://Bilder/Yes.ai"" /></Lebensmittelversorgung>"));
        }
    }
}