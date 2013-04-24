using PHmiTools.Utils.Npg;
using System.Collections.Generic;

namespace PHmiRunner.Utils.Trends
{
    public class TrendsRepositoryFactory : ITrendsRepositoryFactory
    {
        private readonly INpgsqlConnectionFactory _connectionFactory;
        private readonly int _categoryId;
        private readonly IEnumerable<int> _trendTagIds; 

        public TrendsRepositoryFactory(INpgsqlConnectionFactory connectionFactory, int categoryId, IEnumerable<int> trendTagIds)
        {
            _connectionFactory = connectionFactory;
            _categoryId = categoryId;
            _trendTagIds = trendTagIds;
        }

        public ITrendsRepository Create()
        {
            return new TrendsRepository(_connectionFactory.Create(), _categoryId, _trendTagIds);
        }
    }
}
