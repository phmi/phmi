using System.Collections.Generic;

namespace PHmiModel.Interfaces
{
    public interface IRepository
    {
        ICollection<T> GetRepository<T>();
    }
}
