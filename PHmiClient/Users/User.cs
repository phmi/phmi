using System.ComponentModel;
using System.Runtime.Serialization;
using PHmiClient.Utils;

namespace PHmiClient.Users
{
    [DataContract]
    public sealed class User : INotifyPropertyChanged
    {
        private long _id;
        private string _name;
        private string _description;
        private byte[] _photo;
        private bool _enabled = true;
        private bool _canChange;
        private int? _privilege;

        [DataMember]
        public long Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        [DataMember]
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        [DataMember]
        public byte[] Photo
        {
            get { return _photo; }
            set
            {
                _photo = value;
                OnPropertyChanged("Photo");
            }
        }

        [DataMember]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        [DataMember]
        public bool CanChange
        {
            get { return _canChange; }
            set
            {
                _canChange = value;
                OnPropertyChanged("CanChange");
            }
        }

        [DataMember]
        public int? Privilege
        {
            get { return _privilege; }
            set
            {
                _privilege = value;
                OnPropertyChanged("Privilege");
            }
        }

        public void UpdateFrom(User user)
        {
            Id = user.Id;
            Name = user.Name;
            Description = user.Description;
            Photo = user.Photo;
            Enabled = user.Enabled;
            CanChange = user.CanChange;
            Privilege = user.Privilege;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
