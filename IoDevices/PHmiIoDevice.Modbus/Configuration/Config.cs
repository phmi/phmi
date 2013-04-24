using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using PHmiIoDevice.Modbus.BytesConverters;

namespace PHmiIoDevice.Modbus.Configuration
{
    public class Config : INotifyPropertyChanged
    {
        private int _timeout = 3000;
        private int _messageEndTimeout = 10;
        private readonly string _name;
        private byte _defaultAddress = 1;
        private BytesOrder _bytesOrder = BytesOrder.HL;

        public Config(string name)
        {
            _name = name;
        }

        public string ConfigName { get { return _name; } }

        public byte DefaultAddress
        {
            get { return _defaultAddress; }
            set
            {
                _defaultAddress = value;
                OnPropertyChanged("DefaultAddress");
            }
        }

        public BytesOrder BytesOrder
        {
            get { return _bytesOrder; }
            set
            {
                _bytesOrder = value;
                OnPropertyChanged("BytesOrder");
            }
        }

        public int Timeout
        {
            get { return _timeout; }
            set
            {
                if (value < 100)
                    throw new Exception("Timeout. Must be >= 100");
                _timeout = value;
                OnPropertyChanged("Timeout");
            }
        }

        public int MessageEndTimeout
        {
            get { return _messageEndTimeout; }
            set
            {
                if (value < 10)
                    throw new Exception("Message end timeout. Must be >= 10");
                _messageEndTimeout = value;
                OnPropertyChanged("MessageEndTimeout");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void AddElement(XmlDocument document, XmlNode rootElement, string name, string value)
        {
            var element = document.CreateElement(name);
            element.InnerText = value;
            rootElement.AppendChild(element);
        }

        protected virtual void GetXml(XmlDocument document, XmlNode rootElement)
        {
            AddElement(document, rootElement, "Timeout", Timeout.ToString(CultureInfo.InvariantCulture));
            AddElement(document, rootElement, "MessageEndTimeout", MessageEndTimeout.ToString(CultureInfo.InvariantCulture));
        }

        public string GetXml()
        {
            return ConfigHelper.GetXml(GetDocument());
        }

        public XmlDocument GetDocument()
        {
            var document = new XmlDocument();
            var rootElement = document.CreateElement(_name);
            document.AppendChild(rootElement);
            GetXml(document, rootElement);
            return document;
        }

        public void SetXml(string xml)
        {
            var document = ConfigHelper.GetDocument(xml);
            SetDocument(document);
        }

        public void SetDocument(XmlDocument document)
        {
            var rootElement = document.GetElementsByTagName(_name).Item(0);
            SetXml(rootElement);
        }

        protected virtual void SetXml(XmlNode rootElement)
        {
            Timeout = GetInt(rootElement, "Timeout");
            MessageEndTimeout = GetInt(rootElement, "MessageEndTimeout");
        }

        protected string GetString(XmlNode rootElement, string tagName)
        {
            foreach (var node in rootElement.ChildNodes.OfType<XmlNode>().Where(node => node.Name == tagName))
            {
                return node.InnerText;
            }
            throw new KeyNotFoundException(tagName);
        }

        protected int GetInt(XmlNode rootElement, string tagName)
        {
            return int.Parse(GetString(rootElement, tagName));
        }
    }
}
