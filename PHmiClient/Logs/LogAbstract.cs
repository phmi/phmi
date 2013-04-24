using System;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Logs
{
    public abstract class LogAbstract
    {
        internal abstract int Id { get; }

        public abstract string Name { get; }

        public abstract void GetItems(CriteriaType criteriaType, DateTime criteria, int maxCount, bool includeBytes, Action<LogItem[]> callback);

        public abstract void Save(LogItem item);

        public abstract void Delete(LogItem item);

        internal abstract Tuple<CriteriaType, DateTime, int, bool, Action<LogItem[]>>[] GetInfo();

        internal abstract LogItem ItemToSave();

        internal abstract DateTime[] TimesForDelete();
    }
}
