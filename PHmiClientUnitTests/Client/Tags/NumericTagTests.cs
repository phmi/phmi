using NUnit.Framework;
using PHmiClient.Tags;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Client.Tags
{
    public class WhenUsingNumericTag : Specification
    {
        protected int Id;
        protected string Name;
        protected string Description;
        protected string Format;
        protected string EngUnit;
        protected double MinValue;
        protected double MaxValue;
        protected NumericTag NumericTag;
        
        protected override void EstablishContext()
        {
            base.EstablishContext();
            Id = RandomGenerator.GetRandomInt32();
            Name = "Name";
            Description = "Description";
            Format = "0.0";
            EngUnit = "EngUnit";
            MinValue = RandomGenerator.GetRandomInt32();
            MaxValue = MinValue + 100;
            NumericTag = new NumericTag(new DispatcherService(), Id, Name, () => Description, () => Format, () => EngUnit, MinValue, MaxValue);
        }

        public class ThenNameIsReturned : WhenUsingNumericTag
        {
            [Test]
            public void Test()
            {
                Assert.That(NumericTag.Name, Is.EqualTo(Name));
            }
        }

        public class ThenDescriptionIsReturned : WhenUsingNumericTag
        {
            [Test]
            public void Test()
            {
                Assert.That(NumericTag.Description, Is.EqualTo(Description));
            }
        }

        public class ThenMinValueIsReturned : WhenUsingNumericTag
        {
            [Test]
            public void Test()
            {
                Assert.That(NumericTag.MinValue, Is.EqualTo(MinValue));
            }
        }

        public class ThenMaxValueIsReturned : WhenUsingNumericTag
        {
            [Test]
            public void Test()
            {
                Assert.That(NumericTag.MaxValue, Is.EqualTo(MaxValue));
            }
        }

        public class ThenValueStringReturnsNull : WhenUsingNumericTag
        {
            [Test]
            public void Test()
            {
                Assert.That(NumericTag.ValueString, Is.Null);
            }
        }

        public class ThenSettingValueRaisesPropertyChangedForValueString : WhenUsingNumericTag
        {
            protected int Counter;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Counter = 0;
                NumericTag.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == "ValueString")
                            Counter++;
                    };
                NumericTag.Value = RandomGenerator.GetRandomInt32();
            }

            [Test]
            public void Test()
            {
                Assert.That(Counter, Is.EqualTo(1));
            }
        }

        public class AndSettingValue : WhenUsingNumericTag
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                NumericTag.Value = RandomGenerator.GetRandomInt32();
            }

            public class ThenValueStringReturnsFormattedValue : AndSettingValue
            {
                [Test]
                public void Test()
                {
                    var expected = NumericTag.Value.Value.ToString(Format) + EngUnit;
                    Assert.That(NumericTag.ValueString, Is.EqualTo(expected));
                }
            }
        }

        public class AndSettedValueMoreThanMaxValue : WhenUsingNumericTag
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                NumericTag.Value = MaxValue + 1;
            }

            public class ThenValueIsMaxValue : AndSettedValueMoreThanMaxValue
            {
                [Test]
                public void Test()
                {
                    Assert.That(NumericTag.Value, Is.EqualTo(MaxValue));
                }
            }
        }

        public class AndSettedValueLessThanMinValue : WhenUsingNumericTag
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                NumericTag.Value = MinValue - 1;
            }

            public class ThenValueIsMinValue : AndSettedValueLessThanMinValue
            {
                [Test]
                public void Test()
                {
                    Assert.That(NumericTag.Value, Is.EqualTo(MinValue));
                }
            }
        }
    }
}
