using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using PHmiClient.Utils;
using PHmiResources.Loc;

namespace PHmiTools.Utils.Npg
{
    public class NpgConnectionParameters : INpgConnectionParameters
    {
        private string _server;
        private string _port;
        private string _userId;
        private string _password;
        private string _database;

        private const string Pattern = @"^[^;]+$";

        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
        [RegularExpression(Pattern, ErrorMessageResourceName = "ConnectionParameterErrorMessage", ErrorMessageResourceType = typeof(Res))]
        [LocDisplayName("Server", ResourceType = typeof(Res))]
        public string Server
        {
            get { return _server; }
            set
            {
                _server = value;
                OnPropertyChanged("Server");
            }
        }

        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
        [RegularExpression(Pattern, ErrorMessageResourceName = "ConnectionParameterErrorMessage", ErrorMessageResourceType = typeof(Res))]
        [LocDisplayName("Port", ResourceType = typeof(Res))]
        public string Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }

        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
        [RegularExpression(Pattern, ErrorMessageResourceName = "ConnectionParameterErrorMessage", ErrorMessageResourceType = typeof(Res))]
        [LocDisplayName("UserId", ResourceType = typeof(Res))]
        public string UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                OnPropertyChanged("UserId");
            }
        }

        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
        [RegularExpression(Pattern, ErrorMessageResourceName = "ConnectionParameterErrorMessage", ErrorMessageResourceType = typeof(Res))]
        [LocDisplayName("Password", ResourceType = typeof(Res))]
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
        [LocDisplayName("Database", ResourceType = typeof(Res))]
        [StringLength(PHmiConstants.MaxPHmiProjectDatabaseNameLength, ErrorMessageResourceName = "LengthMessage", ErrorMessageResourceType = typeof(Res))]
        public string Database
        {
            get { return _database; }
            set
            {
                _database = value;
                OnPropertyChanged("Database");
            }
        }

        public string ConnectionString
        {
            get
            {
                return string.Format(
                    "{0}{1}",
                    ConnectionStringWithoutDatabase,
                    Database == null ? string.Empty : string.Format("Database={0}", Database));
            }
        }

        public string ConnectionStringWithoutDatabase
        {
            get
            {
                return string.Format(
                    "Server={0};Port={1};User Id={2};Password={3};Enlist=true;",
                    Server, Port, UserId, Password);
            }
        }

        public void Update(string connectionString)
        {
            Server = FindString(connectionString, "Server");
            Port = FindString(connectionString, "Port");
            UserId = FindString(connectionString, "User Id");
            Password = FindString(connectionString, "Password");
            Database = FindString(connectionString, "Database");
        }

        private static string FindString(string text, string parameter)
        {
            if (text == null)
                return null;
            var r = new Regex(parameter + @"=([^;]+)");
            var m = r.Match(text);
            return m.Success ? m.Groups[1].ToString() : null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }

        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error
        {
            get { return this.GetError(); }
        }
    }
}
