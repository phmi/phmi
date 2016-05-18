using System.Collections.Generic;
using System.IO.Ports;

namespace PHmiIoDevice.Melsec.Implementation
{
    internal class FxCom: FxBase
    {
        private readonly SerialPortHelper _portHelper;

        public FxCom(SerialPort port, int timeout, int messageEndTimeout)
        {
            _portHelper = new SerialPortHelper(port, timeout, messageEndTimeout);
        }

        public override void Open()
        {
            _portHelper.Open();
        }

        public override void Dispose()
        {
            _portHelper.Close();
        }

        protected override void Write(byte[] message)
        {
            _portHelper.Write(message);
        }

        protected override IList<byte> Read(int length)
        {
            return _portHelper.Read(length);
        }

        protected override char FirstChar
        {
            get { return (char) 0x2; }
        }
    }
}
