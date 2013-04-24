using System.Xml;

namespace PHmiIoDevice.Melsec.Configuration
{
    public class EnetConfig : Config
    {
        private string _address = "127.0.0.1";

        public EnetConfig(string name) : base(name)
        {
        }

        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChanged("Address");
            }
        }

        protected override void GetXml(XmlDocument document, XmlNode rootElement)
        {
            base.GetXml(document, rootElement);
            AddElement(document, rootElement, "Address", Address);
        }

        protected override void SetXml(XmlNode rootElement)
        {
            base.SetXml(rootElement);
            Address = GetString(rootElement, "Address");
        }
    }
}
