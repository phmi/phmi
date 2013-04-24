using System;
using System.Collections.Concurrent;
using PHmiClient.Loc;
using PHmiClient.Utils.Pagination;
using PHmiClient.Wcf;

namespace PHmiClient.Users
{
    internal class UsersRunTarget : IUsersRunTarget
    {
        private readonly ConcurrentQueue<Action<IService>> _queue = new ConcurrentQueue<Action<IService>>();

        public void Run(IService service)
        {
            Action<IService> action;
            while (_queue.TryDequeue(out action))
            {
                action.Invoke(service);
            }
        }

        public void Clean()
        {
            while (_queue.Count > 0)
            {
                Action<IService> t;
                _queue.TryDequeue(out t);
            }
        }

        public string Name
        {
            get { return Res.UsersService; }
        }

        public void LogOn(string name, string password, Action<User> callback)
        {
            _queue.Enqueue(service => callback(service.LogOn(name, password)));
        }

        public void ChangePassword(string name, string oldPassword, string newPassword, Action<bool> callback)
        {
            _queue.Enqueue(service => callback(service.ChangePassword(name, oldPassword, newPassword)));
        }

        public void GetUsers(Identity identity, CriteriaType criteriaType, string name, int count, Action<User[]> callback)
        {
            _queue.Enqueue(service => callback(service.GetUsers(identity, criteriaType, name, count)));
        }

        public void SetPassword(Identity identity, long userId, string password, Action<bool> callback)
        {
            _queue.Enqueue(service => callback(service.SetPassword(identity, userId, password)));
        }

        public void UpdateUser(Identity identity, User user, Action<UpdateUserResult> callback)
        {
            _queue.Enqueue(service => callback(service.UpdateUser(identity, user)));
        }

        public void InsertUser(Identity identity, User user, Action<InsertUserResult> callback)
        {
            _queue.Enqueue(service => callback(service.InsertUser(identity, user)));
        }

        public void GetUsers(Identity identity, long[] ids, Action<User[]> callback)
        {
            _queue.Enqueue(service => callback(service.GetUsersByIds(identity, ids)));
        }
    }
}
