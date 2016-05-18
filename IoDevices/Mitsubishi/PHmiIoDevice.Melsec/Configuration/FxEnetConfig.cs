using System.Globalization;
using System.Xml;

namespace PHmiIoDevice.Melsec.Configuration
{
    public class FxEnetConfig : EnetConfig
    {
        public const string Name = "FxEnet";
        private int _port = 5551;

        public FxEnetConfig() : base(Name)
        {
        }

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }

        protected override void GetXml(XmlDocument document, XmlNode rootElement)
        {
            base.GetXml(document, rootElement);
            AddElement(document, rootElement, "Port", Port.ToString(CultureInfo.InvariantCulture));
        }

        protected override void SetXml(XmlNode rootElement)
        {
            base.SetXml(rootElement);
            Port = GetInt(rootElement, "Port");
        }
    }
}
