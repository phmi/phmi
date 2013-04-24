using Moq;
using NUnit.Framework;
using PHmiClient.Tags;
using PHmiClient.Utils;
using PHmiClient.Wcf.ServiceTypes;
using System;
using System.Globalization;

namespace PHmiClientUnitTests.Client.Tags
{
    public class WhenUsingIoDevice : Specification
    {
        protected int Id;
        protected string Name;
        protected IoDeviceStub IoDevice;

        protected class IoDeviceStub : IoDeviceBase
        {
            public IoDeviceStub(int id, string name) : base(id, name)
            {
            }

            public IDigitalTag AddDigTag(TagAbstract<bool?> tag)
            {
                Add(tag);
                return tag as IDigitalTag;
            }

            public INumericTag AddNumTag(TagAbstract<double?> tag)
            {
                Add(tag);
                return tag as INumericTag;
            }
        }

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Id = RandomGenerator.GetRandomInt32();
            Name = RandomGenerator.GetRandomInt32().ToString(CultureInfo.InvariantCulture);
            IoDevice = new IoDeviceStub(Id, Name);
        }

        public class AndAddedTags : WhenUsingIoDevice
        {
            protected int ReadDigTagId;
            protected Mock<TagAbstract<bool?>> ReadDigTag;
            protected int DigitalTagId;
            protected Mock<TagAbstract<bool?>> DigitalTag;
            protected int NumericTagId;
            protected Mock<TagAbstract<double?>> NumericTag;
            protected int ReadNumericTagId;
            protected Mock<TagAbstract<double?>> ReadNumericTag;

            protected override void EstablishContext()
            {
                base.EstablishContext();

                ReadDigTag = new Mock<TagAbstract<bool?>>();
                ReadDigTagId = RandomGenerator.GetRandomInt32(int.MinValue + 1);
                ReadDigTag.SetupGet(t => t.Id).Returns(ReadDigTagId);
                IoDevice.AddDigTag(ReadDigTag.Object);

                DigitalTag = new Mock<TagAbstract<bool?>>();
                DigitalTagId = ReadDigTagId - 1;
                DigitalTag.SetupGet(t => t.Id).Returns(DigitalTagId);
                IoDevice.AddDigTag(DigitalTag.Object);

                NumericTag = new Mock<TagAbstract<double?>>();
                NumericTagId = RandomGenerator.GetRandomInt32(int.MinValue + 1);
                NumericTag.SetupGet(t => t.Id).Returns(NumericTagId);
                IoDevice.AddNumTag(NumericTag.Object);

                ReadNumericTag = new Mock<TagAbstract<double?>>();
                ReadNumericTagId = NumericTagId - 1;
                ReadNumericTag.SetupGet(t => t.Id).Returns(ReadNumericTagId);
                IoDevice.AddNumTag(ReadNumericTag.Object);
            }

