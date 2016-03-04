using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PHmiClient.Utils;
using PHmiResources.Loc;

namespace PHmiModel.Entities
{
    [MetadataType(typeof(UserMetadata))]
    [Table("users", Schema = "public")]
    public class User : NamedEntity
    {
        public User()
        {
            Enabled = true;
        }

        public class UserMetadata : EntityMetadataBase
        {
            private string _name;
            private byte[] _photo;
            private string _description;
            private int? _privilege;
            private bool _canChange;
            private bool _enabled;
            private string _password;

            [LocDisplayName("Name", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public string Name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    OnPropertyChanged(this, m => m.Name);
                }
            }

            [LocDisplayName("Photo", ResourceType = typeof(Res))]
            public byte[] Photo
            {
                get { return _photo; }
                set
                {
                    _photo = value;
                    OnPropertyChanged(this, m => m.Photo);
                }
            }

            [LocDisplayName("Description", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public string Description
            {
                get { return _description; }
                set
                {
                    _description = value;
                    OnPropertyChanged(this, m => m.Description);
                }
            }

            [LocDisplayName("Password", ResourceType = typeof(Res))]
            public string Password
            {
                get { return _password; }
                set
                {
                    _password = value;
                    OnPropertyChanged(this, m => m.Password);
                }
            }

            [LocDisplayName("UserEnabled", ResourceType = typeof(Res))]
            public bool Enabled
            {
                get { return _enabled; }
                set
                {
                    _enabled = value;
                    OnPropertyChanged(this, m => m.Enabled);
                }
            }

            [LocDisplayName("CanChange", ResourceType = typeof(Res))]
            public bool CanChange
            {
                get { return _canChange; }
                set
                {
                    _canChange = value;
                    OnPropertyChanged(this, m => m.CanChange);
                }
            }

            [LocDisplayName("Privilege", ResourceType = typeof(Res))]
            public int? Privilege
            {
                get { return _privilege; }
                set
                {
                    _privilege = value;
                    OnPropertyChanged(this, m => m.Privilege);
                }
            }
        }

        #region Description

        private string _description;

        [Column("description")]
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value; 
                OnPropertyChanged(this, e => e.Description);
            }
        }

        #endregion

        #region Photo

        private byte[] _photo;

        [Column("photo")]
        public byte[] Photo
        {
            get { return _photo; }
            set
            {
                _photo = value; 
                OnPropertyChanged(this, e => e.Photo);
            }
        }

        #endregion

        #region Password

        private string _password;

        [Column("password")]
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(this, e => e.Password);
            }
        }

        #endregion

        #region Enabled

        private bool _enabled;

        [Column("enabled")]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged(this, e => e.Enabled);
            }
        }

        #endregion

        #region CanChange

        private bool _canChange;

        [Column("can_change")]
        public bool CanChange
        {
            get { return _canChange; }
            set
            {
                _canChange = value;
                OnPropertyChanged(this, e => e.CanChange);
            }
        }

        #endregion

        #region Privilege

        private int? _privilege;

        [Column("privilege")]
        public int? Privilege
        {
            get { return _privilege; }
            set
            {
                _privilege = value;
                OnPropertyChanged(this, e => e.Privilege);
            }
        }

        #endregion
    }
}
