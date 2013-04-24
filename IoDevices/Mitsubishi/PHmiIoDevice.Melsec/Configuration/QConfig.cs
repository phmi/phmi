using System.Globalization;
using System.Xml;

namespace PHmiIoDevice.Melsec.Configuration
{
    public class QConfig : EnetConfig
    {
        public const string Name = "Q";
        private int _port = 5002;
        private byte _pcNumber = 0xFF;
        private byte _networkNumber;

        public QConfig() : base(Name)
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

        public byte PcNumber
        {
            get { return _pcNumber; }
            set
            {
                _pcNumber = value;
                OnPropertyChanged("PcNumber");
            }
        }

        public byte NetworkNumber
        {
            get { return _networkNumber; }
            set
            {
                _networkNumber = value;
                OnPropertyChanged("NetworkNumber");
            }
        }

        protected override void GetXml(XmlDocument document, XmlNode rootElement)
        {
            base.GetXml(document, rootElement);
            AddElement(document, rootElement, "Port", Port.ToString(CultureInfo.InvariantCulture));
            AddElement(document, rootElement, "PcNumber", PcNumber.ToString(CultureInfo.InvariantCulture));
            AddElement(document, rootElement, "NetworkNumber", NetworkNumber.ToString(CultureInfo.InvariantCulture));
        }

        protected override void SetXml(XmlNode rootElement)
        {
            base.SetXml(rootElement);
            Port = GetInt(rootElement, "Port");
            PcNumber = (byte) GetInt(rootElement, "PcNumber");
            NetworkNumber = (byte) GetInt(rootElement, "NetworkNumber");
        }
    }
}
