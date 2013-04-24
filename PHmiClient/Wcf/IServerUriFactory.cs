using System;

namespace PHmiClient.Wcf
{
    internal interface IServerUriFactory
    {
        Uri Create(string server);
    }
}
