using System.Resources;
using Moq;
using PHmiClientUnitTests;
using PHmiConfigurator.Utils;

namespace PHmiUnitTests.Configurator.Utils
{
    public class WhenUsingResourceBuilder : Specification
    {
        protected Mock<IResourceWriter> ResourceWriter;
        protected IResourceBuilder ResourceBuilder;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            ResourceWriter = new Mock<IResourceWriter>();
            ResourceBuilder = new ResourceBuilder(ResourceWriter.Object);
        }
    }
}
