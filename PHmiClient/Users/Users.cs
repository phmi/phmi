using PHmiClient.Converters;
using PHmiClient.Utils;
using System;
using System.ComponentModel;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Users
{
    public class Users : IUsers
    {
        private readonly IUsersRunTarget _usersRunTarget;
        private User _current;
        private Identity _identity;

        internal Users(IUsersRunTarget usersRunTarget)
        {
            _usersRunTarget = usersRunTarget;
        }

        public User Current
        {
            get { return _current; }
            private set
            {
                _current = value;
                OnPropertyChanged("Current");
            }
        }

        public void LogOn(string name, string password, Action<bool> callback)
        {
            var p = PasswordConverter.ConvertBack(password);
            _usersRunTarget.LogOn(name, p, user =>
                {
                    if (user != null)
                    {
                        Current = user;
                        _identity = new Identity(user.Id, user.Name, p);
                    }
                    callback.Invoke(user != null);
                });
        }

        public void LogOff()
        {
            _identity = null;
            Current = null;
        }

        public void ChangePassword(string oldPassword, string newPassword, Action<bool> callback)
        {
            var user = Current;
            if (user == null)
            {
                callback(false);
            }
            else
            {
                var p = PasswordConverter.ConvertBack(newPassword);
                _usersRunTarget.ChangePassword(
                    user.Name,
                    PasswordConverter.ConvertBack(oldPassword),
                    p,
                    result =>
                        {
                            if (result)
                            {
                                _identity = new Identity(user.Id, user.Name, p);
                            }
                            callback(result);
                        });
            }
        }

        public Identity Identity()
        {
            return _identity;
        }

        public void GetUsers(CriteriaType criteriaType, string name, int count, Action<User[]> callback)
        {
            _usersRunTarget.GetUsers(_identity, criteriaType, name, count, callback);
        }

        public void GetUsers(long[] ids, Action<User[]> callback)
        {
            _usersRunTarget.GetUsers(_identity, ids, callback);
        }

        public void SetPassword(long userId, string password, Action<bool> callback)
        {
            _usersRunTarget.SetPassword(_identity, userId, PasswordConverter.ConvertBack(password), callback);
        }

        public void UpdateUser(User user, Action<UpdateUserResult> callback)
        {
            _usersRunTarget.UpdateUser(_identity, user, callback);
        }

        public void InsertUser(User user, Action<InsertUserResult> callback)
        {
            _usersRunTarget.InsertUser(_identity, user, callback);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
