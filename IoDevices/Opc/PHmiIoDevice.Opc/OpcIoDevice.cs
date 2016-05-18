using System;
using System.Collections.Generic;
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
        private Subscription _group;
        private readonly Dictionary<string, object> _itemValues = new Dictionary<string, object>();
        private bool _canSubscribe = true;

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
            var groupState = new SubscriptionState
            {
                Name = "Groups",
                UpdateRate = 50,
                Active = true
            };
            _group = (Subscription)_server.CreateSubscription(groupState);
            _group.DataChanged += _group_DataChanged;
        }

        private void _group_DataChanged(object subscriptionHandle, object requestHandle, ItemValueResult[] values)
        {
            lock (_itemValues)
            {
                foreach (var value in values)
                {
                    _itemValues[value.ItemName] = value.Value;
                }
            }
        }

        public object[] Read(ReadParameter[] readParameters)
        {
            lock (_itemValues)
            {
                if (_canSubscribe)
                {
                    SubscribeToRead(readParameters);
                }
                var result = new object[readParameters.Length];
                if (_canSubscribe)
                {
                    for (var i = 0; i < readParameters.Length; i++)
                    {
                        result[i] = _itemValues[readParameters[i].Address];
                    }
                }
                else
                {
                    var items = readParameters.Select(p => new Item
                    {
                        ItemName = p.Address
                    }).ToArray();
                    var itemValues = _server.Read(items);
                    for (var i = 0; i < readParameters.Length; i++)
                    {
                        result[i] = itemValues[i].Value;
                    }
                }
                return result;
            }
        }

        private void SubscribeToRead(ReadParameter[] readParameters)
        {
            var newParameters = readParameters.Where(p => !_itemValues.ContainsKey(p.Address)).ToArray();
            if (!newParameters.Any())
                return;
            var items = newParameters.Select(p => new Item
            {
                ItemName = p.Address
            }).ToArray();
            try
            {
                _group.AddItems(items);
            }
            catch
            {
                _canSubscribe = false;
            }
            if (_canSubscribe)
            {
                var itemValues = _server.Read(items);
                foreach (var itemValue in itemValues)
                {
                    _itemValues.Add(itemValue.ItemName, itemValue.Value);
                }
            }
        }

        public void Write(WriteParameter[] writeParameters)
        {
            lock (_itemValues)
            {
                var itemValues = new ItemValue[writeParameters.Length];
                var items = new Item[writeParameters.Length];
                for (var i = 0; i < writeParameters.Length; i++)
                {
                    var writeParameter = writeParameters[i];
                    var item = new Item { ItemName = writeParameter.Address };
                    items[i] = item;
                    itemValues[i] = new ItemValue(item) { Value = writeParameter.Value };
                }
                _server.Write(itemValues);

                var readResult = _server.Read(items);
                foreach (var itemValueResult in readResult)
                {
                    if (_itemValues.ContainsKey(itemValueResult.ItemName))
                    {
                        _itemValues[itemValueResult.ItemName] = itemValueResult.Value;
                    }
                }
            }
        }
    }
}
