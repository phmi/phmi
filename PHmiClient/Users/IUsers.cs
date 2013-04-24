using System;
using System.ComponentModel;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Users
{
    public interface IUsers : INotifyPropertyChanged
    {
        User Current { get; }
        void LogOn(string name, string password, Action<bool> callback);
        void LogOff();
        void ChangePassword(string oldPassword, string newPassword, Action<bool> callback);
        Identity Identity();
        void GetUsers(CriteriaType criteriaType, string name, int count, Action<User[]> callback);
        void GetUsers(long[] ids, Action<User[]> callback);
        void SetPassword(long userId, string password, Action<bool> callback);
        void UpdateUser(User user, Action<UpdateUserResult> callback);
        void InsertUser(User user, Action<InsertUserResult> callback);
    }
}
