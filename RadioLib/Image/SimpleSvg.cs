using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RadioLib.Image
{
    [XmlRoot("svg")]
    public class SimpleSvg
    {
        [XmlAttribute("geoViewBox", Namespace = "http://mapsvg.com")]
        public string? ViewBox { get; set; }

        [XmlAttribute("width")]
        public double Width { get; set; }

        [XmlAttribute("height")]
        public double Height { get; set; }

        [XmlElement("path")]
        public List<SimplePath> Paths { get; set; }

        public static SimpleSvg Load(string path)
        {
            XmlSerializer ser = new XmlSerializer(typeof(SimpleSvg), "http://www.w3.org/2000/svg");

            SimpleSvg? svg;
            using (XmlReader reader = XmlReader.Create(path))
            {
                svg = (SimpleSvg?)ser.Deserialize(reader);
            }

            if (svg == null)
                throw new Exception("Invalud svg file.");

            return svg;
        }

        public static SimpleSvg LoadFromText(string xml)
        {
            XmlSerializer ser = new XmlSerializer(typeof(SimpleSvg), "http://www.w3.org/2000/svg");

            SimpleSvg? svg;
            using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
            {
                svg = (SimpleSvg?)ser.Deserialize(reader);
            }

            if (svg == null)
                throw new Exception("Invalud svg file.");

            return svg;
        }

        public static SimpleSvg Load(Stream stream)
        {
            XmlSerializer ser = new XmlSerializer(typeof(SimpleSvg), "http://www.w3.org/2000/svg");

            SimpleSvg? svg;
            using (XmlReader reader = XmlReader.Create(stream))
            {
                svg = (SimpleSvg?)ser.Deserialize(reader);
            }

            if (svg == null)
                throw new Exception("Invalud svg file.");

            return svg;
        }
    }

    [XmlRoot("path")]
    public class SimplePath
    {
        [XmlAttribute("d")]
        public string? Path { get; set; }

        [XmlAttribute("title")]
        public string? Title { get; set; }

        [XmlAttribute("id")]
        public string? Id { get; set; }

        [XmlIgnore]
        public bool IsValid => !string.IsNullOrEmpty(Path);

        


        public override string ToString()
        {
            string isvalid = IsValid ? "Valid" : "Invalid";
            return $"Path [{Id}]:[{Title}] ({isvalid})";
        }
    }
}
