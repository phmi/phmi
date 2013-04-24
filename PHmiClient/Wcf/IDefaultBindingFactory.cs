using System.ServiceModel.Channels;

namespace PHmiClient.Wcf
{
    internal interface IDefaultBindingFactory
    {
        Binding Create();
    }
}
