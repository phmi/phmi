using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using PHmiClient.Alarms;
using PHmiClient.Logs;
using PHmiClient.Users;
using PHmiClient.Utils;
using PHmiClient.Utils.Pagination;
using PHmiClient.Wcf;
using PHmiClient.Wcf.ServiceTypes;
using PHmiRunner.Utils.Alarms;
using PHmiRunner.Utils.IoDeviceRunner;
using PHmiRunner.Utils.Logs;
using PHmiRunner.Utils.Trends;

namespace PHmiRunner.Utils.Wcf
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class Service : IService
    {
        private readonly IProject _project;
        private readonly string _guid;
        private readonly ITimeService _timeService;

        public Service(IProject project, string guid, ITimeService timeService)
        {
            _project = project;
            _guid = guid;
            _timeService = timeService;
        }

        private bool NotValid()
        {
            if (_guid == null)
                return false;
            try
            {
                var headers = OperationContext.Current.IncomingMessageHeaders;
                var guid = headers.GetHeader<string>(ServiceClient.GuidHeaderName, ServiceClient.GuidHeaderNamespace);
                return _guid != guid;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public UpdateStatusResult UpdateStatus()
        {
            if (NotValid())
                return null;
            return new UpdateStatusResult
                {
                    Time = _timeService.UtcTime
                };
        }

        public RemapTagsResult[] RemapTags(RemapTagsParameter[] parameters)
        {
            if (NotValid())
                return null;
            var results = new RemapTagsResult[parameters.Length];
            for (var parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                var parameter = parameters[parameterIndex];
                var result = new RemapTagsResult();
                results[parameterIndex] = result;
                var ioDevice = _project.IoDeviceRunTargets[parameter.IoDeviceId];
                Remap(ioDevice, parameter, result);
            }
            return results;
        }

        private static void Remap(IIoDeviceRunTarget ioDevice, RemapTagsParameter parameter, RemapTagsResult result)
        {
            var readOnly = !parameter.DigWriteIds.Any() && !parameter.NumWriteIds.Any();
            if (readOnly)
                ioDevice.EnterReadLock();
            else
                ioDevice.EnterWriteLock();
            try
            {
                if (!readOnly)
                {
                    WriteDigValues(ioDevice, parameter.DigWriteIds, parameter.DigWriteValues);
                    WriteNumValues(ioDevice, parameter.NumWriteIds, parameter.NumWriteValues);
                }
                result.DigReadValues = ReadDigValues(ioDevice, parameter.DigReadIds);
                result.NumReadValues = ReadNumValues(ioDevice, parameter.NumReadIds);
                result.Notifications = GetNotifications(ioDevice.Reporter.Notifications);
            }
            finally
            {
                if (readOnly)
                    ioDevice.ExitReadLock();
                else
                    ioDevice.ExitWriteLock();
            }
        }

        private static void WriteDigValues(IIoDeviceRunTarget ioDevice, IList<int> ids, IList<bool> values)
        {
            for (var i = 0; i < ids.Count; i++)
            {
                ioDevice.SetDigitalValue(ids[i], values[i]);
            }
        }

        private static void WriteNumValues(IIoDeviceRunTarget ioDevice, IList<int> ids, IList<double> values)
        {
            for (var i = 0; i < ids.Count; i++)
            {
                ioDevice.SetNumericValue(ids[i], values[i]);
            }
        }

        private static bool?[] ReadDigValues(IIoDeviceRunTarget ioDevice, IList<int> ids)
        {
            var values = new bool?[ids.Count];
            for (var i = 0; i < ids.Count; i++)
            {
                values[i] = ioDevice.GetDigitalValue(ids[i]);
            }
            return values;
        }

        private static double?[] ReadNumValues(IIoDeviceRunTarget ioDevice, IList<int> ids)
        {
            var values = new double?[ids.Count];
            for (var i = 0; i < ids.Count; i++)
            {
                values[i] = ioDevice.GetNumericValue(ids[i]);
            }
            return values;
        }

        private static WcfNotification[] GetNotifications(IEnumerable<PHmiClient.Utils.Notifications.Notification> notifications)
        {
            return notifications.Select(n => new WcfNotification
                {
                    StartTime = n.StartTime,
                    Message = n.Message,
                    ShortDescription = n.ShortDescription,
                    LongDescription = n.LongDescription
                }).ToArray();
        }

        public User LogOn(string name, string password)
        {
            if (NotValid())
                return null;
            return _project.UsersRunner.LogOn(name, password);
        }

        public bool ChangePassword(string name, string oldPassword, string newPassword)
        {
            return _project.UsersRunner.ChangePassword(name, oldPassword, newPassword);
        }

        public RemapAlarmResult[] RemapAlarms(RemapAlarmsParameter[] parameters)
        {
            if (NotValid())
                return null;
            var results = new RemapAlarmResult[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var result = new RemapAlarmResult();
                var parameter = parameters[i];
                results[i] = result;
                IAlarmsRunTarget runTarget;
                if (!_project.AlarmsRunTargets.TryGetValue(parameter.CategoryId, out runTarget))
                    continue;
                AcknowledgeAlarms(parameter, runTarget);
                ReadAlarmStatus(parameter, result, runTarget);
                ReadCurrentAlarms(parameter, result, runTarget);
                ReadHistoryAlarms(parameter, result, runTarget); 
                result.Notifications = GetNotifications(runTarget.Reporter.Notifications);
            }
            return results;
        }

        private static void AcknowledgeAlarms(RemapAlarmsParameter parameter, IAlarmsRunTarget runTarget)
        {
            foreach (var p in parameter.AcknowledgeParameters)
            {
                runTarget.Acknowledge(p.Item1, p.Item2);
            }
        }

        private static void ReadAlarmStatus(RemapAlarmsParameter parameter, RemapAlarmResult result, IAlarmsRunTarget runTarget)
        {
            if (!parameter.GetStatus)
                return;
            var status = runTarget.GetHasActiveAndUnacknowledged();
            result.HasActive = status.Item1;
            result.HasUnacknowledged = status.Item2;
        }

        private static void ReadCurrentAlarms(RemapAlarmsParameter parameter, RemapAlarmResult result, IAlarmsRunTarget runTarget)
        {
            var currentAlarms = new Alarm[parameter.CurrentParameters.Length][];
            for (var i = 0; i < parameter.CurrentParameters.Length; i++)
            {
                var curPar = parameter.CurrentParameters[i];
                currentAlarms[i] = runTarget.GetCurrentAlarms(curPar.Item1, curPar.Item2, curPar.Item3);
            }
            result.Current = currentAlarms;
        }

        private static void ReadHistoryAlarms(RemapAlarmsParameter parameter, RemapAlarmResult result, IAlarmsRunTarget runTarget)
        {
            var currentAlarms = new Alarm[parameter.HistoryParameters.Length][];
            for (var i = 0; i < parameter.HistoryParameters.Length; i++)
            {
                var curPar = parameter.HistoryParameters[i];
                currentAlarms[i] = runTarget.GetHistoryAlarms(curPar.Item1, curPar.Item2, curPar.Item3);
            }
            result.History = currentAlarms;
        }

        public RemapTrendsResult[] RemapTrends(RemapTrendsParameter[] parameters)
        {
            if (NotValid())
                return null;
            var result = new RemapTrendsResult[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var trendsRunTarget = _project.TrendsRunTargets[parameter.CategoryId];
                var r = new RemapTrendsResult();
                ReadTrendsSamples(parameter, r, trendsRunTarget);
                ReadTrendsPages(parameter, r, trendsRunTarget);
                r.Notifications = GetNotifications(trendsRunTarget.Reporter.Notifications);
                result[i] = r;
            }
            return result;
        }

        private static void ReadTrendsSamples(RemapTrendsParameter parameter, RemapTrendsResult result, ITrendsRunTarget runTarget)
        {
            var r = new Tuple<DateTime, double?[]>[parameter.SamplesParameters.Length][];
            for (var i = 0; i < parameter.SamplesParameters.Length; i++)
            {
                var samplesParameter = parameter.SamplesParameters[i];
                r[i] = runTarget.GetSamples(
                    samplesParameter.Item1, samplesParameter.Item2, samplesParameter.Item3, samplesParameter.Item4);
            }
            result.Samples = r;
        }

        private static void ReadTrendsPages(RemapTrendsParameter parameter, RemapTrendsResult result, ITrendsRunTarget runTarget)
        {
            var r = new Tuple<DateTime, double?[]>[parameter.PageParameters.Length][];
            for (var i = 0; i < parameter.PageParameters.Length; i++)
            {
                var pageParameter = parameter.PageParameters[i];
                r[i] = runTarget.GetPage(
                    pageParameter.Item1, pageParameter.Item2, pageParameter.Item3, pageParameter.Item4);
            }
            result.Pages = r;
        }

        public RemapLogResult[] RemapLogs(RemapLogParameter[] parameters)
        {
            if (NotValid())
                return null;
            var result = new RemapLogResult[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var maintainer = _project.LogMaintainers[parameter.LogId];
                var r = new RemapLogResult();
                DeleteItems(maintainer, parameter);
                SaveItem(maintainer, parameter, r);
                GetItems(maintainer, parameter, r);
                result[i] = r;
            }
            return result;
        }

        private static void DeleteItems(ILogMaintainer maintainer, RemapLogParameter parameter)
        {
            maintainer.Delete(parameter.ItemTimesToDelete);
        }

        private static void SaveItem(ILogMaintainer maintainer, RemapLogParameter parameter, RemapLogResult remapLogResult)
        {
            if (parameter.ItemToSave == null)
                return;
            remapLogResult.SaveResult = maintainer.Save(parameter.ItemToSave);
        }

        private static void GetItems(ILogMaintainer maintainer, RemapLogParameter parameter, RemapLogResult remapLogResult)
        {
            remapLogResult.Items = parameter.GetItemsParameters.Any()
                ? maintainer.GetItems(parameter.GetItemsParameters) : new LogItem[0][];
        }

        public User[] GetUsers(Identity identity, CriteriaType criteriaType, string name, int count)
        {
            if (NotValid())
                return null;
            return _project.UsersRunner.GetUsers(identity, criteriaType, name, count);
        }

        public bool SetPassword(Identity identity, long id, string password)
        {
            if (NotValid())
                return false;
            return _project.UsersRunner.SetPassword(identity, id, password);
        }

        public UpdateUserResult UpdateUser(Identity identity, User user)
        {
            if (NotValid())
                return UpdateUserResult.Fail;
            return _project.UsersRunner.UpdateUser(identity, user);
        }

        public InsertUserResult InsertUser(Identity identity, User user)
        {
            if (NotValid())
                return InsertUserResult.Fail;
            return _project.UsersRunner.InsertUser(identity, user);
        }

        public User[] GetUsersByIds(Identity identity, long[] ids)
        {
            if (NotValid())
                return null;
            return _project.UsersRunner.GetUsers(identity, ids);
        }
    }
}
