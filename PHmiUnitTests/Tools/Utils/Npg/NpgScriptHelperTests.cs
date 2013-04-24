using System.Linq;
using NUnit.Framework;
using PHmiTools.Utils.Npg;

namespace PHmiUnitTests.Tools.Utils.Npg
{
    [TestFixture]
    public class NpgScriptHelperTests
    {
        [Test]
        public void ExtractScriptTest()
        {
            const string script = @"alter table num_trend_triggers
add constraint fk_numerictrtr_ref_digitaltag foreign key (dig_tag_id)
references dig_tags (id)
on delete cascade on update cascade;

/*==============================================================*/
/* Table: alarm_tags                                            */
/*==============================================================*/

create function alarm_tags_del()
returns trigger as
'BEGIN
delete from alarm_tags where alarm_tags.id=OLD.id;
return OLD;
END;'
language 'plpgsql';";
            var helper = new NpgScriptHelper();
            var result = helper.ExtractScriptLines(script);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(
                @"alter table num_trend_triggers add constraint fk_numerictrtr_ref_digitaltag foreign key (dig_tag_id) references dig_tags (id) on delete cascade on update cascade;",
  result.First());
            Assert.AreEqual(
                @"create function alarm_tags_del() returns trigger as 'BEGIN delete from alarm_tags where alarm_tags.id=OLD.id;return OLD;END;' language 'plpgsql';",
   result.Last());
        }
    }
}
