using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using PHmiClient.Users;
using PHmiClient.Utils.Pagination;
using PHmiClient.Utils.Wcf;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClient.Wcf
{
    internal sealed class ServiceClient  : ClientBase<IService>, IServiceClient
    {
        private string _guid;
        public const string GuidHeaderName = "guid";
        public const string GuidHeaderNamespace = "ns:phmi";
        
        public ServiceClient(string guid, string endpointConfigurationName, EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
            Initialize(guid);
        }

        public ServiceClient(string guid, Binding binding, EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
            Initialize(guid);
        }

        private void Initialize(string guid)
        {
            _guid = guid;
            Endpoint.Contract.Behaviors.Add(new HeaderEncriptionContractBehavior(GuidHeaderName, GuidHeaderNamespace));
        }

        void IDisposable.Dispose()
        {
            try
            {
                if (State != CommunicationState.Faulted)
                {
                    Close();
                }
            }
            finally
            {
                if (State != CommunicationState.Closed)
                {
                    Abort();
                }
            }
        }

        private T Invoke<T>(Func<T> func)
        {
            using (new OperationContextScope(InnerChannel))
            {
                var guidHeader = new MessageHeader<string>(_guid);
                var untypedGuidHeader = guidHeader.GetUntypedHeader(GuidHeaderName, GuidHeaderNamespace);
                OperationContext.Current.OutgoingMessageHeaders.Add(untypedGuidHeader);
                return func.Invoke();
            }
        }

        private T Invoke<TP, T>(Func<TP, T> func, TP parameter)
        {
            return Invoke(() => func.Invoke(parameter));
        }

        private T Invoke<TP1, TP2, T>(Func<TP1, TP2, T> func, TP1 parameter1, TP2 parameter2)
        {
            return Invoke(() => func.Invoke(parameter1, parameter2));
        }

        private T Invoke<TP1, TP2, TP3, T>(Func<TP1, TP2, TP3, T> func, TP1 parameter1, TP2 parameter2, TP3 parameter3)
        {
            return Invoke(() => func.Invoke(parameter1, parameter2, parameter3));
        }

        private T Invoke<TP1, TP2, TP3, TP4, T>(Func<TP1, TP2, TP3, TP4, T> func, TP1 parameter1, TP2 parameter2, TP3 parameter3, TP4 parameter4)
        {
            return Invoke(() => func.Invoke(parameter1, parameter2, parameter3, parameter4));
        }

        public UpdateStatusResult UpdateStatus()
        {
            return Invoke(Channel.UpdateStatus);
        }

        public RemapTagsResult[] RemapTags(RemapTagsParameter[] parameters)
        {
            return Invoke(Channel.RemapTags, parameters);
        }

        public User LogOn(string name, string password)
        {
            return Invoke(Channel.LogOn, name, password);
        }

        public bool ChangePassword(string name, string oldPassword, string newPassword)
        {
            return Invoke(Channel.ChangePassword, name, oldPassword, newPassword);
        }

        public RemapAlarmResult[] RemapAlarms(RemapAlarmsParameter[] parameters)
        {
            return Invoke(Channel.RemapAlarms, parameters);
        }

        public RemapTrendsResult[] RemapTrends(RemapTrendsParameter[] parameters)
        {
            return Invoke(Channel.RemapTrends, parameters);
        }

        public RemapLogResult[] RemapLogs(RemapLogParameter[] parameters)
        {
            return Invoke(Channel.RemapLogs, parameters);
        }

        public User[] GetUsers(Identity identity, CriteriaType criteriaType, string name, int count)
        {
            return Invoke(Channel.GetUsers, identity, criteriaType, name, count);
        }

        public bool SetPassword(Identity identity, long id, string password)
        {
            return Invoke(Channel.SetPassword, identity, id, password);
        }

        public UpdateUserResult UpdateUser(Identity identity, User user)
        {
            return Invoke(Channel.UpdateUser, identity, user);
        }

        public InsertUserResult InsertUser(Identity identity, User user)
        {
            return Invoke(Channel.InsertUser, identity, user);
        }

        public User[] GetUsersByIds(Identity identity, long[] ids)
        {
            return Invoke(Channel.GetUsersByIds, identity, ids);
        }
    }
}
