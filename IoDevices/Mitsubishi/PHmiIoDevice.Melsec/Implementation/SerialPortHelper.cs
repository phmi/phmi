using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace PHmiIoDevice.Melsec.Implementation
{
    internal class SerialPortHelper
    {
        private readonly SerialPort _port;
        private readonly int _readRetries;
        private readonly int _delayTime;

        public SerialPortHelper(SerialPort port, int timeout, int messageEndTimeout)
        {
            _port = port;
            _port.WriteTimeout = timeout;
            _port.ReadTimeout = timeout;
            _delayTime = messageEndTimeout;
            _readRetries = timeout/messageEndTimeout;
            if (_readRetries < 1)
                _readRetries = 1;
        }

        public void Open()
        {
            _port.Open();
        }

        public List<byte> Read(int messageLength)
        {
            var tryIndex = 0;
            while (_port.BytesToRead == 0)
            {
                if (++tryIndex > _readRetries)
                    throw new TimeoutException("Timeout when reading");
                Thread.Sleep(_delayTime);
            }
            var result = new List<byte>(messageLength);
            while (_port.BytesToRead > 0 && result.Count < messageLength)
            {
                var length = _port.BytesToRead;
                var message = new byte[length];
                _port.Read(message, 0, length);
                result.AddRange(message);
                Thread.Sleep(_delayTime);
            }
            return result;
        }

        public void Write(byte[] message)
        {
            _port.Write(message, 0, message.Length);
        }

        public void Close()
        {
            _port.Close();
        }
    }
}
