using System;
using System.Collections.Generic;
using PHmiClient.Alarms;
using PHmiClient.Loc;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Controls.Pages
{
    public class AlarmHistoryPage : AlarmsPage
    {
        private class PaginationService : IPaginationService<Alarm, AlarmSampleId>
        {
            private readonly AlarmCategoryAbstract _alarmCategory;

            public PaginationService(AlarmCategoryAbstract alarmCategory)
            {
                _alarmCategory = alarmCategory;
            }

            public void GetItems(CriteriaType criteriaType, int maxCount, AlarmSampleId criteria, Action<IEnumerable<Alarm>> callback)
            {
                _alarmCategory.GetHistory(criteriaType, criteria, maxCount, callback);
            }
        }

        protected override IPaginationService<Alarm, AlarmSampleId> CreatePaginationService(
            AlarmCategoryAbstract alarmCategory)
        {
            return new PaginationService(alarmCategory);
        }

        public override object PageName { get { return Res.AlarmHistory; } }
    }
}
