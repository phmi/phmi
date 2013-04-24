using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace PHmiIoDevice.Melsec.Implementation
{
    internal class TcpClientHelper
    {
        private readonly TcpClient _client;
        private readonly string _address;
        private readonly int _port;
        private readonly int _retries;
        private readonly int _delayTime;
        private readonly int _timeout;

        public TcpClientHelper(string address, int port, int timeout, int messageEndTimeout)
        {
            _address = address;
            _port = port;
            _timeout = timeout;
            _client = new TcpClient { ReceiveTimeout = timeout, SendTimeout = timeout};
            _delayTime = messageEndTimeout;
            _retries = timeout/messageEndTimeout;
            if (_retries < 1)
            {
                _retries = 1;
            }
        }

        public void Open()
        {
            var connectionStatus = 0;
            Exception socketException = null;
            _client.BeginConnect(_address, _port, result =>
                {
                    var connected = 0;
                    try
                    {
                        if (_client.Client == null)
                            return;
                        _client.EndConnect(result);
                        connected = 1;
                    }
                    catch (Exception exception)
                    {
                        socketException = exception;
                    }
                    finally
                    {
                        if (connected == 0)
                        {
                            connected = 2;
                        }
                        Interlocked.Exchange(ref connectionStatus, connected);
                    }
                }, null);
            for (var i = 0; i < _retries; i++)
            {
                if (connectionStatus != 0)
                {
                    if (connectionStatus == 1)
                        return;
                    break;
                }
                Thread.Sleep(_delayTime);
            }
            if (socketException != null)
            {
                throw socketException;
            }
            throw new TimeoutException("Connection timeout");
        }

        public List<byte> Read(int messageLength)
        {
            var tryIndex = 0;
            var stream = _client.GetStream();
            while (!stream.DataAvailable)
            {
                if (++tryIndex > _retries)
                {
                    throw new TimeoutException("Timeout when reading");
                }
                Thread.Sleep(_delayTime);
            }
            var result = new List<byte>(messageLength);
            while (stream.DataAvailable && result.Count < messageLength)
            {
                var message = new byte[messageLength];
                var length = stream.Read(message, 0, messageLength);
                for (var i = 0; i < length; ++i)
                {
                    result.Add(message[i]);
                }
                Thread.Sleep(_delayTime);
            }
            return result;
        }

        public void Write(byte[] message)
        {
            _client.GetStream().Write(message, 0, message.Length);
        }

        public void Close()
        {
            _client.Close();
        }
    }
}
