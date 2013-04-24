using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace PHmiClient.Wcf
{
    internal class ServiceClientFactory : IServiceClientFactory
    {
        private readonly IDefaultBindingFactory _defaultBindingFactory;
        private readonly IServerUriFactory _serverUriFactory;
        private readonly string _server;
        private readonly string _guid;

        public ServiceClientFactory(string server, string guid) : this(server, guid, new DefaultBindingFactory(), new ServerUriFactory())
        {
        }

        public ServiceClientFactory(string server, string guid, IDefaultBindingFactory defaultBindingFactory, IServerUriFactory serverUriFactory)
        {
            _server = server;
            _guid = guid;
            _defaultBindingFactory = defaultBindingFactory;
            _serverUriFactory = serverUriFactory;
        }

        public IServiceClient Create()
        {
            var binding = _defaultBindingFactory.Create();
            var endpointAddress = GetEndpointAddress(_server);
            return new ServiceClient(_guid, binding, endpointAddress);
        }

        internal EndpointAddress GetEndpointAddress(string server)
        {
            var uri = _serverUriFactory.Create(server);
            var identity = EndpointIdentity.CreateDnsIdentity("localhost");
            return new EndpointAddress(uri, identity, new AddressHeaderCollection());
        }
    }
}
