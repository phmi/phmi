using System;
using NUnit.Framework;
using PHmiTools.Utils.Npg;

namespace PHmiUnitTests.Tools.Utils.Npg
{
    [TestFixture]
    public class NpgConnectionParametersTests
    {
        [Test]
        public void ConnectionStringTest()
        {
            var parameters = new NpgConnectionParameters
                {
                    Server = "server", Port = "port", UserId = "user id", Password = "password", Database = "database"
                };
            Assert.AreEqual(
                "Server=server;Port=port;User Id=user id;Password=password;Enlist=true;Database=database",
                parameters.ConnectionString);
        }

        [Test]
        public void ConnectionStringWithoutDatabaseTest()
        {
            var parameters = new NpgConnectionParameters
                {
                    Server = "server", Port = "port", UserId = "user id", Password = "password", Database = "database"
                };
            Assert.AreEqual(
                "Server=server;Port=port;User Id=user id;Password=password;Enlist=true;",
                parameters.ConnectionStringWithoutDatabase);
        }

        #region StringToParameters

        [Test]
        public void StringToParametersTest()
        {
            var p = new NpgConnectionParameters();
            p.Update("Server=server;Port=port;User Id=user id;Password=password;Enlist=true;Database=database");
            Assert.AreEqual("server", p.Server);
            Assert.AreEqual("port", p.Port);
            Assert.AreEqual("user id", p.UserId);
            Assert.AreEqual("password", p.Password);
            Assert.AreEqual("database", p.Database);
        }

        [Test]
        public void StringToParametersDoesNotContainDatabaseIfItIsNotPresent()
        {
            var p = new NpgConnectionParameters();
            p.Update("Server=server;Port=port;User Id=user id;Password=password;Enlist=true;");
            Assert.AreEqual(null, p.Database);
        }

        [Test]
        public void StringToParametersReturnsEmptyParametersIfStringIsNull()
        {
            var p = new NpgConnectionParameters();
            p.Update(null);
            Assert.AreEqual(null, p.Server);
            Assert.AreEqual(null, p.Port);
            Assert.AreEqual(null, p.UserId);
            Assert.AreEqual(null, p.Password);
            Assert.AreEqual(null, p.Database);
        }

        #endregion

        #region PropertyChanged

        public void SetRaisesPropertyChangedTest(Action<NpgConnectionParameters> setAction, string property)
        {
            var p = new NpgConnectionParameters();
            var raised = false;
            string eventProperty = null;
            p.PropertyChanged += (sender, args) =>
                {
                    raised = true;
                    eventProperty = args.PropertyName;
                };
            setAction.Invoke(p);
            Assert.IsTrue(raised);
            Assert.AreEqual(property, eventProperty);
        }

        [Test]
        public void ServerSetRaisesPropertyChanged()
        {
            SetRaisesPropertyChangedTest(p => p.Server = null, "Server");
        }

        [Test]
        public void PortSetRaisesPropertyChanged()
        {
            SetRaisesPropertyChangedTest(p => p.Port = null, "Port");
        }

        [Test]
        public void UserIdSetRaisesPropertyChanged()
        {
            SetRaisesPropertyChangedTest(p => p.UserId = null, "UserId");
        }

        [Test]
        public void PasswordIdSetRaisesPropertyChanged()
        {
            SetRaisesPropertyChangedTest(p => p.Password = null, "Password");
        }

        [Test]
        public void DatabaseSetRaisesPropertyChanged()
        {
            SetRaisesPropertyChangedTest(p => p.Database = null, "Database");
        }

        #endregion
    }
}
