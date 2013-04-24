using System;
using System.Diagnostics;
using System.Reflection;

namespace PHmiRunner.Utils.IoDeviceRunner
{
    public class IoDeviceWrapperFactory : IIoDeviceWrapperFactory
    {
        private readonly string _domainName;
        private AppDomain _appDomain;

        public IoDeviceWrapperFactory(string domainName)
        {
            _domainName = domainName;
        }

        public IIoDeviceWrapper Create()
        {
            _appDomain = AppDomain.CreateDomain(_domainName, null, null);
            var type = typeof (IoDeviceWrapper);
            var assembly = Assembly.GetAssembly(type);
            Debug.Assert(type.FullName != null, "type.FullName != null");
            return (IIoDeviceWrapper)_appDomain.CreateInstanceAndUnwrap(assembly.FullName, type.FullName);
        }

        public void UnloadDomain()
        {
            var appDomain = _appDomain;
            if (appDomain != null)
            {
                AppDomain.Unload(appDomain);
            }
        }
    }
}
