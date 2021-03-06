﻿using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CampingInfoCsvToXml {
    [TestFixture]
    public class CsvToXmlTest {
        private Options _options;
        private static readonly string Tab = CsvToXmlConverter.XmlTabCode.Replace("&", "&amp;");

        [SetUp]
        public void SetUp() {
            _options = new Options
                {
                XmlTemplateFile = new FileInfo("tmp.xml"),
                CsvDataFile = new FileInfo("tmp.csv"),
                ImagesRootFolder = new Uri("file:///c:/"),
                FolderColumn = "Pfad"
                };
        }

        [Test]
        public void Convert_image_column_value_to_href_attribute_in_xml() {
            var csv = "Spalte" + Environment.NewLine + "Bilder-Layout/Wert.ai";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
<cell><Spalte /></cell>
</Root>";
            File.WriteAllText("tmp.csv", csv);
            File.WriteAllText("tmp.xml", xml);
            var xmlResult = new CsvToXmlConverter(_options).Process().First().ToString();
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining("<Spalte href=\"file:///c:/Bilder-Layout/Wert.ai\" />"));
        }

        [Test]
        public void Convert_simple_column_value_to_text_node_in_xml() {
            var csv = "Spalte" + Environment.NewLine + "Wert";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
<cell><Spalte /></cell>
</Root>";
            File.WriteAllText("tmp.csv", csv);
            File.WriteAllText("tmp.xml", xml);
            var xmlResult = new CsvToXmlConverter(_options).Process().First().ToString();
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining("<Spalte>Wert</Spalte>"));
        }

        [Test]
        public void Convert_columns_with_value_and_href_to_correct_xml_nodes() {
            var csv = "Spalte;NochEine" + Environment.NewLine + "Wert;Bilder-Layout/bild.ai";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
<cell><Spalte /><NochEine /></cell>
</Root>";
            File.WriteAllText("tmp.csv", csv);
            File.WriteAllText("tmp.xml", xml);
            var xmlResult = new CsvToXmlConverter(_options).Process().First().ToString();
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining("<Spalte>Wert</Spalte>"));
            Assert.That(xmlResult, Is.StringContaining("<NochEine href=\"file:///c:/Bilder-Layout/bild.ai\" />"));
        }

        [Test]
        public void Convert_column_to_text_node_from_xml_file() {
            var csv = "Spalte" + Environment.NewLine + "Wert";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
<cell><Spalte /></cell>
</Root>";
            File.WriteAllText("tmp.csv", csv);
            File.WriteAllText("tmp.xml", xml);
            var xmlResult = new CsvToXmlConverter(_options).Process().First().ToString();
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining("<Spalte>Wert</Spalte>"));
        }

        [Test]
        public void Convert_column_and_image_neighbor_to_nested_node_with_value_and_href() {
            var csv = "Lebensmittelversorgung;LebensmittelversorgungValue" + Environment.NewLine +
                      "Lebensmittel am Platz;/Bilder-Layout/Yes.ai";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
  <cell>
    <Lebensmittelversorgung>
      Lebensmittel am Platz
      <Lebensmittelversorgung href=""file:///c:/some-other-path/Yes.ai"" />
    </Lebensmittelversorgung>
  </cell>
</Root>";
            File.WriteAllText("tmp.csv", csv);
            File.WriteAllText("tmp.xml", xml);
            var xmlResult = new CsvToXmlConverter(_options).Process().First().ToString();
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining(
                $@"<Lebensmittelversorgung>Lebensmittel am Platz{Tab}<Lebensmittelversorgung " +
                @"href=""file:///c:/Bilder-Layout/Yes.ai"" /></Lebensmittelversorgung>"));
        }

        [Test]
        public void Convert_column_and_value_neighbor_to_nested_node_with_only_value() {
            var csv = "Lebensmittelversorgung;LebensmittelversorgungValue" + Environment.NewLine +
                      "Lebensmittel am Platz;200 m";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
  <cell>
    <Lebensmittelversorgung></Lebensmittelversorgung>
  </cell>
</Root>";
            File.WriteAllText("tmp.csv", csv);
            File.WriteAllText("tmp.xml", xml);
            var xmlResult = new CsvToXmlConverter(_options).Process().First().ToString();
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining(
                $@"<Lebensmittelversorgung>Lebensmittel am Platz{Tab}200 m</Lebensmittelversorgung>"));
        }

        [Test]
        public void Convert_column_and_neighbor_to_node_with_tabbed_text() {
            var csv = "Activity1;Activity1Value" + Environment.NewLine +
                      "Schwimmen;55 %";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
  <cell>
    <Activity1></Activity1>
  </cell>
</Root>";
            File.WriteAllText("tmp.csv", csv);
            File.WriteAllText("tmp.xml", xml);
            var xmlResult = new CsvToXmlConverter(_options).Process().First().ToString();
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining($"<Activity1>Schwimmen{Tab}55 %</Activity1>"));
        }

        [Test]
        public void Convert_rating_column_to_nested_node_with_graphic_and_value() {
            var csv = "RatingAvgSth" + Environment.NewLine +
                      "Bilder-Layout/balken_43.ai";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
  <cell>
    <RatingAvgSth></RatingAvgSth>
  </cell>
</Root>";
            File.WriteAllText("tmp.csv", csv);
            File.WriteAllText("tmp.xml", xml);
            var xmlResult = new CsvToXmlConverter(_options).Process().First().ToString();
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining(
                $@"<RatingAvgSthGraphic href=""file:///c:/Bilder-Layout/balken_43.ai"" />{Tab}4,3</RatingAvgSth>"));
        }

        [Test]
        public void Convert_column_with_text_and_empty_value_column_to_node_with_text() {
            var csv = "Stars;StarsValue" + Environment.NewLine +
                      "noch keine;";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
  <cell>
    <Stars></Stars>
  </cell>
</Root>";
            File.WriteAllText("tmp.csv", csv);
            File.WriteAllText("tmp.xml", xml);
            var xmlResult = new CsvToXmlConverter(_options).Process().First().ToString();
            Console.WriteLine(xmlResult);
            Assert.That(xmlResult, Is.StringContaining("<Stars>noch keine</Stars>"));
        }

        [Test]
        public void Convert_text_with_newlines_to_node_with_text_with_newlines() {
            var csv = "Id;Description" + Environment.NewLine +
                      @"2;""Wunderschön!

Was soll ich sagen?

Einfach toll!!!""";
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
  <cell>
    <Description />
  </cell>
</Root>";
            File.WriteAllText("tmp.csv", csv);
            File.WriteAllText("tmp.xml", xml);
            var xmlResult = new CsvToXmlConverter(_options).Process().First();
            Console.WriteLine(xmlResult);
            var expected = @"<Description>Wunderschön!

Was soll ich sagen?

Einfach toll!!!</Description>";
            Assert.That(xmlResult.ToString(), Is.StringContaining(expected));
            xmlResult.Save("result.xml");
        }
    }
}