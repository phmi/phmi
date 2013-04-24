using PHmiClient.Wcf;

namespace PHmiClient.PHmiSystem
{
    internal interface IServiceRunTarget
    {
        void Run(IService service);
        void Clean();
        string Name { get; }
    }
}
