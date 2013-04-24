using NUnit.Framework;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.ExIm;

namespace PHmiUnitTests.Tools.Utils.Npg.ExIm
{
    [TestFixture]
    public class SerialHelperTests
    {
        private SerialHelper _helper; 

        [SetUp]
        public void SetUp()
        {
            var tables = new[]
                {
                    new NpgTableInfo
                        {
                            Name = "Table",
                            Columns = new[]
                                {
                                    new NpgColumnInfo
                                        {
                                            Name = "serial",
                                            DataType = NpgDataType.Serial
                                        },
                                    new NpgColumnInfo
                                        {
                                            Name = "id",
                                            DataType = NpgDataType.Float8
                                        }
                                }
                        },
                    new NpgTableInfo
                        {
                            Name = "Table2",
                            Columns = new[]
                                {
                                    new NpgColumnInfo
                                        {
                                            Name = "id",
                                            DataType = NpgDataType.Int4
                                        },
                                    new NpgColumnInfo
                                        {
                                            Name = "bigserial",
                                            DataType = NpgDataType.Bigserial
                                        }
                                }
                        }
                };
            _helper = SerialHelper.Create(tables);
        }

        [Test]
        public void CreateTest()
        {
            Assert.AreEqual(2, _helper.Items.Count);
            Assert.AreEqual(1, _helper.Items["Table"].Count);
            Assert.AreEqual("serial", _helper.Items["Table"]["serial"].Column);
            Assert.AreEqual("Table", _helper.Items["Table"]["serial"].Table);
            Assert.AreEqual(0, _helper.Items["Table"]["serial"].MaxValue);
            Assert.AreEqual(1, _helper.Items["Table2"].Count);
            Assert.AreEqual("bigserial", _helper.Items["Table2"]["bigserial"].Column);
            Assert.AreEqual("Table2", _helper.Items["Table2"]["bigserial"].Table);
            Assert.AreEqual(0, _helper.Items["Table2"]["bigserial"].MaxValue);
        }

        [Test]
        public void UpdateTest()
        {
            var tableData = new TableData
                {
                    Columns = new[] {"id", "serial", "papa"},
                    TableName = "Table",
                    Data = new[]
                        {
                            new object[] {1, 10, "papa"},
                            new object[] {2, 20, "papa"},
                            new object[] {2, 15, "papa"}
                        }
                };
            _helper.Update(tableData);
            Assert.AreEqual(20, _helper.Items["Table"]["serial"].MaxValue);
        }

        [Test]
        public void UpdateWithLessValueTest()
        {
            var tableData = new TableData
                {
                    Columns = new[] { "id", "serial", "papa" },
                    TableName = "Table",
                    Data = new[]
                            {
                                new object[] {1, 10, "papa"},
                                new object[] {2, 20, "papa"},
                                new object[] {2, 15, "papa"}
                            }
                };
            _helper.Items["Table"]["serial"].MaxValue = 21;
            _helper.Update(tableData);
            Assert.AreEqual(21, _helper.Items["Table"]["serial"].MaxValue);
        }

        [Test]
        public void CreateAlterScriptItemTest()
        {
            _helper.Items["Table"]["serial"].MaxValue = 21;
            _helper.Items["Table2"]["bigserial"].MaxValue = 22;
            var items = _helper.CreateAlterScriptItem();
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual("ALTER SEQUENCE Table_serial_seq RESTART WITH 22", items[0].Text);
            Assert.IsEmpty(items[0].Parameters);
            Assert.AreEqual("ALTER SEQUENCE Table2_bigserial_seq RESTART WITH 23", items[1].Text);
            Assert.IsEmpty(items[1].Parameters);
        }
    }
}
