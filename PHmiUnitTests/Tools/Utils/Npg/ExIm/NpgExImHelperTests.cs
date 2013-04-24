using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.ExIm;

namespace PHmiUnitTests.Tools.Utils.Npg.ExIm
{
    [TestFixture]
    public class NpgExImHelperTests
    {
        private NpgExImHelper _helper;

        [SetUp]
        public void SetUp()
        {
            _helper = new NpgExImHelper();
        }

        #region GetColunns

        [Test]
        public void GetColumnsReturnsEmptyIfEmptyIsPassed()
        {
            Assert.That(_helper.GetColumns(string.Empty), Is.Empty);
        }

        [Test]
        public void GetColumnsSerialTest()
        {
            var result = _helper.GetColumns("id serial not null");
            var column = result.Single();
            Assert.That(column.DataType, Is.EqualTo(NpgDataType.Serial));
        }

        [Test]
        public void GetColumnsNotNullTest()
        {
            var result = _helper.GetColumns("id serial not null");
            var column = result.Single();
            Assert.That(column.NotNull, Is.True);
        }

        [Test]
        public void GetColumnsNullTest()
        {
            var result = _helper.GetColumns("id serial null");
            var column = result.Single();
            Assert.That(column.NotNull, Is.False);
        }

        [Test]
        public void GetColumnsNameTest()
        {
            var result = _helper.GetColumns("id serial null");
            var column = result.Single();
            Assert.That(column.Name, Is.EqualTo("id"));
        }

        [Test]
        public void GetColumnsTwoRowsTest()
        {
            var result = _helper.GetColumns("id serial not null, name  text not null");
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("id"));
            Assert.That(result[1].Name, Is.EqualTo("name"));
        }

        #endregion

        #region GetPrimaryKey

