using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PHmiClient.Loc;
using PHmiClient.Utils.Pagination;
using PHmiClient.Wcf;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClient.Logs
{
    internal class LogService : ILogService
    {
        private readonly IDictionary<int, LogAbstract> _logs = new Dictionary<int, LogAbstract>();
        private IList<Tuple<RemapLogParameter, Action<LogItem[]>[], LogItem>> _remapList; 

        public void Run(IService service)
        {
            _remapList = new List<Tuple<RemapLogParameter, Action<LogItem[]>[], LogItem>>();
            foreach (var log in _logs.Values)
            {
                var getInfo = log.GetInfo();
                var p = new RemapLogParameter
                    {
                        ItemTimesToDelete = log.TimesForDelete(),
                        ItemToSave = log.ItemToSave(),
                        GetItemsParameters = getInfo.Select(i => new Tuple<CriteriaType, DateTime, int, bool>(
                            i.Item1, i.Item2, i.Item3, i.Item4)).ToArray()
                    };
                if (!p.ItemTimesToDelete.Any() && p.ItemToSave == null && !p.GetItemsParameters.Any())
                    continue;
                p.LogId = log.Id;
                _remapList.Add(new Tuple<RemapLogParameter, Action<LogItem[]>[], LogItem>(
                    p, getInfo.Select(i => i.Item5).ToArray(), p.ItemToSave));
            }
            if (!_remapList.Any())
                return;
            var parameters = _remapList.Select(t => t.Item1).ToArray();
            var result = service.RemapLogs(parameters);
            for (var i = 0; i < parameters.Length; i++)
            {
                var getCallbacks = _remapList[i].Item2;
                var itemToSave = _remapList[i].Item3;
                if (itemToSave != null)
                {
                    itemToSave.Time = result[i].SaveResult;
                }
                var r = result[i];
                for (var j = 0; j < r.Items.Length; j++)
                {
                    var items = r.Items[j];
                    var callback = getCallbacks[j];
                    callback(items);
                }
            }
        }

        public void Clean()
        {
            if (_remapList == null)
                return;
            var emptyItems = new LogItem[0];
            foreach (var callback in _remapList.SelectMany(t => t.Item2))
            {
                callback(emptyItems);
            }
        }

        public string Name
        {
            get { return Res.LogService; }
        }

        public void Add(LogAbstract log)
        {
            _logs.Add(log.Id, log);
        }
    }
}
