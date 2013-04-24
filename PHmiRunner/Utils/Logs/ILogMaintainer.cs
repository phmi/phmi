using System;
using PHmiClient.Logs;
using PHmiClient.Utils.Pagination;

namespace PHmiRunner.Utils.Logs
{
    public interface ILogMaintainer
    {
        DateTime Save(LogItem item);
        void Delete(DateTime[] times);
        LogItem[][] GetItems(Tuple<CriteriaType, DateTime, int, bool>[] parameters);
    }
}
