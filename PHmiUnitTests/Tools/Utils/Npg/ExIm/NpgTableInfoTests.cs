using NUnit.Framework;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.ExIm;

namespace PHmiUnitTests.Tools.Utils.Npg.ExIm
{
    [TestFixture]
    public class NpgTableInfoTests
    {
        [Test]
        public void GetPrimaryKeyIndexesTest()
        {
            var table = new NpgTableInfo
                {
                    Columns = new[]
                        {
                            new NpgColumnInfo {Name = "column0"},
                            new NpgColumnInfo {Name = "column1"},
                            new NpgColumnInfo {Name = "column2"},
                            new NpgColumnInfo {Name = "column3"},
                            new NpgColumnInfo {Name = "column4"},
                        },
                    PrimaryKey = new []
                        {
                            "column3",
                            "column1"
                        }
                };
            var result = table.GetPrimaryKeyIndexes();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(3, result[0]);
            Assert.AreEqual(1, result[1]);
        }
    }
}
