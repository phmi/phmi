namespace PHmiClient.Utils.Runner
{
    public interface ICyclicRunnerFactory
    {
        ICyclicRunner Create(IRunTarget target);
    }
}
