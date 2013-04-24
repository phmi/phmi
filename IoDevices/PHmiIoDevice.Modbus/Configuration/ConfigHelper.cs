using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace PHmiIoDevice.Modbus.Configuration
{
    public static class ConfigHelper
    {
        public static XmlDocument GetDocument(string xml)
        {
            var document = new XmlDocument();
            using (var stream = new MemoryStream())
            {
                var textWriter = new StreamWriter(stream, Encoding.Unicode);
                textWriter.Write(xml);
                textWriter.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                document.Load(stream);
            }
            return document;
        }

        public static string GetXml(XmlDocument document)
        {
            var sb = new StringBuilder();
            var w = XmlWriter.Create(sb, new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true
            });
            document.Save(w);
            return sb.ToString();
        }

        public static Config GetConfig(string options)
        {
            var document = GetDocument(options);
            var configNames = new []
                {
                    AsciiConfig.Name,
                    AsciiViaTcpConfig.Name,
                    RtuConfig.Name,
                    RTUviaTCPConfig.Name,
                    TcpConfig.Name
                };
            var rootTagName = document.ChildNodes.OfType<XmlNode>()
                .First(n => configNames.Contains(n.Name))
                .Name;
            Config config;
            switch (rootTagName)
            {
                case AsciiConfig.Name:
                    config = new AsciiConfig();
                    break;
                case AsciiViaTcpConfig.Name:
                    config = new AsciiViaTcpConfig();
                    break;
                case RtuConfig.Name:
                    config = new RtuConfig();
                    break;
                case RTUviaTCPConfig.Name:
                    config = new RTUviaTCPConfig();
                    break;
                case TcpConfig.Name:
                    config = new TcpConfig();
                    break;
                default:
                    throw new NotSupportedException(rootTagName);
            }
            config.SetDocument(document);
            return config;
        }
    }
}
