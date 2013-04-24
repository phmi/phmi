using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace PHmiClient.Users
{
    [DataContract]
    public sealed class Identity
    {
        public Identity(long id, string name, string password)
        {
            UserId = id;
            Hash = GetHash(name + password);
        }

        [DataMember]
        public long UserId { get; private set; }

        [DataMember]
        public string Hash { get; private set; }

        private static string GetHash(string value)
        {
            using (var cripto = SHA512.Create())
            {
                var bytes = cripto.ComputeHash((new UTF8Encoding()).GetBytes(value + "solt"));
                var psw = new StringBuilder();
                foreach (var b in bytes)
                {
                    psw.Append(b.ToString(CultureInfo.InvariantCulture));
                }
                return psw.ToString();
            }
        }

        public override int GetHashCode()
        {
            return UserId.GetHashCode() ^ Hash.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var i = obj as Identity;
            if (i == null)
                return false;
            return UserId.Equals(i.UserId) && Hash.Equals(i.Hash);
        }
    }
}
