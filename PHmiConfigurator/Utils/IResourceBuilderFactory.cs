namespace PHmiConfigurator.Utils
{
    public interface IResourceBuilderFactory
    {
        IResourceBuilder CreateResXBuilder(string file);
    }
}
