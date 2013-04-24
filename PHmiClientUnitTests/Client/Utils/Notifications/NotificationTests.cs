using NUnit.Framework;
using System;
using PHmiClient.Utils.Notifications;

namespace PHmiClientUnitTests.Client.Utils.Notifications
{
    public class WhenUsingNotification : Specification
    {
        public class ThenINotifyPropertyChangedShouldBeImplemented : WhenUsingNotification
        {
            private Notification _notification;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                _notification = new Notification(DateTime.UtcNow, "message", "shortDescription", "longDescription");
            }

            [Test]
            public void EndTimeSetRaisesPropertyChanged()
            {
                NotifyPropertyChangedTester.Test(_notification, m => m.EndTime, DateTime.UtcNow);
            }
        }

        public class AndCreatingItWithNullLongDescription : WhenUsingNotification
        {
            protected Notification Notification;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Notification = new Notification(DateTime.UtcNow, "message", "shortDescription", null);
            }

            public class ThenLongDescriptionShouldReturnShortDescription : AndCreatingItWithNullLongDescription
            {
                [Test]
                public void Test()
                {
                    Assert.That(Notification.LongDescription, Is.EqualTo(Notification.ShortDescription));
                }
            }
        }
    }
}