            public class AndNothingElseOnTags : AndAddedTags
            {
                public class ThenCreateRemapParameterReturnsNull : AndNothingElseOnTags
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(IoDevice.CreateRemapParameter(), Is.Null);
                    }
                } 
            }

            public class AndTagsAreRead : AndAddedTags
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ReadDigTag.SetupGet(t => t.IsRead).Returns(true);
                    ReadNumericTag.SetupGet(t => t.IsRead).Returns(true);
                }

                public class AndCallingCreateRemapParameter : AndTagsAreRead
                {
                    internal RemapTagsParameter Parameter;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Parameter = IoDevice.CreateRemapParameter();
                    }

                    public class ThenIoDeviceIdIsPassed : AndCallingCreateRemapParameter
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Parameter.IoDeviceId, Is.EqualTo(Id));
                        }
                    }

                    public class ThenItContainsTagsInfo : AndCallingCreateRemapParameter
                    {
                        [Test]
                        public void ForDigitalTagTest()
                        {
                            Assert.That(Parameter.DigReadIds, Contains.Item(ReadDigTagId));
                        }

                        [Test]
                        public void ForNumericTagTest()
                        {
                            Assert.That(Parameter.NumReadIds, Contains.Item(ReadNumericTagId));
                        }
                    }

                    public class ThenItDoesNotContainTagsInfo : AndCallingCreateRemapParameter
                    {
                        [Test]
                        public void ForUnreadDigitalTagTest()
                        {
                            Assert.That(Parameter.DigReadIds, Is.Not.Contains(DigitalTagId));
                        }

                        [Test]
                        public void ForUnreadNumericTagTest()
                        {
                            Assert.That(Parameter.NumReadIds, Is.Not.Contains(NumericTagId));
                        }
                    }

                    public class AndApplyRemapResultInvoked : AndCallingCreateRemapParameter
                    {
                        internal RemapTagsResult Result;

                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Result = new RemapTagsResult
                            {
                                DigReadValues = new bool?[] { false },
                                NumReadValues = new double?[] { RandomGenerator.GetRandomInt32() }
                            };
                            IoDevice.ApplyRemapResult(Result);
                        }

                        public class ThenValuesUpdated : AndApplyRemapResultInvoked
                        {
                            [Test]
                            public void ForDigitalTag()
                            {
                                ReadDigTag.Verify(t => t.UpdateValue(Result.DigReadValues[0]), Times.Once());
                            }

                            [Test]
                            public void ForNumericTag()
                            {
                                ReadNumericTag.Verify(t => t.UpdateValue(Result.NumReadValues[0]), Times.Once());
                            }
                        }
                    }

                    public class AndApplyRemapResultWithNull : AndCallingCreateRemapParameter
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            IoDevice.ApplyRemapResult(null);
                        }

                        public class ThenValuesUpdatedWithNull : AndApplyRemapResultWithNull
                        {
                            [Test]
                            public void ForDigitalTag()
                            {
                                ReadDigTag.Verify(t => t.UpdateValue(null), Times.Once());
                            }

                            [Test]
                            public void ForNumericTag()
                            {
                                ReadNumericTag.Verify(t => t.UpdateValue(null), Times.Once());
                            }
                        }
                    }

                    public class ThenUnreadTagsAreUpdatedWithNull : AndCallingCreateRemapParameter
                    {
                        [Test]
                        public void ForUnreadDigitalTag()
                        {
                            DigitalTag.Verify(t => t.UpdateValue(null), Times.Once());
                        }

                        [Test]
                        public void ForUnreadNumericTag()
                        {
                            NumericTag.Verify(t => t.UpdateValue(null), Times.Once());
                        }
                    }
                }
            }

            public class AndTagsAreWritten : AndAddedTags
            {
                protected double? NumericValue;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ReadDigTag.SetupGet(t => t.IsWritten).Returns(true);
                    ReadDigTag.Setup(t => t.GetWrittenValue()).Returns(true);
                    ReadNumericTag.SetupGet(t => t.IsWritten).Returns(true);
                    NumericValue = RandomGenerator.GetRandomInt32();
                    ReadNumericTag.Setup(t => t.GetWrittenValue()).Returns(NumericValue);
                }

                public class AndCreateRemapParameterInvoked : AndTagsAreWritten
                {
                    internal RemapTagsParameter Parameter;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Parameter = IoDevice.CreateRemapParameter();
                    }

                    public class ThenParameterContainsWriteData : AndCreateRemapParameterInvoked
                    {
                        [Test]
                        public void ForDigitalTag()
                        {
                            Assert.That(Parameter.DigWriteIds, Contains.Item(ReadDigTagId));
                            Assert.That(Parameter.DigWriteValues[Array.IndexOf(Parameter.DigWriteIds, ReadDigTagId)], Is.EqualTo(true));
                        }

                        [Test]
                        public void ForNumericTag()
                        {
                            Assert.That(Parameter.NumWriteIds, Contains.Item(ReadNumericTagId));
                            Assert.That(Parameter.NumWriteValues[Array.IndexOf(Parameter.NumWriteIds, ReadNumericTagId)], Is.EqualTo(NumericValue));
                        }
                    }
                }
            }
        }

        public class AndAddedInterfaceTags : WhenUsingIoDevice
        {
            protected DigitalTag DigitalTag;
            protected NumericTag NumericTag;
            protected IDigitalTag ReturnedDigitalTag;
            protected INumericTag ReturnedNumericTag;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                DigitalTag = new DigitalTag(new DispatcherService(), 1, "name", () => "");
                ReturnedDigitalTag = IoDevice.AddDigTag(DigitalTag);
                NumericTag = new NumericTag(new DispatcherService(), 1, "name", () => "", () => "", () => "", 0, 0);
                ReturnedNumericTag = IoDevice.AddNumTag(NumericTag);
            }

            public class ThenDigitalTagReturned : AndAddedInterfaceTags
            {
                [Test]
                public void Test()
                {
                    Assert.That(ReturnedDigitalTag, Is.SameAs(DigitalTag));
                }
            }

            public class ThenNumericTagReturned : AndAddedInterfaceTags
            {
                [Test]
                public void Test()
                {
                    Assert.That(ReturnedNumericTag, Is.SameAs(NumericTag));
                }
            }
        }

        public class ThenNameReturnsName : WhenUsingIoDevice
        {
            [Test]
            public void Test()
            {
                Assert.That(IoDevice.Name, Is.EqualTo(Name));
            }
        }
    }
}
