using System.Resources;

namespace PHmiConfigurator.Utils
{
    public class ResourceBuilderFactory : IResourceBuilderFactory
    {
        public IResourceBuilder CreateResXBuilder(string file)
        {
            return new ResourceBuilder(new ResXResourceWriter(file));
        }
    }
}
