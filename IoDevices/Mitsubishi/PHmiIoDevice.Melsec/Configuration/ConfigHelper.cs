using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace PHmiIoDevice.Melsec.Configuration
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
            var rootTagName = document.ChildNodes.OfType<XmlNode>()
                .First(n => n.Name == FxComConfig.Name || n.Name == FxEnetConfig.Name || n.Name == QConfig.Name)
                .Name;
            Config config;
            switch (rootTagName)
            {
                case FxComConfig.Name:
                    config = new FxComConfig();
                    break;
                case FxEnetConfig.Name:
                    config = new FxEnetConfig();
                    break;
                case QConfig.Name:
                    config = new QConfig();
                    break;
                default:
                    throw new NotSupportedException(rootTagName);
            }
            config.SetDocument(document);
            return config;
        }
    }
}
