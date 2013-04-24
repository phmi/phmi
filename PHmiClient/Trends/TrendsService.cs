using System.Collections.Generic;
using System.Linq;
using PHmiClient.Loc;
using PHmiClient.Utils.Notifications;
using PHmiClient.Wcf;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClient.Trends
{
    internal class TrendsService : ITrendsService
    {
        public const int MaxRarerer = 9;
        private readonly IReporter _reporter;
        private readonly IList<TrendsCategoryAbstract> _categories = new List<TrendsCategoryAbstract>(); 

        public TrendsService(IReporter reporter)
        {
            _reporter = reporter;
        }

        public void Run(IService service)
        {
            var categories = new List<TrendsCategoryAbstract>();
            var parameters = new List<RemapTrendsParameter>();
            foreach (var category in _categories)
            {
                var parameter = category.CreateRemapParameter();
                if (parameter == null)
                    continue;
                parameters.Add(parameter);
                categories.Add(category);
            }
            if (parameters.Any())
            {
                var result = service.RemapTrends(parameters.ToArray());
                for (var i = 0; i < categories.Count; i++)
                {
                    ApplyResult(categories[i], result[i]);
                }
            }
        }

        private void ApplyResult(TrendsCategoryAbstract category, RemapTrendsResult result)
        {
            _reporter.Report(result.Notifications);
            category.ApplyRemapResult(result);
        }

        public void Clean()
        {
            foreach (var ioDevice in _categories)
            {
                ioDevice.ApplyRemapResult(null);
            }
        }

        public string Name { get { return Res.TrendsService; } }

        public void Add(TrendsCategoryAbstract category)
        {
            _categories.Add(category);
        }
    }
}
