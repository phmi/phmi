using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PHmiClient.Utils;
using PHmiResources;
using PHmiResources.Loc;

namespace PHmiModel.Entities
{
    [MetadataType(typeof(SettingsMetadata))]
    [Table("settings", Schema = "public")]
    public class Settings : Entity
    {
        public class SettingsMetadata : EntityMetadataBase
        {
            private string _server;
            private string _standByServer;

            [LocDisplayName("Server", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.Server, ErrorMessageResourceName = "ServerNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string Server
            {
                get { return _server; }
                set
                {
                    _server = value;
                    OnPropertyChanged(this, m => m.Server);
                }
            }

            [LocDisplayName("StandByServer", ResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.Server, ErrorMessageResourceName = "ServerNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string StandByServer
            {
                get { return _standByServer; }
                set
                {
                    _standByServer = value;
                    OnPropertyChanged(this, m => m.StandByServer);
                }
            }
        }

        #region Server

        private string _server;

        [Column("server")]
        public string Server
        {
            get { return _server; }
            set
            {
                _server = value; 
                OnPropertyChanged(this, e => e.Server);
            }
        }

        #endregion

        #region StandByServer

        private string _standByServer;

        [Column("stand_by_server")]
        public string StandByServer
        {
            get { return _standByServer; }
            set
            {
                _standByServer = value;
                OnPropertyChanged(this, e => e.StandByServer);
            }
        }

        #endregion

        #region Guid

        private string _guid;

        [Column("guid")]
        public string Guid
        {
            get { return _guid; }
            set
            {
                _guid = value;
                OnPropertyChanged(this, e => e.Guid);
            }
        }

        #endregion

        #region Guid2

        private string _guid2;

        [Column("guid2")]
        public string Guid2
        {
            get { return _guid2; }
            set
            {
                _guid2 = value;
                OnPropertyChanged(this, e => e.Guid2);
            }
        }

        #endregion

        #region PhmiGuid

        private string _phmiGuid;

        [Column("phmi_guid")]
        public string PhmiGuid
        {
            get { return _phmiGuid; }
            set
            {
                _phmiGuid = value;
                OnPropertyChanged(this, e => e.PhmiGuid);
            }
        }

        #endregion
    }
}
