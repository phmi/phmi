using System.ComponentModel;

namespace PHmiClient.Tags
{
    public abstract class TagAbstract<T> : ITag<T>
    {
        internal abstract int Id { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract T Value { get; set; }

        internal abstract void UpdateValue(T value);

        internal abstract bool IsWritten { get; }

        internal abstract T GetWrittenValue();

        internal abstract bool IsRead { get; }

        public abstract event PropertyChangedEventHandler PropertyChanged;
    }
}
