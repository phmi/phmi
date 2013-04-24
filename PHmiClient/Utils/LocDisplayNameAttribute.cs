using System;
using System.ComponentModel;
using System.Reflection;

namespace PHmiClient.Utils
{
    public class LocDisplayNameAttribute : DisplayNameAttribute
    {
        public LocDisplayNameAttribute(string displayName) : base(displayName) { }

        public Type ResourceType { get; set; }

        public override string DisplayName
        {
            get
            {
                if (ResourceType == null)
                    return base.DisplayName;
                return (string) ResourceType.InvokeMember(
                    base.DisplayName,
                    BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Public,
                    null, ResourceType, null);
            }
        }
    }
}
