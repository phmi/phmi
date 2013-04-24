namespace PHmiConfigurator.Utils
{
    public interface ICodeWriterFactory
    {
        ICodeWriter Create(string file);
    }
}
