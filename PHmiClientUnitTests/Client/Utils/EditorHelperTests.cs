using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Client.Utils
{
    [TestFixture]
    public class EditorHelperTests
    {
        #region Stubs

        private class Meta
        {
            public int Int { get; set; }
            public string Str { get; set; }
            public List<int> Collection { get; set; }
            public byte[] Array { get; set; }
        }

        [MetadataType(typeof(Meta))]
        private class Stub
        {
            private byte[] _array;
            public int Int { get; set; }
            public string Str { get; set; }
            public List<int> Collection { get; set; }
            public byte[] Array
            {
                get { return _array; }
                set
                {
                    _array = value;
                    ArrayChanged = true;
                }
            }

            public bool ArrayChanged;
        }

        #endregion

        private Stub _stub;
        private Stub _newStub;
        private Meta _meta;
        private EditorHelper _helper;

        [SetUp]
        public void SetUp()
        {
            _stub = new Stub { Int = 10, Str = "Str", Collection = new List<int> { 1, 2, 3 }, Array = new byte[10] };
            _newStub = new Stub { Collection = new List<int> { 11, 12, 13 } };
            _meta = new Meta { Int = 10, Str = "Str", Collection = new List<int> { 1, 2, 3 }, Array = new byte[10] };
            _helper = new EditorHelper();
        }

        [Test]
        public void CloneThrowsIfObjHasNotMetadataTypeAttr()
        {
            TestDelegate del = () => _helper.Clone(new object());
            Assert.Throws(typeof (ArgumentException), del);
        }

        [Test]
        public void CloneClonesIntProperty()
        {
            var meta = (Meta) _helper.Clone(_stub);
            Assert.AreEqual(_stub.Int, meta.Int);
        }

        [Test]
        public void CloneClonesStringProperty()
        {
            var meta = (Meta)_helper.Clone(_stub);
            Assert.AreSame(_stub.Str, meta.Str);
        }

        [Test]
        public void CloneCopiesGenericCollection()
        {
            var meta = (Meta)_helper.Clone(_stub);
            Assert.AreEqual(_stub.Collection, meta.Collection);
            Assert.AreNotSame(_stub.Collection, meta.Collection);
        }

        [Test]
        public void CloneClonesArrayProperty()
        {
            var meta = (Meta)_helper.Clone(_stub);
            Assert.AreSame(_stub.Array, meta.Array);
        }

        [Test]
        public void UpdateUpdatesIntProperty()
        {
            _helper.Update(_meta, _newStub);
            Assert.AreEqual(_meta.Int, _newStub.Int);
        }

        [Test]
        public void UpdateUpdatesStringProperty()
        {
            _helper.Update(_meta, _newStub);
            Assert.AreSame(_meta.Str, _newStub.Str);
        }

        [Test]
        public void UpdateCopiesGenericCollection()
        {
            _helper.Update(_meta, _newStub);
            Assert.AreEqual(_meta.Collection, _newStub.Collection);
            Assert.AreNotSame(_meta.Collection, _newStub.Collection);
        }

        [Test]
        public void UpdateUpdatesArray()
        {
            _helper.Update(_meta, _newStub);
            Assert.AreSame(_newStub.Array, _meta.Array);
        }

        [Test]
        public void UpdateDoesNotUpdateIfArraysAreSequentiallyEqual()
        {
            _newStub.Array = new byte[_meta.Array.Length];
            for (var i = 0; i < _meta.Array.Length; i++)
            {
                _newStub.Array[i] = _meta.Array[i];
            }
            _newStub.ArrayChanged = false;
            _helper.Update(_meta, _newStub);
            Assert.IsFalse(_newStub.ArrayChanged);
        }
    }
}
