using System;
using NUnit.Framework;
using PHmiClientUnitTests;
using PHmiModel;
using PHmiModel.Entities;
using PHmiRunner.Utils.IoDeviceRunner;

namespace PHmiUnitTests.Runner.Utils.IoDeviceRunner
{
    public class WhenUsingNumTagValueConverter : Specification
    {
        public class WithNullBounds : WhenUsingNumTagValueConverter
        {
            protected NumTag NumTag;
            protected NumTagValueConverter Converter;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                NumTag = new NumTag
                    {
                        NumTagType = new NumTagType { Name = "Int32" }
                    };
                Converter = new NumTagValueConverter(NumTag);
            }

            public class AndRawValueNull : WithNullBounds
            {
                public class ThenRawToEngReturnsNull : AndRawValueNull
                {
                    protected Double? EngValue;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        EngValue = Converter.RawToEng(null);
                    }
 
                    [Test]
                    public void Test()
                    {
                        Assert.That(EngValue, Is.Null);
                    }
                }
            }

            public class ThenRawToEngReturnsRawValue : WithNullBounds
            {
                protected Int32 RawValue;
                protected Double? EngValue;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    RawValue = new Random().Next();
                    EngValue = Converter.RawToEng(RawValue);
                }

                [Test]
                public void Test()
                {
                    Assert.That(EngValue, Is.EqualTo(RawValue));
                }
            }

            public class ThenEngToRawReturnsEngValue : WithNullBounds
            {
                protected object RawValue;
                protected Double EngValue;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    EngValue = new Random().Next();
                    RawValue = Converter.EngToRaw(EngValue);
                }

                [Test]
                public void Test()
                {
                    Assert.That(RawValue, Is.TypeOf<Int32>());
                    Assert.That(RawValue, Is.EqualTo(EngValue));
                }
            }
        }

        public class WithNotNullBounds : WhenUsingNumTagValueConverter
        {
            protected NumTag NumTag;
            protected NumTagValueConverter Converter;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                NumTag = new NumTag
                {
                    NumTagType = new NumTagType { Name = "Int32" },
                    RawMinDb = 0,
                    RawMaxDb = 100,
                    EngMinDb = 0,
                    EngMaxDb = 10
                };
                Converter = new NumTagValueConverter(NumTag);
            }

            public class AndRawValueNull : WithNotNullBounds
            {
                public class ThenRawToEngReturnsNull : AndRawValueNull
                {
                    protected Double? EngValue;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        EngValue = Converter.RawToEng(null);
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That(EngValue, Is.Null);
                    }
                }
            }

            public class ThenRawToEngReturnsCalculatedRawValue : WithNotNullBounds
            {
                protected Int32 RawValue;
                protected Double? EngValue;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    RawValue = 33;
                    EngValue = Converter.RawToEng(RawValue);
                }

                [Test]
                public void Test()
                {
                    Assert.That(EngValue, Is.EqualTo(3.3));
                }
            }

            public class ThenEngToRawReturnsEngValue : WithNotNullBounds
            {
                protected object RawValue;
                protected Double EngValue;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    EngValue = 3.3;
                    RawValue = Converter.EngToRaw(EngValue);
                }

                [Test]
                public void Test()
                {
                    Assert.That(RawValue, Is.TypeOf<Int32>());
                    Assert.That(RawValue, Is.EqualTo(33));
                }
            }
        }
    }
}
