using System;

namespace PHmiClient.Wcf
{
    internal class ServerUriFactory : IServerUriFactory
    {
        public Uri Create(string server)
        {
            return new Uri("http://" + server + "/PHmiService", UriKind.Absolute);
        }
    }
}
