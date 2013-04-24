using System;
using System.Collections.Generic;

namespace PHmiClient.Utils.Pagination
{
    public interface IPaginationService<out T, in TCriteria>
    {
        void GetItems(CriteriaType criteriaType, int maxCount, TCriteria criteria, Action<IEnumerable<T>> callback);
    }
}
