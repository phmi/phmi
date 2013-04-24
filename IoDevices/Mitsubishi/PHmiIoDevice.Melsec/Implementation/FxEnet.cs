using System.Collections.Generic;

namespace PHmiIoDevice.Melsec.Implementation
{
    internal class FxEnet: FxBase
    {
        private readonly TcpClientHelper _tcpHelper;

        public FxEnet(string address, int timeout, int messageEndTimeout)
        {
            _tcpHelper = new TcpClientHelper(address, 5551, timeout, messageEndTimeout);
        }

        public override void Open()
        {
            _tcpHelper.Open();
        }

        public override void Dispose()
        {
            _tcpHelper.Close();
        }

        protected override void Write(byte[] message)
        {
            _tcpHelper.Write(message);
        }

        protected override IList<byte> Read(int length)
        {
            return _tcpHelper.Read(length);
        }
    }
}