        [Test]
        public void GetPrimaryKeysReturnsEmptyIfEmptyIsPassed()
        {
            var result = _helper.GetPrimaryKey(string.Empty);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetPrimaryKeysTest()
        {
            var result = _helper.GetPrimaryKey("constraint pk_alarm_categories primary key (id, name)");
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.Contains("id", result);
            Assert.Contains("name", result);
        }

        #endregion

        #region GetTable

        [Test]
        public void GetTableReturnsNullIfItIsNotTable()
        {
            var result = _helper.GetTable(string.Empty);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetTableTest()
        {
            var result = _helper.GetTable(
@"create table alarm_tags (
id                   serial not null,
name                 text                 not null,
ref_dig_tags         int4                 not null
constraint pk_alarm_tags primary key (id)
);");
            Assert.AreEqual("alarm_tags", result.Name);
            Assert.AreEqual(new [] { "id" }, result.PrimaryKey);
            Assert.AreEqual(3, result.Columns.Length);
        }
        #endregion

        #region GetConstraint

        [Test]
        public void GetConstraintReturnsNullIfItIsNotConstraint()
        {
            var result = _helper.GetConstraint(string.Empty);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetConstraintTest()
        {
            var result = _helper.GetConstraint(
@"alter table trend_tags
   add constraint fk_trend_ta_reference_num_tags foreign key (ref_num_tags)
      references num_tags (id)
      on delete cascade on update cascade;");
            Assert.AreEqual("trend_tags", result.TableName);
            Assert.AreEqual("num_tags", result.ReferencedTable);
        }

        #endregion

        #region GetTables

        [Test]
        public void GetTablesIfEmptyIsPassed()
        {
            var result = _helper.GetTables(new string[0]);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetTablesReturnsTables()
        {
            var script = new []
                {
                    @"create table alarm_tags (
id                   serial not null,
ref_dig_tags         int4                 not null,
constraint pk_alarm_tags primary key (id)
);",
   @"create table dig_tags (
id                   serial not null,
ref_io_devices       int4                 not null,
constraint pk_dig_tags primary key (id)
);",
   @"create table io_devices (
id                   serial not null,
constraint pk_io_devices primary key (id)
);"
                };
            var result = _helper.GetTables(script);
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("alarm_tags", result[0].Name);
            Assert.AreEqual("dig_tags", result[1].Name);
            Assert.AreEqual("io_devices", result[2].Name);
        }

        [Test]
        public void GetTablesReturnsOrderedByConstraintsTables()
        {
            var script = new[]
                {
                    @"create table alarm_tags (
id                   serial not null,
ref_dig_tags         int4                 not null,
constraint pk_alarm_tags primary key (id)
);",
   @"create table dig_tags (
id                   serial not null,
ref_io_devices       int4                 not null,
constraint pk_dig_tags primary key (id)
);",
   @"create table io_devices (
id                   serial not null,
constraint pk_io_devices primary key (id)
);",
   @"alter table alarm_tags
   add constraint fk_alarm_ta_reference_dig_tags foreign key (ref_dig_tags)
      references dig_tags (id)
      on delete cascade on update cascade;",
   @"alter table dig_tags
   add constraint fk_dig_tags_reference_io_devic foreign key (ref_io_devices)
      references io_devices (id)
      on delete cascade on update cascade;"
                };
            var result = _helper.GetTables(script);
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("alarm_tags", result[2].Name);
            Assert.AreEqual("dig_tags", result[1].Name);
            Assert.AreEqual("io_devices", result[0].Name);
        }

        #endregion

        #region GetSelectScriptItem

        [Test]
        public void GetSelectScriptItemTest()
        {
            var columns = new[]
                {
                    new NpgColumnInfo
                        {
                            Name = "id",
                            DataType = NpgDataType.Serial,
                            NotNull = true
                        },
                    new NpgColumnInfo
                        {
                            Name = "name",
                            DataType = NpgDataType.Text,
                            NotNull = false
                        },
                    new NpgColumnInfo
                        {
                            Name = "data",
                            DataType = NpgDataType.Int4,
                            NotNull = false
                        }
                };
            var primaryKey = new[]
                {
                    "id", "name"
                };
            var tableInfo = new NpgTableInfo
                {
                    Name = "tableName",
                    Columns = columns,
                    PrimaryKey = primaryKey
                };
            var result = _helper.GetSelectScriptItem(tableInfo, 1000);
            Assert.AreEqual(
                "SELECT \"id\", \"name\", \"data\" FROM \"tableName\" ORDER BY \"id\", \"name\" LIMIT 1000",
                result.Text);
            Assert.IsEmpty(result.Parameters);
        }

        [Test]
        public void GetSelectScriptItemTestWithParameters()
        {
            var columns = new[]
                {
                    new NpgColumnInfo
                        {
                            Name = "id",
                            DataType = NpgDataType.Serial,
                            NotNull = true
                        },
                    new NpgColumnInfo
                        {
                            Name = "name",
                            DataType = NpgDataType.Text,
                            NotNull = false
                        },
                    new NpgColumnInfo
                        {
                            Name = "data",
                            DataType = NpgDataType.Int4,
                            NotNull = false
                        }
                };
            var primaryKey = new[]
                {
                    "id", "name"
                };
            var tableInfo = new NpgTableInfo
            {
                Name = "tableName",
                Columns = columns,
                PrimaryKey = primaryKey
            };
            var result = _helper.GetSelectScriptItem(
                tableInfo,
                1000,
                new KeyValuePair<string, object>("id", 10),
                new KeyValuePair<string, object>("name", "abc"));
            Assert.AreEqual(
                "SELECT \"id\", \"name\", \"data\" FROM \"tableName\" WHERE \"id\" > :id AND \"name\" > :name ORDER BY \"id\", \"name\" LIMIT 1000",
                result.Text);
            Assert.AreEqual(2, result.Parameters.Length);
            Assert.AreEqual("id", result.Parameters[0].ParameterName);
            Assert.AreEqual(10, result.Parameters[0].Value);
            Assert.AreEqual("name", result.Parameters[1].ParameterName);
            Assert.AreEqual("abc", result.Parameters[1].Value);
        }

        #endregion

        #region GetInsertScriptItem

        [Test]
        public void GetInsertScriptItemEmptyTest()
        {
            var tableData = new TableData
            {
                TableName = "Table",
                Columns = new[] { "id", "name", "data" },
                Data = new object[0][]
            };
            var result = _helper.GetInsertScriptItem(tableData);
            Assert.IsNull(result);
        }

        [Test]
        public void GetInsertScriptItemTest()
        {
            var tableData = new TableData
                {
                    TableName = "Table",
                    Columns = new[] {"id", "name", "data"},
                    Data = new[]
                        {
                            new object[] {1, "name1", "data1"}
                        }
                };
            var result = _helper.GetInsertScriptItem(tableData);
            Assert.AreEqual(
                "INSERT INTO \"Table\" (\"id\", \"name\", \"data\") VALUES (:id0, :name0, :data0)",
                result.Text);
            Assert.AreEqual(3, result.Parameters.Length);
            Assert.AreEqual("id0", result.Parameters[0].ParameterName);
            Assert.AreEqual(1, result.Parameters[0].Value);
            Assert.AreEqual("name0", result.Parameters[1].ParameterName);
            Assert.AreEqual("name1", result.Parameters[1].Value);
            Assert.AreEqual("data0", result.Parameters[2].ParameterName);
            Assert.AreEqual("data1", result.Parameters[2].Value);
        }

        [Test]
        public void GetInsertScriptItemMultipleRowsTest()
        {
            var tableData = new TableData
            {
                TableName = "Table",
                Columns = new[] { "id", "name", "data" },
                Data = new[]
                        {
                            new object[] {1, "name1", "data1"},
                            new object[] {2, "name2", "data2"}
                        }
            };
            var result = _helper.GetInsertScriptItem(tableData);
            Assert.AreEqual(
                "INSERT INTO \"Table\" (\"id\", \"name\", \"data\") VALUES (:id0, :name0, :data0), (:id1, :name1, :data1)",
                result.Text);
            Assert.AreEqual(6, result.Parameters.Length);
            Assert.AreEqual("id0", result.Parameters[0].ParameterName);
            Assert.AreEqual(1, result.Parameters[0].Value);
            Assert.AreEqual("name0", result.Parameters[1].ParameterName);
            Assert.AreEqual("name1", result.Parameters[1].Value);
            Assert.AreEqual("data0", result.Parameters[2].ParameterName);
            Assert.AreEqual("data1", result.Parameters[2].Value);
            Assert.AreEqual("id1", result.Parameters[3].ParameterName);
            Assert.AreEqual(2, result.Parameters[3].Value);
            Assert.AreEqual("name1", result.Parameters[4].ParameterName);
            Assert.AreEqual("name2", result.Parameters[4].Value);
            Assert.AreEqual("data1", result.Parameters[5].ParameterName);
            Assert.AreEqual("data2", result.Parameters[5].Value);
        }

        #endregion
    }
}
