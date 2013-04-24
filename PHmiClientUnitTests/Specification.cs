using System;
using NUnit.Framework;

namespace PHmiClientUnitTests
{
    public abstract class Specification
    {
        protected Exception ThrownException;

        protected bool CatchExceptionInEstablishContext = false;

        [SetUp]
        public void SetUp()
        {
            ThrownException = null;
            try
            {
                EstablishContext();
            }
            catch (Exception exception)
            {
                ThrownException = exception;
                if (!CatchExceptionInEstablishContext)
                    throw;
            }
        }

        protected virtual void EstablishContext()
        {

        }

        [TearDown]
        public void TearDown()
        {
            AfterEach();
        }

        protected virtual void AfterEach()
        {
            
        }
    }
}
