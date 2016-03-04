using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using PHmiClient.Utils.Notifications;
using PHmiIoDeviceTools;
using PHmiModel;
using PHmiModel.Entities;
using PHmiResources.Loc;
using PHmiTools;

namespace PHmiRunner.Utils.IoDeviceRunner
{
    public class IoDeviceRunTarget : IIoDeviceRunTarget
    {
        private readonly IIoDeviceWrapperFactory _wrapperFactory;
        private IIoDeviceWrapper _ioDeviceWrapper;
        private readonly IDictionary<int, DigTagValueHolder> _digValueHolders = new Dictionary<int, DigTagValueHolder>(); 
        private readonly IDictionary<int, NumTagValueHolder> _numValueHolders = new Dictionary<int, NumTagValueHolder>();
        private readonly ReadParameter[] _readParameters;
        private readonly ITagValueHolder[] _readValueHolders;
        private readonly ISet<ITagValueHolder> _valueHoldersToWrite = new HashSet<ITagValueHolder>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly string _filePath;
        private readonly string _options;
        private readonly INotificationReporter _reporter;

        public IoDeviceRunTarget(IoDevice ioDevice, IIoDeviceWrapperFactory wrapperFactory, INotificationReporter reporter)
        {
            _reporter = reporter;
            Name = string.Format(Res.IoDeviceWithName, ioDevice.Name);
            _filePath = GetFilePath(ioDevice.Type);
            _options = ioDevice.Options;
            _wrapperFactory = wrapperFactory;

            var digReadParameters = new List<ReadParameter>();
            var digReadValueHolders = new List<ITagValueHolder>();
            foreach (var t in ioDevice.DigTags)
            {
                var holder = new DigTagValueHolder(t);
                _digValueHolders.Add(t.Id, holder);
                if (t.CanRead)
                {
                    digReadValueHolders.Add(holder);
                    digReadParameters.Add(new ReadParameter(t.Device, typeof(bool)));
                }
            }
            var numReadParameters = new List<ReadParameter>();
            var numReadValueHolders = new List<ITagValueHolder>();
            foreach (var t in ioDevice.NumTags)
            {
                var holder = new NumTagValueHolder(t);
                _numValueHolders.Add(t.Id, holder);
                if (t.CanRead)
                {
                    numReadValueHolders.Add(holder);
                    numReadParameters.Add(new ReadParameter(t.Device, t.TagType));
                }
            }
            _readParameters = digReadParameters.Concat(numReadParameters).ToArray();
            _readValueHolders = digReadValueHolders.Concat(numReadValueHolders).ToArray();
        }

        private static string GetFilePath(string type)
        {
            var folder = Path.GetDirectoryName(Assembly.GetAssembly(typeof (IoDeviceRunTarget)).Location);
            var files = type
                .Split(PHmiConstants.PHmiIoDeviceSeparator)
                .Select(s => PHmiConstants.PHmiIoDevicePrefix + s);
            var pathPieces = new[] {folder, PHmiConstants.PHmiIoDevicesDirectoryName}.Concat(files).ToArray();
            return Path.Combine(pathPieces) + PHmiConstants.PHmiIoDeviceExt;
        }

        public string Name { get; private set; }

        public INotificationReporter Reporter
        {
            get { return _reporter; }
        }

        public void Clean()
        {
            EnterWriteLock();
            try
            {
                foreach (var h in _digValueHolders.Values)
                {
                    h.ClearValue();
                }
                foreach (var h in _numValueHolders.Values)
                {
                    h.ClearValue();
                }
            }
            finally
            {
                ExitWriteLock();
            }
            Exception thrownException = null;
            try
            {
                _ioDeviceWrapper.Dispose();
            }
            catch (Exception exception)
            {
                thrownException = exception;
            }
            _ioDeviceWrapper = null;
            try
            {
                _wrapperFactory.UnloadDomain();
            }
            catch (Exception exception)
            {
                if (thrownException == null)
                    thrownException = exception;
            }
            if (thrownException != null)
                throw thrownException;
        }

        public void Run()
        {
            if (_ioDeviceWrapper == null)
            {
                _ioDeviceWrapper = _wrapperFactory.Create();
                _ioDeviceWrapper.Create(_filePath, _options);
            }
            Write();
            Read();
        }

        public bool? GetDigitalValue(int id)
        {
            DigTagValueHolder holder;
            return _digValueHolders.TryGetValue(id, out holder) ? holder.GetValue() : null;
        }

        public void SetDigitalValue(int id, bool value)
        {
            DigTagValueHolder holder;
            if (!_digValueHolders.TryGetValue(id, out holder))
                return;
            holder.SetValue(value);
            _valueHoldersToWrite.Add(holder);
        }

        public double? GetNumericValue(int id)
        {
            NumTagValueHolder holder;
            return _numValueHolders.TryGetValue(id, out holder) ? holder.GetValue() : null;
        }

        public void SetNumericValue(int id, double value)
        {
            NumTagValueHolder holder;
            if (!_numValueHolders.TryGetValue(id, out holder))
                return;
            holder.SetValue(value);
            _valueHoldersToWrite.Add(holder);
        }

        public void EnterReadLock()
        {
            _lock.EnterReadLock();
        }

        public void ExitReadLock()
        {
            _lock.ExitReadLock();
        }

        public void EnterWriteLock()
        {
            _lock.EnterWriteLock();
        }

        public void ExitWriteLock()
        {
            _lock.ExitWriteLock();
        }

        public void Dispose()
        {
            _lock.Dispose();
        }

        private void Read()
        {
            var result = _ioDeviceWrapper.Read(_readParameters);
            if (result == null)
                throw new Exception("Read result is null");
            if (result.Length != _readValueHolders.Length)
                throw new Exception("Read result count does not match");
            EnterWriteLock();
            try
            {
                for (var i = 0; i < result.Length; i++)
                {
                    var holder = _readValueHolders[i];
                    if (!_valueHoldersToWrite.Contains(holder))
                        holder.Update(result[i]);
                }
            }
            finally
            {
                ExitWriteLock();
            }
        }

        private void Write()
        {
            var parameters = GetWriteParameters();
            if (parameters.Length == 0)
                return;
            _ioDeviceWrapper.Write(parameters);
        }

        private WriteParameter[] GetWriteParameters()
        {
            EnterWriteLock();
            try
            {
                var parameters = _valueHoldersToWrite
                    .Select(h => new WriteParameter(h.Address, h.GetWriteValue()))
                    .ToArray();
                return parameters;
            }
            finally
            {
                _valueHoldersToWrite.Clear();
                ExitWriteLock();
            }
        }
    }
}
