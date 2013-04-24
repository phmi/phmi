using System.ComponentModel;

namespace PHmiClient.Tags
{
    public interface ITag : INotifyPropertyChanged
    {
        string Name { get; }
        string Description { get; }
    }

    public interface ITag<T> : ITag
    {
        T Value { get; set; }
    }
}
