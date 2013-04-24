using System;
using System.ComponentModel;

namespace PHmiClient.Utils
{
    public interface IStatusService : INotifyPropertyChanged
    {
        TimeSpan LifeTime { get; set; }
        string Message { get; set; }
    }
}
