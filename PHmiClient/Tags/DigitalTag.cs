using System;
using PHmiClient.Utils;

namespace PHmiClient.Tags
{
    public class DigitalTag : Tag<bool?>, IDigitalTag
    {
        internal DigitalTag(IDispatcherService dispatcherService, int id, string name, Func<string> descriptionGetter)
            : base(dispatcherService, id, name, descriptionGetter)
        {
        }
    }
}
