namespace PHmiClient.Utils.Runner
{
    public class CyclicRunnerFactory : ICyclicRunnerFactory
    {
        public ICyclicRunner Create(IRunTarget target)
        {
            return new CyclicRunner(new TimerService(), target);
        }
    }
}
