using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Xml;

namespace PHmiClient.Utils.Wcf
{
    public class HeaderEncriptionContractBehavior : IContractBehavior
    {
        private readonly string _name;
        private readonly string _ns;

        public HeaderEncriptionContractBehavior(string name, string ns)
        {
            _name = name;
            _ns = ns;
        }

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            var requirements = bindingParameters.Find<ChannelProtectionRequirements>();
            requirements.IncomingSignatureParts.ChannelParts.HeaderTypes.Add(new XmlQualifiedName(_name, _ns));
            requirements.IncomingEncryptionParts.ChannelParts.HeaderTypes.Add(new XmlQualifiedName(_name, _ns));
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }
    }
}
