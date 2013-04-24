using PHmiClient.PHmiSystem;

namespace PHmiClient.Trends
{
    internal interface ITrendsService : IServiceRunTarget
    {
        void Add(TrendsCategoryAbstract category);
    }
}
