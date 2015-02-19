using System;
using System.Linq;
using Opc;
using Opc.Da;
using PHmiIoDeviceTools;
using Server = Opc.Da.Server;

namespace PHmiIoDevice.Opc
{
    public class OpcIoDevice : IIoDevice
    {
        private readonly URL _url;
        private readonly Server _server;

        public OpcIoDevice(string options)
        {
            _url = new URL(options);
            var fact = new OpcCom.Factory();
            _server = new Server(fact, null);
        }

        public void Dispose()
        {
            _server.Disconnect();
        }

        public void Open()
        {
            _server.Connect(_url, new ConnectData(new System.Net.NetworkCredential()));
        }

        public object[] Read(ReadParameter[] readParameters)
        {
            var items = new Item[readParameters.Length];
            for (var i = 0; i < readParameters.Length; i++)
            {
                items[i] = new Item {ItemName = readParameters[i].Address};
            }
            var itemValues = _server.Read(items);
            var result = new Object[itemValues.Length];
            for (var i = 0; i < itemValues.Length; i++)
            {
                result[i] = itemValues[i].Value;
            }
            return result;
        }

        public void Write(WriteParameter[] writeParameters)
        {
            var itemValues = new ItemValue[writeParameters.Length];
            for (var i = 0; i < writeParameters.Length; i++)
            {
                var writeParameter = writeParameters[i];
                var item = new Item {ItemName = writeParameter.Address};
                itemValues[i] = new ItemValue(item) {Value = writeParameter.Value};
            }
            _server.Write(itemValues);
        }
    }
}
